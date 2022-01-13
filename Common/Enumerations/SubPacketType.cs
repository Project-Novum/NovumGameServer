namespace Common.Enumerations;

public enum SubPacketType : ushort
{
    INITIAL_HANDSHAKE = 0x9,
    ZONE_PACKET1 = 0x2,
    GAME_PACKET = 0x3,
    ZONE_PACKET2 = 0x7,
    ZONE_PACKET3 = 0x8
    
}