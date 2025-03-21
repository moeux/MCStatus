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
            .Select(option => option.Value)
            .Cast<string>()
            .First();
        var port = command.Data.Options
            .Where(option => option.Name is "port")
            .Select(option => option.Value)
            .Cast<ushort>()
            .FirstOrDefault((ushort)25565);

        if (string.IsNullOrWhiteSpace(ipOrDomain))
        {
            if (command.UserLocale is "de-DE")
                await command.RespondAsync("Bitte gib eine gültige IP oder Domain an!",
                    options: GetRequestOptions(cancellationToken));
            else
                await command.RespondAsync("Please specify a valid IP or domain!",
                    options: GetRequestOptions(cancellationToken));
            return;
        }

        if (port < 1024)
        {
            if (command.UserLocale is "de-DE")
                await command.RespondAsync("Nur Ports größer 1023 sind erlaubt!",
                    options: GetRequestOptions(cancellationToken));
            else
                await command.RespondAsync("Only ports greater than 1023 are allowed!",
                    options: GetRequestOptions(cancellationToken));
            return;
        }

        var status = await service.RequestStatusAsync(new Server
        {
            Address = ipOrDomain,
            Port = port,
            ProtocolVersion = 769
        }, cancellationToken);

        if (status is null)
        {
            if (command.UserLocale is "de-DE")
                await command.RespondAsync("Der Server ist leider nicht erreichbar.",
                    options: GetRequestOptions(cancellationToken));
            else
                await command.RespondAsync("The server is unfortunately not reachable.",
                    options: GetRequestOptions(cancellationToken));
            return;
        }

        var embed = new EmbedBuilder();
        var fileName = $"{Guid.NewGuid()}.png";
        var bytes = Convert.FromBase64String(status.FavIcon);
        using var stream = new MemoryStream(bytes);
        using var fileAttachment = new FileAttachment(stream, fileName, isThumbnail: true);

        embed.WithColor(GetGradientColor(status.Ping));
        embed.WithCurrentTimestamp();
        embed.WithThumbnailUrl($"attachment://{fileName}");
        embed.AddField("Description", status.Description.Text);
        embed.AddField("Online Players", status.Players.Online);
        embed.AddField("Max Players", status.Players.Max, true);
        embed.AddField("Version", status.Version.ToString(), true);
        embed.AddField("Ping", $"{status.Ping:F2}", true);
        embed.AddField("Enforces Secure Chat?", status.EnforcesSecureChat ? "\u2705" : "\u274c");
        embed.AddField("Prevents Chat Reports?", status.PreventsChatReports ? "\u2705" : "\u274c", true);

        if (status.ModInfo is not null)
        {
            embed.AddField("Modloader Type", status.ModInfo.Type);
            embed.AddField("Mods Count", status.ModInfo.ModList.Count(), true);
        }

        await command.RespondWithFileAsync(
            fileAttachment, embed: embed.Build(), options: GetRequestOptions(cancellationToken));
    }

    private static Color GetGradientColor(double ping)
    {
        var ratio = Math.Max(0, Math.Min(200, ping)) / 200.0;
        var red = (int)(ratio * 255);
        var green = (int)((1 - ratio) * 255);

        return new Color(red, green, 0);
    }

    private static RequestOptions GetRequestOptions(CancellationToken cancellationToken)
    {
        return new RequestOptions { CancelToken = cancellationToken };
    }
}