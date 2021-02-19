using GameServer.Arch;
using GameServer.Management;
using GameServer.PacketTypes;
using static GameServer.Arch.SendData;

namespace GameServer.Game {
    public static class GameSend {
        
        private static Packet CreatePacket(ServerGamePacket type) {
            return new((int)PacketType.Game, (int)type);
        }

    }
}
