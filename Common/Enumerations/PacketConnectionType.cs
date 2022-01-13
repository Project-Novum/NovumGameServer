namespace Common.Enumerations;

public enum PacketConnectionType : ushort
{
    UNKNOWN_CONNECTION = 0x0,
    ZONE_CONNECTION = 0x1,
    CHAT_CONNECTION = 0x2,
    INITIAL_HANDSHAKE = 0x3
}