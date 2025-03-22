using AutoCommand.Handler;
using Discord;
using Discord.WebSocket;
using MCStatus.Models;
using MCStatus.Services;
using Serilog;
using Color = Discord.Color;

namespace MCStatus.Commands;

public class StatusCommand(StatusQueryService service) : ICommandHandler
{
    public string CommandName => "status";

    public async Task HandleAsync(ILogger logger, SocketSlashCommand command,
        CancellationToken cancellationToken = default)
    {
        var ipOrDomain = command.Data.Options
            .Where(option => option.Name is "server")
            .Select(option => Convert.ToString(option.Value))
            .First();
        var port = command.Data.Options
            .Where(option => option.Name is "port")
            .Select(option => Convert.ToUInt16(option.Value))
            .FirstOrDefault((ushort)25565);

        if (string.IsNullOrWhiteSpace(ipOrDomain))
        {
            await Respond(
                command,
                "Please specify a valid IP or domain!",
                "Bitte gib eine gültige IP oder Domain an!",
                cancellationToken
            );
            return;
        }

        if (port < 1024)
        {
            await Respond(
                command,
                "Only ports greater than 1023 are allowed!",
                "Nur Ports größer 1023 sind erlaubt!",
                cancellationToken
            );
            return;
        }

        await command.DeferAsync(true, GetRequestOptions(cancellationToken));

        try
        {
            var status = await service.RequestStatusAsync(new Server
            {
                Address = ipOrDomain,
                Port = port,
                ProtocolVersion = 769
            }, cancellationToken);

            if (status is null)
            {
                await Followup(
                    command,
                    "The server is unfortunately not reachable.",
                    "Der Server ist leider nicht erreichbar.",
                    cancellationToken
                );
                return;
            }

            // The FavIcon string is prefixed with data:image/png;base64,<data>, so we create a substring first
            var bytes = Convert.FromBase64String(status.FavIcon[(status.FavIcon.LastIndexOf(',') + 1)..]);
            using var stream = new MemoryStream(bytes);
            using var fileAttachment = new FileAttachment(stream, $"{Guid.NewGuid()}.png", isThumbnail: true);

            await command.FollowupWithFileAsync(
                fileAttachment,
                embed: CreateEmbed(ipOrDomain, port, fileAttachment.FileName, status),
                options: GetRequestOptions(cancellationToken)
            );
        }
        catch (Exception)
        {
            await Followup(
                command,
                "Unfortunately this did not work, please check your input!",
                "Das hat leider nicht funktioniert, bitte überprüfe deine Angaben!",
                cancellationToken
            );
        }
    }

    private static RequestOptions GetRequestOptions(CancellationToken cancellationToken)
    {
        return new RequestOptions { CancelToken = cancellationToken };
    }

    private static Color GetGradientColor(double ping)
    {
        var ratio = Math.Max(0, Math.Min(120, ping)) / 120.0;
        var red = (int)(ratio * 255);
        var green = (int)((1 - ratio) * 255);

        return new Color(red, green, 0);
    }

    private static async Task Respond(
        SocketSlashCommand command,
        string defaultText,
        string localizedText,
        CancellationToken cancellationToken = default)
    {
        if (command.UserLocale is "de")
            await command.RespondAsync(
                localizedText,
                ephemeral: true,
                options: GetRequestOptions(cancellationToken)
            );
        else
            await command.RespondAsync(
                defaultText,
                ephemeral: true,
                options: GetRequestOptions(cancellationToken)
            );
    }

    private static async Task Followup(
        SocketSlashCommand command,
        string defaultText,
        string localizedText,
        CancellationToken cancellationToken = default)
    {
        if (command.UserLocale is "de")
            await command.FollowupAsync(
                localizedText,
                ephemeral: true,
                options: GetRequestOptions(cancellationToken)
            );
        else
            await command.FollowupAsync(
                defaultText,
                ephemeral: true,
                options: GetRequestOptions(cancellationToken)
            );
    }

    private static Embed CreateEmbed(string ipOrDomain, ushort port, string fileName, Status status)
    {
        var embed = new EmbedBuilder();

        embed.WithTitle($"{ipOrDomain}:{port}")
            .WithColor(GetGradientColor(status.Ping))
            .WithCurrentTimestamp()
            .WithThumbnailUrl($"attachment://{fileName}")
            .AddField("Description", status.Description.Text)
            .AddField("Online Players", status.Players.Online, true)
            .AddField("Max Players", status.Players.Max, true)
            // Empty field for layout purposes
            .AddField("\u200B", "\u200B", true)
            .AddField("Version", status.Version.ToString(), true)
            .AddField("Ping", $"{status.Ping:F2} ms", true)
            // Empty field for layout purposes
            .AddField("\u200B", "\u200B", true)
            .AddField("Enforces Secure Chat?", status.EnforcesSecureChat ? "\u2705" : "\u274c", true)
            .AddField("Prevents Chat Reports?", status.PreventsChatReports ? "\u2705" : "\u274c", true);

        if (status.ModInfo is not null)
        {
            embed.AddField("Modloader Type", status.ModInfo.Type);
            embed.AddField("Mods Count", status.ModInfo.ModList.Count(), true);
        }

        return embed.Build();
    }
}