using System.Text;
using Common.Abstractions;
using Common.Entities;

namespace NovumLobbyServer.Packets.Receive;

public class SessionPacket : Packet
{
    public bool InvalidPacket { get; }
    public ulong Sequence { get; }
    public string SessionId { get; }
    public string Version { get; }

    public SessionPacket(SubPacket packet) : this(packet.Data)
    {
    }

    public SessionPacket(byte[] data) : base(data)
    {
        using MemoryStream mem = new MemoryStream(data);
        using BinaryReader binReader = new BinaryReader(mem);
        try
        {
            Sequence = binReader.ReadUInt64();
            binReader.ReadUInt32();
            binReader.ReadUInt32();
            SessionId = Encoding.UTF8.GetString(binReader.ReadBytes(0x40)).Trim(new[] { '\0' });
            Version = Encoding.UTF8.GetString(binReader.ReadBytes(0x20)).Trim(new[] { '\0' });
        }
        catch (Exception)
        {
            InvalidPacket = true;
        }
    }
}