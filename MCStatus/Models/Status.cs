using System.Text.Json.Serialization;

namespace MCStatus.Models;

public class Status(
    Version version,
    Players players,
    Description description,
    string favIcon,
    bool enforcesSecureChat,
    bool preventsChatReports,
    ModInfo? modInfo = null)
{
    [JsonPropertyName("version")] public required Version Version { get; init; } = version;

    [JsonPropertyName("players")] public required Players Players { get; init; } = players;

    [JsonPropertyName("description")] public required Description Description { get; init; } = description;

    [JsonPropertyName("favicon")] public required string FavIcon { get; init; } = favIcon;

    [JsonPropertyName("enforcesSecureChat")]
    public required bool EnforcesSecureChat { get; init; } = enforcesSecureChat;

    [JsonPropertyName("preventsChatReports")]
    public required bool PreventsChatReports { get; init; } = preventsChatReports;

    [JsonPropertyName("modinfo")] public ModInfo? ModInfo { get; init; } = modInfo;

    [JsonIgnore] public long Ping { get; set; }
}