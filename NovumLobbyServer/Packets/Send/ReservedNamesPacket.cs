using Common.Abstractions;

namespace NovumLobbyServer.Packets.Send;

public class ReservedNamesPacket : GamePacket
{
    private readonly UInt64 _sequence;
    private readonly List<String> _namesList;

    private const ushort Maxperpacket = 12;
    
    public ReservedNamesPacket(UInt64 sequence, List<string> namesList) : base(null!)
    {
        _sequence = sequence;
        _namesList = namesList;
    }

    public ReservedNamesPacket(byte[] data) : base(data)
    {
    }

    public override byte[] Create()
    {
        using MemoryStream memoryStream = new(0x210); 
        
        if (_namesList.Count == 0)
        {
            memoryStream.Write(BitConverter.GetBytes(_sequence));
            byte listTracker = 0;
            var trackerIndex = _namesList.Count - 0 <= Maxperpacket ? (byte)(listTracker + 1) : (byte)(listTracker);
            memoryStream.WriteByte(trackerIndex);
            memoryStream.Write(BitConverter.GetBytes(UInt32.MinValue));
            memoryStream.WriteByte(0);
            memoryStream.Write(BitConverter.GetBytes(UInt16.MinValue));
        }

        return memoryStream.GetBuffer();
    }

    public override uint SourceId() => 0xe0006868;

    public override uint TargetId() => 0xe0006868;
    public override ushort OpCode() => 0x16;
}