using System.Text.Json.Serialization;

namespace MCStatus.Models;

public class Description(string text)
{
    [JsonPropertyName("text")] public required string Text { get; init; } = text;
}