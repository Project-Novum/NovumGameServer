namespace Common.Abstractions;

public abstract class GamePacket : Packet
{
    public abstract uint SourceId();
    public abstract uint TargetId();
    public abstract ushort OpCode();
    
    protected GamePacket(byte[] data) : base(data)
    {
    }
}