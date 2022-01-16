using Common.Entities;

namespace Common.Abstractions;

public abstract class Packet
{
    protected Packet(byte[] data)
    {
    }

    public abstract byte[] Create();

    public override string ToString()
    {
        return ObjectDumper.Dump(this);
    }
}