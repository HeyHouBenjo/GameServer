namespace GameServer.PacketTypes {
    public enum ClientRoomPacket {
        RList = 1,
        RCreate,
        RJoin,
        RLeave,
        RKick,
        RReady,
        RColor,
        RLeader,
        RStart,
    }
}
