namespace PeterRosser.GitTools.FindLargeFiles;
using System.Text.Json.Serialization;

// JSON Source Generation Context for AOT compatibility
[JsonSerializable(typeof(List<ResultEntry>))]
[JsonSerializable(typeof(ResultEntry))]
[JsonSerializable(typeof(string[]))]
[JsonSourceGenerationOptions(
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
internal partial class AppJsonContext : JsonSerializerContext
{
}
