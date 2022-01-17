using System.Text;
using Common.Abstractions;
using Common.Entities;

namespace NovumLobbyServer.Packets.Send;
public record Account(uint Id, string Name);


public class AccountListPacket : GamePacket
{
   
    private const ushort MaxPerPacket = 8;
    
    
    private UInt64 _sequence;
    private List<Account> _accountList;

    public AccountListPacket(UInt64 sequence, List<Account> accountList) : base(null!)
    {
        _sequence = sequence;
        _accountList = accountList;
    }

    public AccountListPacket(byte[] data) : base(data)
    {
    }

    public override uint SourceId() => 0xe0006868;

    public override uint TargetId() => 0xe0006868;

    public override ushort OpCode() => 0x0C;
    
    public override byte[] Create()
    {
         
        int accountCount = 0;
        int totalCount = 0;

        using MemoryStream memoryStream = new MemoryStream();

        foreach (var account in _accountList)
        {
            if (totalCount == 0 || accountCount % MaxPerPacket == 0)
            {
                memoryStream.Write(BitConverter.GetBytes(_sequence));
                byte listTracker = 0;

                var trackerIndex = _accountList.Count - totalCount <= MaxPerPacket
                    ? (byte)(listTracker + 1)
                    : (byte)(listTracker);

                var accountIndex = _accountList.Count - totalCount <= MaxPerPacket
                    ? (UInt32)(_accountList.Count - totalCount)
                    : (UInt32)MaxPerPacket;
           
                memoryStream.WriteByte(trackerIndex);
                memoryStream.Write(BitConverter.GetBytes(accountIndex));
                memoryStream.WriteByte(0);
                memoryStream.Write(BitConverter.GetBytes((UInt16.MinValue)));
            }

            UInt32 id = account.Id;
            memoryStream.Write(BitConverter.GetBytes(id));
            memoryStream.Write(BitConverter.GetBytes(UInt32.MinValue));
            memoryStream.Write(Encoding.ASCII.GetBytes(account.Name.PadRight(0x40, '\0')));
            accountCount++;
            totalCount++;
        }
        
        return memoryStream.ToArray();
    }

    public static AccountListPacket CreateDefault()
    {
        var accountList = new List<Account> { new (1, "FINAL FANTASY XIV") };
        AccountListPacket packet = new (1, accountList);

        return packet;

    }
    
    
}

