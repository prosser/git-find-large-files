namespace PeterRosser.GitTools.FindLargeFiles;

using System;

using static ConsoleHelper;
using static ProcessHelper;

internal static class GitHelper
{
    public static Dictionary<string, IReadOnlyList<CommitAndFile>> FindIntroducingCommits(
        Dictionary<string, BlobInfo> largeBlobs)
    {
        Console.Error.WriteLine("🔄 Finding introducing commits for large files...");
        Dictionary<string, IReadOnlyList<CommitAndFile>> blobToCommits = [];
        HashSet<string> uniquePaths = [.. largeBlobs.SelectMany(kv => kv.Value.Paths)];

        Dictionary<string, string> pathToCommit = [];

        Console.Error.WriteLine($"   Analyzing {uniquePaths.Count} unique file paths...");
        foreach ((string path, int i) in uniquePaths.Select((x, i) => (x, i)))
        {
            if ((i + 1) % 50 == 0)
            {
                PrintProgress($"   Finding commits... {i + 1}/{uniquePaths.Count}");
            }

            List<string>? output = RunGitCommand(["log", "--all", "--follow", "--diff-filter=A", "--format=%H", "--max-count=1", "--", path]);

            if (output is null || output.Count == 0)
            {
                continue; // No commit found for this path
            }

            string commitHash = output[0].Trim();
            if (!string.IsNullOrWhiteSpace(commitHash) && (commitHash.Length is 40 or 64)) // SHA-1 or SHA-256
            {
                pathToCommit[path] = commitHash;
            }
        }

        PrintProgress("");
        Console.Error.WriteLine($"   Found introducing commits for {pathToCommit.Count} paths");

        // map commits to blobs
        foreach ((string blobHash, BlobInfo blobInfo) in largeBlobs)
        {
            List<CommitAndFile> commitsAndFiles = [];
            foreach (string path in blobInfo.Paths)
            {
                if (pathToCommit.TryGetValue(path, out string? commit))
                {
                    commitsAndFiles.Add(new(commit, path, blobInfo.Size));
                }
            }

            if (commitsAndFiles.Count > 0)
            {
                blobToCommits[blobHash] = commitsAndFiles;
            }
            else
            {
                // record no commits found for this blob, but still include it
                blobToCommits[blobHash] = [new("unknown", $"unknown-blob-{blobHash[..7]}", blobInfo.Size)];
            }
        }

        Console.Error.WriteLine($"   Found {blobToCommits.Count} introducing commits for large blobs");
        return blobToCommits;
    }

    public static Dictionary<string, BlobInfo> GetAllLargeBlobsWithPaths(long sizeThreshold)
    {
        Console.Error.WriteLine($"🔍 Finding all large blobs (>{sizeThreshold / (1024 * 1024)}MiB) with paths...");
        Dictionary<string, BlobInfo> largeBlobs = [];
        Dictionary<string, HashSet<string>> blobsToPaths = [];

        // Step 1: Get all objects with paths from entire history
        Console.Error.WriteLine("   Getting all objects and paths from history...");
        List<string>? outputLines = RunGitCommand(["rev-list", "--objects", "--all"]);
        if (outputLines is null)
        {
            return largeBlobs;
        }

        for (int i = 0; i < outputLines.Count; i++)
        {
            string line = outputLines[i].Trim();
            if (i % 1000 == 0)
            {
                PrintProgress($"   Processing history... {i + 1}/{outputLines.Count}");
            }

            if (line.Length == 0)
            {
                continue;
            }

            string[] parts = line.Split(' ', 2);
            if (parts.Length < 2)
            {
                continue; // Skip malformed lines
            }

            (string hash, string path) = (parts[0], parts[1]);
            if (hash.Length is 40 or 64) // SHA-1 or SHA-256
            {
                if (!blobsToPaths.TryGetValue(hash, out HashSet<string>? paths))
                {
                    paths = [];
                    blobsToPaths[hash] = paths;
                }

                _ = paths.Add(path);
            }
        }

        PrintProgress("");
        Console.Error.WriteLine($"   Found {blobsToPaths.Count} objects with paths");

        // Step 2: Check sizes for these objects in batches
        Console.Error.WriteLine("   Checking object sizes...");
        int blobCount = 0;

        foreach ((string[] batch, int i) in blobsToPaths.Keys.Chunk(1000).Select((x, i) => (x, i)))
        {
            PrintProgress($"   Checking sizes... {i + 1}/{(blobsToPaths.Count + 999) / 1000}");
            List<string>? batchOutput = RunGitCommand(["cat-file", "--batch-check=%(objecttype) %(objectsize)"], batch);

            if (batchOutput == null)
            {
                continue; // Skip this batch if there was an error
            }

            // Process the batch output to find large blobs
            for (int j = 0; j < batchOutput.Count; j++)
            {
                string line = batchOutput[j].Trim();

                string[] parts = line.Split(' ', 2);
                if (parts.Length >= 2 && parts[0] == "blob")
                {
                    blobCount++;

                    if (long.TryParse(parts[1], out long size) && size >= sizeThreshold)
                    {
                        string objHash = batch[j].Trim();
                        HashSet<string> paths = blobsToPaths[objHash];
                        largeBlobs[objHash] = new(size, paths);
                    }
                }
            }
        }

        PrintProgress("");
        Console.Error.WriteLine($"   Scanned {blobCount} blobs, found {largeBlobs.Count} large ones");

        return largeBlobs;
    }

    public static Dictionary<string, Dictionary<string, string>> GetCommitInfoBatch(IReadOnlySet<string> commits)
    {
        Console.Error.WriteLine("📅 Getting commit information...");

        Dictionary<string, Dictionary<string, string>> commitInfo = new()
        {
            ["unknown"] = new() { ["author_email"] = "unknown", ["author_date"] = "unknown" }
        };

        var validCommits = commits.Where(c => c.Length is 40 or 64).ToHashSet();

        if (validCommits.Count == 0)
        {
            return commitInfo;
        }

        foreach (string? commit in validCommits)
        {
            List<string>? output = RunGitCommand(["log", "--format=%ae|%ad", "--date=iso", "-1", commit]);
            if (output is null || output.Count == 0)
            {
                Console.Error.WriteLine($"   Warning: No commit info found for {commit}");
                continue;
            }

            (string email, string date) = output[0].Trim().Split('|', 2) switch
            {
                [string e, string d] => (e, d),
                _ => ("unknown", "unknown")
            };

            commitInfo[commit] = new Dictionary<string, string>
            {
                ["author_email"] = email,
                ["author_date"] = date
            };
        }

        Console.Error.WriteLine($"   Got info for {commitInfo.Count} commits");
        return commitInfo;
    }

    public static Dictionary<string, IReadOnlyList<string>> GetRemoteBranchesBatch(HashSet<string> commits)
    {
        Console.Error.WriteLine("🌐 Finding remote branches...");
        Dictionary<string, IReadOnlyList<string>> commitToBranches = [];

        // For efficiency, only check a sample of commits if there are too many
        Console.Error.WriteLine($"   Checking branches for {commits.Count} commits (this may take a moment)...");
        HashSet<string> sampleCommits = commits.Count > 20
            ? [.. commits.OrderBy(_ => Guid.NewGuid()).Take(20)]
            : commits;

        foreach ((string commit, int i) in sampleCommits.Select((c, i) => (c, i)))
        {
            if ((i + 1) % 10 == 0)
            {
                PrintProgress($"   Checking branches... {i + 1}/{sampleCommits.Count}");
            }

            List<string>? output = RunGitCommand(["branch", "--remote", "--contains", commit]);
            if (output is null || output.Count == 0)
            {
                continue; // No branches found for this commit
            }

            // Filter and clean branch names
            string[] branches = [..output
                .Select(b => b.Trim().Replace("origin/", "")) // Remove 'origin/' prefix
                .Where(b => !string.IsNullOrWhiteSpace(b)) // Remove empty lines
                ];
            if (branches.Length > 0)
            {
                commitToBranches[commit] = branches;
            }
        }

        if (commits.Count > sampleCommits.Count)
        {
            List<string> commonBranches = [];
            if (sampleCommits.Count > 0 && commits.Count > 0)
            {
                // find most common branches
                HashSet<string> allBranches = [.. commitToBranches.Values.SelectMany(b => b)];

                if (allBranches.Count > 0)
                {
                    var branchCounts = allBranches
                        .GroupBy(b => b)
                        .ToDictionary(g => g.Key, g => g.Count());

                    commonBranches = [.. branchCounts
                        .OrderByDescending(kv => kv.Value)
                        .Take(3) // Get top 3 most common branches
                        .Select(kv => kv.Key)];
                }
            }

            foreach (string commit in commits.Where(x => !commitToBranches.ContainsKey(x)))
            {
                commitToBranches[commit] = commonBranches;
            }
        }

        commitToBranches["unknown"] = [];
        PrintProgress("");
        Console.Error.WriteLine($"   Mapped branches for {commitToBranches.Count} commits");

        return commitToBranches;
    }
}