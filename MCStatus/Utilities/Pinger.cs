using System.Net.Sockets;
using System.Text.Json;
using MCStatus.Models;

namespace MCStatus.Utilities;

public static class Pinger
{
    public static async Task<Status?> RequestStatusAsync(Server server, CancellationToken cancellationToken = default)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(server.Address, server.Port, cancellationToken);
        await using var stream = client.GetStream();

        await SendHandshakeAsync(stream, server.ProtocolVersion, server.Address, server.Port, cancellationToken);
        await SendStatusRequestAsync(stream, cancellationToken);
        var status = ParseStatusResponse(stream);

        if (status is null) return null;

        await SendPingRequestAsync(stream, cancellationToken);
        var start = DateTimeOffset.FromUnixTimeMilliseconds(ParsePingResponse(stream));
        var latency = DateTimeOffset.UtcNow.Subtract(start);
        status.Ping = latency.TotalMilliseconds;

        return status;
    }

    private static async Task SendHandshakeAsync(
        NetworkStream stream,
        int protocolVersion,
        string address,
        ushort port,
        CancellationToken cancellationToken = default)
    {
        var bytes = new List<byte>();

        // Packet ID 0
        bytes.AddRange(DataCodec.EncodeVarInt(0));
        // Handshake Payload Bytes
        bytes.AddRange(DataCodec.EncodeVarInt(protocolVersion));
        bytes.AddRange(DataCodec.EncodeString(address));
        bytes.AddRange(BitConverter.GetBytes(port));
        bytes.AddRange(DataCodec.EncodeVarInt(1)); // State 1 = Status Query
        // Prefix with total packet length
        bytes.InsertRange(0, DataCodec.EncodeVarInt(bytes.Count));

        await stream.WriteAsync(bytes.ToArray(), cancellationToken);
    }

    private static async Task SendStatusRequestAsync(NetworkStream stream,
        CancellationToken cancellationToken = default)
    {
        var bytes = new List<byte>();
        // Packet ID 0 for status request
        bytes.AddRange(DataCodec.EncodeVarInt(0));
        // Prefix with total packet length
        bytes.InsertRange(0, DataCodec.EncodeVarInt(bytes.Count));

        await stream.WriteAsync(bytes.ToArray(), cancellationToken);
    }

    private static async Task SendPingRequestAsync(NetworkStream stream, CancellationToken cancellationToken = default)
    {
        var bytes = new List<byte>();

        // Packet ID 1 for ping request
        bytes.AddRange(DataCodec.EncodeVarInt(1));
        // Ping Request Payload Bytes - Send current Timestamp in order to calculate the latency
        bytes.AddRange(BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
        // Prefix with total packet length
        bytes.InsertRange(0, DataCodec.EncodeVarInt(bytes.Count));

        await stream.WriteAsync(bytes.ToArray(), cancellationToken);
    }

    private static Status? ParseStatusResponse(NetworkStream stream)
    {
        var packetLength = DataCodec.DecodeVarInt(stream);
        var packetId = DataCodec.DecodeVarInt(stream);
        var payloadLength = DataCodec.DecodeVarInt(stream);
        var payload = DataCodec.DecodeString(stream, payloadLength);

        return JsonSerializer.Deserialize<Status>(payload);
    }

    private static long ParsePingResponse(NetworkStream stream)
    {
        var buffer = new byte[8];
        var packetLength = DataCodec.DecodeVarInt(stream);
        var packetId = DataCodec.DecodeVarInt(stream);

        stream.ReadExactly(buffer);

        return BitConverter.ToInt64(buffer, 0);
    }
}