using System.Net.Sockets;
using System.Text.Json;
using MCStatus.Models;

namespace MCStatus.Utilities;

public static class Pinger
{
    public static async Task<Status?> RequestStatus(Server server)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(server.Address, server.Port);
        await using var stream = client.GetStream();

        await SendHandshake(stream, server.ProtocolVersion, server.Address, server.Port);
        await SendStatusRequest(stream);
        var status = ParseStatusResponse(stream);

        if (status is null) return null;

        await SendPingRequest(stream);
        status.Ping = ParsePingResponse(stream);

        return status;
    }

    private static async Task SendHandshake(NetworkStream stream, int protocolVersion, string address, ushort port)
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

        await stream.WriteAsync(bytes.ToArray());
    }

    private static async Task SendStatusRequest(NetworkStream stream)
    {
        var bytes = new List<byte>();
        // Packet ID 0 for status request
        bytes.AddRange(DataCodec.EncodeVarInt(0));
        // Prefix with total packet length
        bytes.InsertRange(0, DataCodec.EncodeVarInt(bytes.Count));

        await stream.WriteAsync(bytes.ToArray());
    }

    private static async Task SendPingRequest(NetworkStream stream)
    {
        var bytes = new List<byte>();

        // Packet ID 1 for ping request
        bytes.AddRange(DataCodec.EncodeVarInt(1));
        // Ping Request Payload Bytes - Send current Timestamp in order to calculate the latency
        bytes.AddRange(BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
        // Prefix with total packet length
        bytes.InsertRange(0, DataCodec.EncodeVarInt(bytes.Count));

        await stream.WriteAsync(bytes.ToArray());
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