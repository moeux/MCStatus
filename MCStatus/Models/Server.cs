namespace MCStatus.Models;

public class Server
{
    public required string Address { get; init; }
    public required ushort Port { get; init; }
    public required int ProtocolVersion { get; init; }
}