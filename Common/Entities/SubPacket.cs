using Common.Abstractions;
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
    private GamePacketAsync _gamePacketAsync;

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

    
    public bool Create(GamePacket gamePacket)
    {
        return Create((ushort)SubPacketType.GAME_PACKET, gamePacket.SourceId(), gamePacket.TargetId(), gamePacket.Create(),gamePacket.OpCode());
    }
    

    public bool Create(ushort PacketType, uint SourceId, Packet packet)
    {
        return Create(PacketType, SourceId, 0,packet.Create(),0);
    }


    public bool Create(ushort PacketType, uint SourceId,uint TargetId ,byte[] data,ushort Opcode)
    {
        using MemoryStream memoryStream = new ();
        
        if (PacketType == (ushort)SubPacketType.GAME_PACKET)
        {
            _gamePacketAsync = ActivatorUtilities.CreateInstance<GamePacketAsync>(_provider);
            _gamePacketAsync.Unk1 = 0x14;
            _gamePacketAsync.Opcode = Opcode;
            _gamePacketAsync.Unk2 = UInt32.MinValue;
            _gamePacketAsync.Timestamp = Utils.UnixTimeStampUTC();
            _gamePacketAsync.Unk3 = UInt32.MinValue;
            _gamePacketAsync.BuildHeader();
            memoryStream.Write(_gamePacketAsync.Header);
        }
        memoryStream.Write(data);
        _data = memoryStream.ToArray();

        _type = (SubPacketType)PacketType;
        _sourceId = SourceId;
        _targetId = TargetId;
        _unknown = UInt32.MinValue;

        
        _packetSize = (ushort)(0x10 + _data.Length);
        _packetSizeWithoutHeader = (ushort)(_data.Length);

        /*if (PacketType == (ushort)SubPacketType.GAME_PACKET)
        {
            _packetSize += 0x10;
            _packetSizeWithoutHeader += 0x10;
        }*/

        using MemoryStream headerStream = new MemoryStream();
        headerStream.Write(BitConverter.GetBytes(_packetSize));
        headerStream.Write(BitConverter.GetBytes((ushort)_type));
        headerStream.Write(BitConverter.GetBytes(_sourceId));
        headerStream.Write(BitConverter.GetBytes(_targetId));
        headerStream.Write(BitConverter.GetBytes(_unknown));
        _header = headerStream.ToArray();
        
        return true;
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
            _packetSize += 0x10;
            _gamePacketAsync = _provider.GetRequiredService<GamePacketAsync>();
            if (!_gamePacketAsync.ReadGamePacket(_data))
            {
                _logger.LogWarning("Error Reading Game Packet");
            }
        }


        offset += _packetSize;

        return true;
    }

    public bool DecryptData(Blowfish blowfish)
    {
        if (_type == SubPacketType.GAME_PACKET)
        {
            blowfish.Decipher(_data, 0, _packetSizeWithoutHeader);

            _gamePacketAsync = _provider.GetRequiredService<GamePacketAsync>();
            if (!_gamePacketAsync.ReadGamePacket(_data))
            {
                _logger.LogWarning("Error Reading Game Packet");
            }

            _data = _data.Skip(0x10).ToArray();
            // Subtract GameMessage header
            _packetSizeWithoutHeader -= 0x10;
        }

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

    public GamePacketAsync GamePacketAsync
    {
        get => _gamePacketAsync;
        set => _gamePacketAsync = value;
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
        return $"{nameof(GamePacketAsync)}: {GamePacketAsync?.ToString()}, {nameof(PacketSize)}: {PacketSize}, {nameof(PacketSizeWithoutHeader)}: {PacketSizeWithoutHeader}, {nameof(Type)}: {Enum.GetName(Type)}, {nameof(SourceId)}: {SourceId}, {nameof(TargetId)}: {TargetId}, {nameof(Unknown)}: {Unknown}";
    }
}