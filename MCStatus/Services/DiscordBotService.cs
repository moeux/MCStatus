using AutoCommand.Handler;
using AutoCommand.Utils;
using Discord;
using Discord.WebSocket;
using MCStatus.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MCStatus.Services;

public class DiscordBotService(
    IServiceScopeFactory serviceScopeFactory,
    AutocompleteService autocompleteService,
    IOptions<DiscordBotOptions> options,
    ILogger<DiscordBotService> logger) : BackgroundService
{
    private readonly DefaultCommandRouter _commandRouter = new();
    private DiscordSocketClient _client = null!;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.None
        });

        _client.Log += Log;
        _client.Ready += () => RegisterCommandsAsync(stoppingToken);
        _client.AutocompleteExecuted += autocompleteService.AutocompleteStatusCommand;

        await _client.LoginAsync(TokenType.Bot, options.Value.Token);
        await _client.StartAsync();
        await Task.Delay(Timeout.Infinite, stoppingToken);
        await _client.StopAsync();
        await _client.LogoutAsync();
    }

    private Task Log(LogMessage message)
    {
        switch (message.Severity)
        {
            case LogSeverity.Critical:
                logger.LogCritical(message.ToString());
                break;
            case LogSeverity.Error:
                logger.LogError(message.ToString());
                break;
            case LogSeverity.Warning:
                logger.LogWarning(message.ToString());
                break;
            case LogSeverity.Info:
                logger.LogInformation(message.ToString());
                break;
            case LogSeverity.Verbose:
                logger.LogTrace(message.ToString());
                break;
            case LogSeverity.Debug:
                logger.LogDebug(message.ToString());
                break;
        }

        return Task.CompletedTask;
    }

    private async Task RegisterCommandsAsync(CancellationToken cancellationToken)
    {
        if (options.Value.RegisterCommands)
        {
            logger.LogInformation("Registering commands from '{CommandPath}'", options.Value.CommandPath);

            var deletionTasks =
                (await _client.GetGlobalApplicationCommandsAsync(options: GetRequestOptions(cancellationToken)))
                .Select(command => command.DeleteAsync(GetRequestOptions(cancellationToken)))
                .ToArray();
            await Task.WhenAll(deletionTasks);

            if (deletionTasks.Length > 0)
            {
                var successes = deletionTasks.Count(task => task.IsCompletedSuccessfully);
                logger.LogInformation("Successfully deleted {DeletionCount} out of {TotalCommands} global comannds",
                    successes, deletionTasks.Length);
            }
            else
            {
                logger.LogInformation("No pre-existing global commands retrieved");
            }

            await _client.CreateSlashCommandsAsync(options.Value.CommandPath, cancellationToken);
        }

        using var scope = serviceScopeFactory.CreateScope();
        var commandHandlers = scope.ServiceProvider.GetServices<ICommandHandler>();

        foreach (var commandHandler in commandHandlers) _commandRouter.Register(commandHandler);

        _client.SlashCommandExecuted += command => _commandRouter.HandleAsync(command, cancellationToken);
    }

    private static RequestOptions GetRequestOptions(CancellationToken cancellationToken)
    {
        return new RequestOptions
        {
            CancelToken = cancellationToken
        };
    }
}