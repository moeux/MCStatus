namespace MCStatus.Models;

public class Server
{
    public required string Address { get; init; }
    public required ushort Port { get; init; }
    public required int ProtocolVersion { get; init; }

    private bool Equals(Server other)
    {
        return string.Equals(Address, other.Address, StringComparison.OrdinalIgnoreCase) &&
               Port == other.Port &&
               ProtocolVersion == other.ProtocolVersion;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Server)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Address, StringComparer.OrdinalIgnoreCase);
        hashCode.Add(Port);
        hashCode.Add(ProtocolVersion);
        return hashCode.ToHashCode();
    }
}