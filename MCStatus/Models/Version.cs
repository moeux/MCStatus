using System.Text.Json.Serialization;

namespace MCStatus.Models;

public class Version(string name, int protocol)
{
    [JsonPropertyName("name")] public required string Name { get; init; } = name;

    [JsonPropertyName("protocol")] public required int Protocol { get; init; } = protocol;
}