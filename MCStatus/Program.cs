using AutoCommand.Handler;
using MCStatus.Commands;
using MCStatus.Configuration;
using MCStatus.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MCStatus;

internal static class Program
{
    private static async Task Main()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Configuration.AddEnvironmentVariables($"{ConfKeys.Prefix}__");

        builder.Services
            .AddMemoryCache()
            .AddOptionsWithValidateOnStart<DiscordBotOptions>()
            .Configure(options =>
            {
                options.Token = builder.Configuration[ConfKeys.Discord.Token] ?? string.Empty;
                options.RegisterCommands = bool.Parse(
                    builder.Configuration[ConfKeys.Discord.RegisterCommands] ?? bool.FalseString);
                options.CommandPath =
                    Path.GetFullPath(builder.Configuration[ConfKeys.Discord.CommandPath] ?? string.Empty);
            })
            .Validate(options => !string.IsNullOrWhiteSpace(options.Token), "Discord bot token is required.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.CommandPath) && Directory.Exists(options.CommandPath),
                "Discord bot command directory is required.");

        builder.Services.AddHostedService<DiscordBotService>();
        builder.Services.AddSingleton<StatusQueryService>();
        builder.Services.AddSingleton<AutocompleteService>();
        builder.Services.AddScoped<ICommandHandler, StatusCommand>();

        var host = builder.Build();
        await host.RunAsync();
    }
}