using System.Net.Sockets;
using System.Text;
using Common.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common.Entities;

public class PacketAsync
{
    private readonly ILogger<PacketAsync> _logger;
    private readonly IServiceProvider _provider;
    private byte[] _header;
    private byte[] _data;
    

    private bool _isAuthenticated;
    private bool _isCompressed;
    private PacketConnectionType _connectionType; 
    private ushort _packetSize;
    private ushort _packetSizeWithoutHeader;
    private ushort _numberOfSubPackets;
    private ulong _timestamp;
    private List<SubPacket> _subPacketList;

    public PacketAsync(ILogger<PacketAsync> logger, IServiceProvider provider)
    {
        _logger = logger;
        _provider = provider;
        _header = new byte[0x10];
        _subPacketList = new List<SubPacket>();
    }

    public async Task<bool> ReadPacketAsync(NetworkStream networkStream)
    {
        _logger.LogTrace("Asking to read network packet...");

        try
        {
            _packetSize = 0;
            if (!networkStream.DataAvailable)
            {
                _logger.LogTrace("No network packet available");
                return false;
            }

            int read = await networkStream.ReadAsync(_header, 0, 0x10);
            if (read == 0)
            {
                _logger.LogWarning("Somehow read no data? There was supposed to be data!");
                return false;
            }

            _isAuthenticated = BitConverter.ToBoolean(_header, 0);
            _isCompressed = BitConverter.ToBoolean(_header, 1);
            _connectionType = (PacketConnectionType)BitConverter.ToUInt16(_header, 2);
            _packetSize = BitConverter.ToUInt16(_header, 4);
            _numberOfSubPackets = BitConverter.ToUInt16(_header, 6);
            _timestamp = BitConverter.ToUInt64(_header, 8);

            _packetSizeWithoutHeader = (ushort)(_packetSize - 0x10);
            _data = new byte[_packetSizeWithoutHeader];
            
            await networkStream.ReadAsync(_data, 0, _packetSizeWithoutHeader);

            int offset = 0;
            for (int i = 0; i < _numberOfSubPackets; i++)
            {
                SubPacket subPacket = _provider.GetRequiredService<SubPacket>();
                if (!subPacket.ReadSubPacket(_data, ref offset))
                {
                    _logger.LogWarning("Error Reading SubPacket");
                }
                _subPacketList.Add(subPacket);
            }
            
            return true;
        }
        catch
        {
            _logger.LogWarning(
                "An error occurred attempting to read data from the client. It may not be important, but, still...");
            return false;
        }
        finally
        {
            //logger.LogData(Data, Code, -1, "", ChecksumInPacket, ChecksumOfPacket);
            _logger.LogTrace("Completed reading network packet...");
        }
    }

    public bool WritePacket(byte[] data)
    {
        using MemoryStream headerStream = new MemoryStream();
        headerStream.Write(data,0,0x10);
        _header = headerStream.ToArray();
        
        _isAuthenticated = BitConverter.ToBoolean(_header, 0);
        _isCompressed = BitConverter.ToBoolean(_header, 1);
        _connectionType = (PacketConnectionType)BitConverter.ToUInt16(_header, 2);
        _packetSize = BitConverter.ToUInt16(_header, 4);
        _numberOfSubPackets = BitConverter.ToUInt16(_header, 6);
        _timestamp = BitConverter.ToUInt64(_header, 8);
        
        _packetSizeWithoutHeader = (ushort)(_packetSize - 0x10);

        using MemoryStream dataStream = new MemoryStream();
        dataStream.Write(data,0x10,_packetSizeWithoutHeader);
        _data = dataStream.ToArray();
        
        int offset = 0;
        for (int i = 0; i < _numberOfSubPackets; i++)
        {
            SubPacket subPacket = _provider.GetRequiredService<SubPacket>();
            if (!subPacket.ReadSubPacket(_data, ref offset))
            {
                _logger.LogWarning("Error Reading SubPacket");
            }
            _subPacketList.Add(subPacket);
        }

        return true;
    }

    public void EncryptPacket(Blowfish blowfish)
    {
        int offset = 0; 

        foreach (var subPacket in _subPacketList)
        {
            offset += 0x10; // skipping the subpacket header
            blowfish.Encipher(_data,offset,subPacket.PacketSizeWithoutHeader); //encrypting the subpacket data
            offset += subPacket.PacketSizeWithoutHeader; // advance to the next subpacket
        }
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

    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        set => _isAuthenticated = value;
    }

    public bool IsCompressed
    {
        get => _isCompressed;
        set => _isCompressed = value;
    }

    public PacketConnectionType ConnectionType
    {
        get => _connectionType;
        set => _connectionType = value;
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

    public ushort NumberOfSubPackets
    {
        get => _numberOfSubPackets;
        set => _numberOfSubPackets = value;
    }

    public ulong Timestamp
    {
        get => _timestamp;
        set => _timestamp = value;
    }

    public List<SubPacket> SubPacketList
    {
        get => _subPacketList;
        set => _subPacketList = value;
    }

    public override string ToString()
    {
        return $"{nameof(IsAuthenticated)}: {IsAuthenticated}, {nameof(IsCompressed)}: {IsCompressed}, {nameof(ConnectionType)}: {Enum.GetName(ConnectionType)}, {nameof(PacketSize)}: {PacketSize}, {nameof(PacketSizeWithoutHeader)}: {PacketSizeWithoutHeader}, {nameof(Timestamp)}: {Timestamp}, {nameof(NumberOfSubPackets)}: {NumberOfSubPackets}, {nameof(SubPacketList)}: {SubPacketListToString()}";
    }

    private string SubPacketListToString()
    {
        int index = 0;
        StringBuilder sb = new();
        sb.Append("[");
        foreach (var subPacket in _subPacketList)
        {
            sb.Append($"SubPacket#{index} : {subPacket.ToString()}");
        }

        sb.Append("]");

        return sb.ToString();
    }
}