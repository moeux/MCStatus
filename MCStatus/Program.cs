using System.Text.Json;
using MCStatus.Models;
using MCStatus.Utilities;

namespace MCStatus;

internal static class Program
{
    private static async Task Main()
    {
        var server = new Server
        {
            Address = "3uuwubnrz2utgcsf.myfritz.net",
            Port = 25565,
            ProtocolVersion = 763
        };

        var status = await Pinger.RequestStatus(server);

        if (status is null)
        {
            await Console.Error.WriteLineAsync("Server returned null.");
            return;
        }

        Console.WriteLine(JsonSerializer.Serialize(status, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Ping: {status.Ping}");
    }
}