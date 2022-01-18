using System.Text;
using Common.Abstractions;
using Common.Entities;
using Database.Models;

namespace NovumLobbyServer.Packets.Send;

public class WorldListPacket : GamePacket
{
    private readonly UInt64 _sequence;
    private readonly List<GameWorld> _gameWorlds;
    
    private const ushort Maxperpacket = 6;

    public WorldListPacket(UInt64 sequence, List<GameWorld> gameWorlds) : base(null!)
    {
        _sequence = sequence;
        _gameWorlds = gameWorlds;
    }

    public WorldListPacket(byte[] data) : base(data)
    {
    }

    public override byte[] Create()
    {
        int serverCount = 0;
        int totalCount = 0;

        using MemoryStream memoryStream = new (0x210);
        foreach (var world in _gameWorlds)
        {
            if (totalCount == 0 || serverCount % Maxperpacket == 0)
            {
                //Write List Info
                memoryStream.Write(BitConverter.GetBytes(_sequence));
                byte listTracker = 0;
                var trackerIndex = _gameWorlds.Count - totalCount <= Maxperpacket ? (byte)(listTracker + 1) : (byte)(listTracker);
                var worldIndex = _gameWorlds.Count - totalCount <= Maxperpacket ? (UInt32)(_gameWorlds.Count - totalCount) : (UInt32)Maxperpacket; 
                memoryStream.WriteByte(trackerIndex);
                memoryStream.Write(BitConverter.GetBytes(worldIndex));
                memoryStream.WriteByte(0);
                memoryStream.Write(BitConverter.GetBytes((UInt16.MinValue)));
            }

            ushort worldId = (ushort)world.Id;
            //Write Entries
            memoryStream.Write(BitConverter.GetBytes(worldId));
            memoryStream.Write(BitConverter.GetBytes((ushort)world.Position));
            memoryStream.Write(BitConverter.GetBytes((uint) world.Population));
            memoryStream.Write(BitConverter.GetBytes(UInt64.MinValue));
            memoryStream.Write(Encoding.ASCII.GetBytes(world.Name.PadRight(64,'\0')));
            
            serverCount++;
            totalCount++;
        }

        return memoryStream.GetBuffer();
    }

    public override uint SourceId() => 0xe0006868;

    public override uint TargetId() => 0xe0006868;
    public override ushort OpCode() => 0x15;

}