using BasicServer.Arch;
using BasicServer.PacketTypes;
using static BasicServer.Arch.SendData;
using static BasicServer.PacketTypes.ServerDefaultPacket;

namespace GameServer.Management {
    internal static class ServerSend {
        
        private static Packet CreatePacket(ServerDefaultPacket type) {
            return new((int)PacketType.Default, (int)type);
        }

        public static void Welcome(int toClient, string msg) {
            using var packet = CreatePacket(DWelcome);
            
            packet.Write(msg);
            packet.Write(toClient);

            SendTcpData(toClient, packet);
        }
    }
}
