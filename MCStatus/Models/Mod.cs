using System.Text.Json.Serialization;

namespace MCStatus.Models;

public class Mod(string modId, string version)
{
    [JsonPropertyName("modid")] public required string ModId { get; init; } = modId;

    [JsonPropertyName("version")] public required string Version { get; init; } = version;
}