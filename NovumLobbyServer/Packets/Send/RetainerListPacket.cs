using Common.Abstractions;

namespace NovumLobbyServer.Packets.Send;

public record Retainer(uint id, uint characterId, string name, bool doRename);

public class RetainerListPacket : GamePacket
{
    
    private readonly UInt64 _sequence;
    private readonly List<Retainer> _retainerList;
    
    private const ushort Maxperpacket = 9; 

    public RetainerListPacket(ulong sequence, List<Retainer> retainerList) : base(null!)
    {
        _sequence = sequence;
        _retainerList = retainerList;
    }

    public RetainerListPacket(byte[] data) : base(data)
    {
    }
    
    public override byte[] Create()
    {
        int retainerCount = 0;
        int totalCount = 0;
        
        using MemoryStream memoryStream = new (0x210);

        if (_retainerList.Count == 0)
        {
            memoryStream.Write(BitConverter.GetBytes(_sequence));
            byte listTracker = 0;
            var trackerIndex = _retainerList.Count - 0 <= Maxperpacket ? (byte)(listTracker + 1) : (byte)(listTracker);
            memoryStream.WriteByte(trackerIndex);
            memoryStream.Write(BitConverter.GetBytes(UInt32.MinValue));
            memoryStream.WriteByte(0);
            memoryStream.Write(BitConverter.GetBytes(UInt16.MinValue));
        }

        return memoryStream.GetBuffer();
    }

    public override uint SourceId() => 0xe0006868;

    public override uint TargetId() => 0xe0006868;
    public override ushort OpCode() => 0x17;
   
}