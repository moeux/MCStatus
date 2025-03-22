using Discord;
using Discord.WebSocket;

namespace MCStatus.Services;

public class AutocompleteService(StatusQueryService service)
{
    public async Task AutocompleteStatusCommand(SocketAutocompleteInteraction interaction)
    {
        if (interaction.Data.CommandName is not "status") return;

        var input = interaction.Data.Current.Value.ToString() ?? string.Empty;
        var previousRequests = service.GetPreviousRequests(interaction.User.Id)
            .Where(server => server.Address.Contains(input, StringComparison.InvariantCultureIgnoreCase))
            .Take(25)
            .SelectMany(server => interaction.Data.Current.Name is "server"
                ? (AutocompleteResult[]) [new AutocompleteResult(server.Address, server.Address)]
                : (AutocompleteResult[]) [new AutocompleteResult(server.Port.ToString(), server.Port.ToString())]);

        await interaction.RespondAsync(previousRequests);
    }
}