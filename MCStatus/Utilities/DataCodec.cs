using System.Text;

namespace MCStatus.Utilities;

public static class DataCodec
{
    private const int ContinueBit = 0x80;
    private const int SegmentBits = 0x7F;

    public static IEnumerable<byte> EncodeVarInt(int value)
    {
        var bytes = new List<byte>();

        // Split up bytes into groups of 7 bits and attach a continue-bit between them
        while ((value & ContinueBit) != 0)
        {
            bytes.Add((byte)((value & SegmentBits) | ContinueBit));
            value >>>= 7;
        }

        // Add last remaining byte
        bytes.Add((byte)value);

        return bytes;
    }

    public static int DecodeVarInt(Stream stream)
    {
        var value = 0;
        var position = 0;
        var b = 0;

        while (b != -1)
        {
            b = stream.ReadByte();
            // Read next group of 7 bits and advance position
            value |= (b & SegmentBits) << position;
            position += 7;

            // No Continue Bit, EOF reached
            if ((b & ContinueBit) == 0) break;
            if (position >= 32) throw new OverflowException("VarInt too big");
        }

        return value;
    }

    public static IEnumerable<byte> EncodeString(string value)
    {
        var sBytes = Encoding.UTF8.GetBytes(value);
        var bytes = new List<byte>();

        bytes.AddRange(EncodeVarInt(sBytes.Length));
        bytes.AddRange(sBytes);

        return bytes;
    }

    public static string DecodeString(Stream stream, int length)
    {
        var span = new Span<byte>(new byte[length]);
        stream.ReadExactly(span);
        return Encoding.UTF8.GetString(span);
    }
}