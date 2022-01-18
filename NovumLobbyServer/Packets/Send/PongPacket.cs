using Common;
using Common.Abstractions;

namespace NovumLobbyServer.Packets.Send;

public class PongPacket : Packet
{
    private byte[] _data;
    private const uint PacketSize = 0x18;
    public PongPacket() : base(null!)
    {
        byte[] data = new byte[PacketSize];

        using (MemoryStream mem = new MemoryStream(data))
        {
            using (BinaryWriter binWriter = new BinaryWriter(mem))
            {
                try
                {
                    binWriter.Write((UInt32)0);
                    binWriter.Write((UInt32)Utils.UnixTimeStampUTC());
                }
                catch (Exception)
                {}
            }
        }

        _data = data;
    }

    public PongPacket(byte[] data) : base(data)
    {
        _data = data;
    }

    public override byte[] Create()
    {
        return _data;
    }
}