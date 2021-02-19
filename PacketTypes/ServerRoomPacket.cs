namespace GameServer.PacketTypes {
    public enum ServerRoomPacket {
        RList = 1,
        RCreated,
        RJoined,
        RLeft,
        RStart,
        RCreateFailed,
        RJoinFailed,
        RLeaveFailed,
        RKickFailed,
        RProperties,
    }
}
