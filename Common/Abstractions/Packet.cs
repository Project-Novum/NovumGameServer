using Common.Entities;

namespace Common.Abstractions;

public abstract class Packet
{
    protected readonly byte[] Data;

    protected Packet(SubPacket packet) : this(packet.Data)
    {

    }

    protected Packet(byte[] data)
    {
        Data = data;
    }

    public override string ToString()
    {
        return ObjectDumper.Dump(this);
    }
}