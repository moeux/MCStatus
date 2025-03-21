namespace MCStatus.Configuration;

public static class ConfKeys
{
    public const string Prefix = "MCStatus";

    public static class Discord
    {
        public const string Token = $"{Prefix}:Discord:Token";
        public const string RegisterCommands = $"{Prefix}:Discord:RegisterCommands";
        public const string CommandPath = $"{Prefix}:Discord:CommandPath";
    }
}