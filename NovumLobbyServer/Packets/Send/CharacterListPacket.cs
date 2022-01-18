using System.Text;
using Common.Abstractions;
using Common.Entities;
using Common.Enumerations;
using Database;
using Database.Models;
using NovumLobbyServer.Entities;

namespace NovumLobbyServer.Packets.Send;

public class CharacterListPacket : GamePacket
{
    private readonly DBContext _dbContext;
    private readonly IServiceProvider _provider;
    
    private const ushort Maxperpacket = 2;
    
    private  UInt64 _sequence;
    
    private List<Character> _charactersList;
    

    public CharacterListPacket(DBContext dbContext,IServiceProvider provider) : base(null!)
    {
      
        _dbContext = dbContext;
        _provider = provider;
    }

    public CharacterListPacket(byte[] data) : base(data)
    {
        
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

    public List<SubPacket> BuildPacket(ulong sequence, List<Character> characters)
    {
        _sequence = sequence;
        _charactersList = characters;
        
        List<SubPacket> subPackets = new List<SubPacket>();
        int numCharacters = _charactersList.Count >= 8 ? 8 : _charactersList.Count + 1;
        
        int characterCount = 0;
        int totalCount = 0;

        MemoryStream memoryStream = new MemoryStream(0x3B0);

        foreach (var chara in _charactersList)
        {
            Appearance appearance = _dbContext.Appearances.First(a => a.CharacterId == chara.Id);
            
            if (totalCount == 0 || characterCount % Maxperpacket == 0)
            {
                
                memoryStream.Write(BitConverter.GetBytes(_sequence));
                byte listTracker = (byte)((Maxperpacket * 2) * subPackets.Count);
                byte trackerIndex = numCharacters - totalCount <= Maxperpacket
                    ? (byte)(listTracker + 1)
                    : (byte)(listTracker);
                UInt32 charIndex = numCharacters - totalCount <= Maxperpacket
                    ? (UInt32)(numCharacters - totalCount)
                    : (UInt32)Maxperpacket;
                memoryStream.WriteByte(trackerIndex);
                memoryStream.Write(BitConverter.GetBytes(charIndex));
                memoryStream.WriteByte(0);
                memoryStream.Write(BitConverter.GetBytes(UInt16.MinValue));
            }

            memoryStream.Seek(0x10 + (0x1D0 * characterCount), SeekOrigin.Begin);

            GameWorld gameWorld = _dbContext.GameWorlds.First(w => w.Id == chara.ServerId);
            
            string worldname = gameWorld == null ? "Unknown" : gameWorld.Name;
            
            memoryStream.Write(BitConverter.GetBytes(uint.MinValue));
            memoryStream.Write(BitConverter.GetBytes((uint)chara.Id));
            memoryStream.WriteByte((byte) totalCount);
            
            byte options = 0;
            if (chara.State == 1)
                options |= 0x01;
            if (chara.DoRename)
                options |= 0x02;
            if (chara.IsLegacy)
                options |= 0x08;
            
            memoryStream.WriteByte(options);
            memoryStream.Write(BitConverter.GetBytes(ushort.MinValue));
            memoryStream.Write(BitConverter.GetBytes((uint) chara.CurrentZoneId));
            memoryStream.Write(Encoding.ASCII.GetBytes(chara.Name.PadRight(0x20, '\0'))); // Name
            memoryStream.Write(Encoding.ASCII.GetBytes(worldname.PadRight(0xE, '\0'))); //World Name
            memoryStream.Write(Encoding.ASCII.GetBytes(CharaInfo.BuildForCharaList(chara,appearance)));

            characterCount++;
            totalCount++;

            if (characterCount >= Maxperpacket)
            {
                byte[] data = memoryStream.GetBuffer();
                memoryStream.Dispose();
                memoryStream.Close();
                SubPacket subPacket = ActivatorUtilities.CreateInstance<SubPacket>(_provider);
                subPacket.Create((ushort)SubPacketType.GAME_PACKET, 0xe0006868, 0xe0006868, data, OpCode());
                subPackets.Add(subPacket);
                characterCount = 0;
            }

            if (totalCount >= 8)
            {
                break;
            }
        }
        
        if (_charactersList.Count < 8)
        {
            if (characterCount % Maxperpacket == 0)
            {
                memoryStream = new MemoryStream(0x3B0);
                memoryStream.Write(BitConverter.GetBytes(_sequence));
                byte listTracker = (byte)((Maxperpacket * 2) * subPackets.Count);
                byte trackerIndex = numCharacters - totalCount <= Maxperpacket
                    ? (byte)(listTracker + 1)
                    : (byte)(listTracker);
                UInt32 charIndex = numCharacters - totalCount <= Maxperpacket
                    ? (UInt32)(numCharacters - totalCount)
                    : (UInt32)Maxperpacket;
                memoryStream.WriteByte(trackerIndex);
                memoryStream.Write(BitConverter.GetBytes(charIndex));
                memoryStream.WriteByte(0);
                memoryStream.Write(BitConverter.GetBytes(UInt16.MinValue));
            }
                
            memoryStream.Seek(0x10 + (0x1D0 * characterCount), SeekOrigin.Begin);
                
            memoryStream.Write(BitConverter.GetBytes(uint.MinValue));
            memoryStream.Write(BitConverter.GetBytes(uint.MinValue));
            memoryStream.WriteByte((byte)totalCount);
            memoryStream.WriteByte(0);
            memoryStream.Write(BitConverter.GetBytes(ushort.MinValue));
            memoryStream.Write(BitConverter.GetBytes(uint.MinValue));

            characterCount++;
            totalCount++;
            
            if (characterCount >= Maxperpacket)
            {
                byte[] data = memoryStream.GetBuffer();
                memoryStream.Dispose();
                memoryStream.Close();
                SubPacket subPacket = ActivatorUtilities.CreateInstance<SubPacket>(_provider);
                subPacket.Create((ushort)SubPacketType.GAME_PACKET, 0xe0006868, 0xe0006868, data, OpCode());
                subPackets.Add(subPacket);
                characterCount = 0;
            }
                
        }
        
        if (characterCount > 0 || numCharacters == 0)
        {
            byte[] data = memoryStream.GetBuffer();
            memoryStream.Dispose();
            memoryStream.Close();
            SubPacket subPacket = ActivatorUtilities.CreateInstance<SubPacket>(_provider);
            subPacket.Create((ushort)SubPacketType.GAME_PACKET, 0xe0006868, 0xe0006868, data, OpCode());
            subPackets.Add(subPacket);
            characterCount = 0;
        }

        return subPackets;
    }

    public override uint SourceId() => 0xe0006868;

    public override uint TargetId() => 0xe0006868;

    public override ushort OpCode() => 0x0D;
}