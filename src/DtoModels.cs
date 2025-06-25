namespace PeterRosser.GitTools.FindLargeFiles;

public record BlobInfo(long Size, IReadOnlySet<string> Paths);
public record CommitAndFile(string Commit, string Path, long Size);
public record ResultEntry(string FileName, long Size, string SizeFormatted, string BlobHash, string Commit, string AuthorEmail, string AuthorDate, string[] Branches);