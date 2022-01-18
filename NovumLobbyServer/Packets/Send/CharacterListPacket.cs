using Common.Abstractions;

namespace NovumLobbyServer.Packets.Send;

public class CharacterListPacket : GamePacket
{
    private readonly UInt64 _sequence;
    public const ushort Maxperpacket = 2;
    
    public CharacterListPacket(byte[] data) : base(data)
    {
        _sequence = 0;
    }

    public override byte[] Create()
    {
        int numCharacters = 1;

        int characterCount = 0;
        int totalCount = 0;
        using MemoryStream memoryStream = new ();
        // add the new button
        memoryStream.Write(BitConverter.GetBytes(_sequence));
        byte listTracker = 0;
        /*byte trackerIndex = numCharacters - totalCount <= Maxperpacket ? (byte)(listTracker + 1) : (byte)(listTracker);
        UInt32 charIndex = numCharacters - totalCount <= Maxperpacket
            ? (UInt32)(numCharacters - totalCount)
            : (UInt32)Maxperpacket;*/
        memoryStream.WriteByte((byte) (listTracker + 1));
        memoryStream.Write(BitConverter.GetBytes(UInt32.MinValue));
        memoryStream.WriteByte(0);
        memoryStream.Write(BitConverter.GetBytes(UInt16.MinValue));
        
        
        
        memoryStream.Write(BitConverter.GetBytes(uint.MinValue));
        memoryStream.Write(BitConverter.GetBytes(uint.MinValue));
        memoryStream.WriteByte(0);
        memoryStream.WriteByte(0);
        memoryStream.Write(BitConverter.GetBytes(ushort.MinValue));
        memoryStream.Write(BitConverter.GetBytes(uint.MinValue));
        
        return memoryStream.ToArray();

    }

    public override uint SourceId() => 0xe0006868;

    public override uint TargetId() => 0xe0006868;

    public override ushort OpCode() => 0x0D;
}