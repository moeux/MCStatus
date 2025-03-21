namespace MCStatus.Configuration;

public class DiscordBotOptions
{
    public required string Token { get; set; }
    public required bool RegisterCommands { get; set; }
    public required string CommandPath { get; set; }
}