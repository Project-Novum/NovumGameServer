using Microsoft.Extensions.Logging;

namespace Common.Entities;

public class GamePacket
{
    private readonly ILogger<GamePacket> _logger;

    private byte[] _header;

    private ushort _unk1;
    private ushort _opcode;
    private uint _unk2;
    private uint _timestamp;
    private uint _unk3;
    
    public GamePacket(ILogger<GamePacket> logger)
    {
        _logger = logger;
    }

    public bool ReadGamePacket(byte[] data)
    {
        using MemoryStream headerStream = new MemoryStream();
        headerStream.Write(data,0,0x10);
        _header = headerStream.ToArray();

        _unk1 = BitConverter.ToUInt16(_header, 0);
        _opcode = BitConverter.ToUInt16(_header, 2);
        _unk2 = BitConverter.ToUInt16(_header, 4);
        _timestamp = BitConverter.ToUInt16(_header, 8);
        _unk3 = BitConverter.ToUInt16(_header, 12);


        return true;

    }

    public byte[] Header
    {
        get => _header;
        set => _header = value;
    }

    public ushort Unk1
    {
        get => _unk1;
        set => _unk1 = value;
    }

    public ushort Opcode
    {
        get => _opcode;
        set => _opcode = value;
    }

    public uint Unk2
    {
        get => _unk2;
        set => _unk2 = value;
    }

    public uint Timestamp
    {
        get => _timestamp;
        set => _timestamp = value;
    }

    public uint Unk3
    {
        get => _unk3;
        set => _unk3 = value;
    }

    public override string ToString()
    {
        return $"{nameof(Unk1)}: {Unk1}, {nameof(Opcode)}: {Opcode:x2}, {nameof(Unk2)}: {Unk2}, {nameof(Timestamp)}: {Timestamp}, {nameof(Unk3)}: {Unk3}";
    }
}