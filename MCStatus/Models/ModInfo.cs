using System.Text.Json.Serialization;

namespace MCStatus.Models;

public class ModInfo(string type, IEnumerable<Mod> modList)
{
    [JsonPropertyName("type")] public required string Type { get; init; } = type;

    [JsonPropertyName("modList")] public required IEnumerable<Mod> ModList { get; init; } = modList;
}