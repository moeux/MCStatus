using System.Text.Json.Serialization;

namespace MCStatus.Models;

public class Player(string name, Guid id)
{
    [JsonPropertyName("name")] public required string Name { get; init; } = name;

    [JsonPropertyName("id")] public required Guid Id { get; init; } = id;
}