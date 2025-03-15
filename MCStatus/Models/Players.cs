using System.Text.Json.Serialization;

namespace MCStatus.Models;

public class Players(int max, int online, IEnumerable<Player>? sample = null)
{
    [JsonPropertyName("max")] public required int Max { get; init; } = max;

    [JsonPropertyName("online")] public required int Online { get; init; } = online;

    [JsonPropertyName("sample")] public IEnumerable<Player>? Sample { get; init; } = sample;
}