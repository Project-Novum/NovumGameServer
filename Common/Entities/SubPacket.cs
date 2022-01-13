using Common.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common.Entities;

public class SubPacket
{
    private readonly ILogger<SubPacket> _logger;
    private readonly IServiceProvider _provider;
    private byte[] _header;
    private byte[] _data;
    private GamePacket _gamePacket;

    private ushort _packetSize;
    private ushort _packetSizeWithoutHeader;
    private SubPacketType _type;
    private uint _sourceId;
    private uint _targetId;
    private uint _unknown;
    
    

    public SubPacket(ILogger<SubPacket> logger, IServiceProvider provider)
    {
        _logger = logger;
        _provider = provider;

    }

    public bool ReadSubPacket(byte[] baseData, ref int offset)
    {
        using MemoryStream headerStream = new MemoryStream();
        headerStream.Write(baseData,offset,0x10);

        _header = headerStream.ToArray();
        _packetSize = BitConverter.ToUInt16(_header, 0);
        _type = (SubPacketType)BitConverter.ToUInt16(_header, 2);
        _sourceId = BitConverter.ToUInt32(_header, 4);
        _targetId = BitConverter.ToUInt32(_header, 8);
        _unknown = BitConverter.ToUInt32(_header, 12);
        
        
        _packetSizeWithoutHeader = (ushort)(_packetSize - 0x10);

        using MemoryStream dataStream = new MemoryStream();
        dataStream.Write(baseData,offset + 0x10,_packetSizeWithoutHeader);
        _data = dataStream.ToArray();

        if (_type == SubPacketType.GAME_PACKET)
        {
            _gamePacket = _provider.GetRequiredService<GamePacket>();
            if (!_gamePacket.ReadGamePacket(_data))
            {
                _logger.LogWarning("Error Reading Game Packet");
            }
        }


        offset += _packetSize;

        return true;
    }


    public byte[] Header
    {
        get => _header;
        set => _header = value;
    }

    public byte[] Data
    {
        get => _data;
        set => _data = value;
    }

    public GamePacket GamePacket
    {
        get => _gamePacket;
        set => _gamePacket = value;
    }

    public ushort PacketSize
    {
        get => _packetSize;
        set => _packetSize = value;
    }

    public ushort PacketSizeWithoutHeader
    {
        get => _packetSizeWithoutHeader;
        set => _packetSizeWithoutHeader = value;
    }

    public SubPacketType Type
    {
        get => _type;
        set => _type = value;
    }

    public uint SourceId
    {
        get => _sourceId;
        set => _sourceId = value;
    }

    public uint TargetId
    {
        get => _targetId;
        set => _targetId = value;
    }

    public uint Unknown
    {
        get => _unknown;
        set => _unknown = value;
    }

    public override string ToString()
    {
        return $"{nameof(GamePacket)}: {GamePacket?.ToString()}, {nameof(PacketSize)}: {PacketSize}, {nameof(PacketSizeWithoutHeader)}: {PacketSizeWithoutHeader}, {nameof(Type)}: {Enum.GetName(Type)}, {nameof(SourceId)}: {SourceId}, {nameof(TargetId)}: {TargetId}, {nameof(Unknown)}: {Unknown}";
    }
}