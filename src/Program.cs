namespace PeterRosser.GitTools.FindLargeFiles;

using System;
using System.Text.Json;

using static GitHelper;

internal static class Program
{
    private const string ToolVersion = "0.0.1"; // Update as needed

    private static string FormatSize(long size)
    {
        foreach (string unit in new[] { "B", "KiB", "MiB", "GiB" })
        {
            if (size < 1024)
            {
                return $"{size:0.#} {unit}";
            }

            size /= 1024;
        }

        return $"{size:0.#} TiB";
    }

    private static int Main(string[] args)
    {
        const long defaultSizeThresholdMiB = 10;
        long? sizeThresholdMiB = null;
        string? outputFile = null;
        bool showHelp = false;
        bool showVersion = false;

        int startIndex = 0;
        if (!sizeThresholdMiB.HasValue && args.Length > 0 && long.TryParse(args[0], out long sizeArg))
        {
            sizeThresholdMiB = sizeArg;
            startIndex = 1; // Skip the first argument since it's the size threshold
        }

        for (int i = startIndex; i < args.Length; i++)
        {
            string arg = args[i].Trim().ToLowerInvariant();
            if (arg is "--help" or "-h")
            {
                showHelp = true;
            }
            else if (arg is "--version" or "-v")
            {
                showVersion = true;
            }
            else if (arg == "--size-threshold" && i + 1 < args.Length && long.TryParse(args[++i], out long size))
            {
                sizeThresholdMiB = size * 1024 * 1024; // Convert MiB to bytes
            }
            else if ((arg == "--output" || arg == "-o") && i + 1 < args.Length)
            {
                outputFile = args[++i];
            }
        }

        if (showVersion)
        {
            Console.WriteLine(ToolVersion);
            return 0;
        }

        if (showHelp)
        {
            Console.Error.WriteLine($"""
                Usage: git-find-large-files [OPTIONS]
                Options:
                 --size-threshold <size>  Set the size threshold for large files in MiB (default: 10 MiB).
                 --help                   Show this help message
                 --version, -v            Show version information
                 -o, --output <file>      Output the results to a file (default: stdout)

                Usage: git-find-large-files <size> [OPTIONS]
                Arguments:
                size - The size threshold for large files in MiB (default: 10 MiB).
                """);
            return 2;
        }

        sizeThresholdMiB ??= defaultSizeThresholdMiB;

        Console.Error.WriteLine($"""
            🚀 Finding ALL files larger than {sizeThresholdMiB}MiB in entire Git history...
               This includes files that may have been deleted or moved

            """);

        // Step 1: Get all large blobs with paths
        Dictionary<string, BlobInfo> largeBlobs = GetAllLargeBlobsWithPaths(sizeThresholdMiB.Value * 1024 * 1024);

        if (largeBlobs.Count == 0)
        {
            Console.Error.WriteLine("✅ No large files found in the repository history.");

            // Still output empty JSON
            string resultJson = "[]";
            if (outputFile is not null)
            {
                Console.Error.WriteLine($"💾 Empty results saved to {outputFile}");
                File.WriteAllText(outputFile, resultJson);
            }
            else
            {
                Console.WriteLine(resultJson);
            }

            return 0;
        }

        // Step 2: Find introducing commits
        Dictionary<string, IReadOnlyList<CommitAndFile>> blobToCommits = FindIntroducingCommits(largeBlobs);

        // Step 3: Get commit info and branches
        HashSet<string> allCommits = [.. blobToCommits.Values.SelectMany(c => c.Select(cf => cf.Commit))];
        Dictionary<string, Dictionary<string, string>> commitInfo = GetCommitInfoBatch(allCommits);
        Dictionary<string, IReadOnlyList<string>> commitToBranches = GetRemoteBranchesBatch(allCommits);

        // Step 4: Build the results
        List<ResultEntry> results = [];
        foreach ((string blobHash, IReadOnlyList<CommitAndFile> commitFileSizes) in blobToCommits)
        {
            foreach ((string commit, string fileName, long size) in commitFileSizes)
            {
                IReadOnlyList<string> branches = commitToBranches.GetValueOrDefault(commit, []);
                Dictionary<string, string> info = commitInfo.GetValueOrDefault(commit, new() { ["author_email"] = "unknown", ["author_date"] = "unknown" });
                results.Add(new(
                    fileName,
                    size,
                    FormatSize(size),
                    blobHash,
                    commit,
                    info["author_email"],
                    info["author_date"],
                    [.. branches]
                ));
            }
        }

        // Step 5: Sort results
        results.Sort((a, b) => b.Size.CompareTo(a.Size)); // Sort by size descending

        // Step 6: Output results
        if (outputFile is not null)
        {
            using FileStream fs = new(outputFile, FileMode.Create, FileAccess.Write, FileShare.None);
            using Utf8JsonWriter writer = new(fs, new JsonWriterOptions { Indented = true });
            JsonSerializer.Serialize(writer, results, AppJsonContext.Default.ListResultEntry);
            Console.Error.WriteLine($"💾 Results saved to {outputFile}");
        }
        else
        {
            string jsonString = JsonSerializer.Serialize(results, AppJsonContext.Default.ListResultEntry);
            Console.WriteLine(jsonString);
        }

        return 0;
    }
}
