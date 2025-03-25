using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MCStatus.Models;

public class Status
{
    [JsonProperty("version")] public Version? Version { get; set; }

    [JsonProperty("players")] public Players? Players { get; set; }

    [JsonProperty("description")] public JToken? DescriptionRaw { get; set; }

    [JsonIgnore]
    public string? Description
    {
        get
        {
            return DescriptionRaw?.Type switch
            {
                JTokenType.String => DescriptionRaw.ToString(),
                JTokenType.Object when DescriptionRaw["text"] is not null => DescriptionRaw["text"]?.ToString(),
                _ => null
            };
        }
    }

    [JsonProperty("favicon")] public string? FavIcon { get; set; }

    [JsonProperty("enforcesSecureChat")] public bool EnforcesSecureChat { get; set; }

    [JsonProperty("preventsChatReports")] public bool PreventsChatReports { get; set; }

    [JsonProperty("modinfo")] public ModInfo? ModInfo { get; set; }

    [JsonIgnore] public double Ping { get; set; }
}