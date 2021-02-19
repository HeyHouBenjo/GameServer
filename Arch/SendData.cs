using System;
using System.Linq;
using BasicServer.Management;

namespace BasicServer.Arch {
    public static class SendData {
        public static void SendTcpData(int toClient, Packet packet) {
            packet.WriteLength();
            Server.Clients[toClient].Tcp.SendData(packet);
        }
        
        public static void SendTcpDataToRoom(Room room, Packet packet) {
            packet.WriteLength();
            foreach (var client in room.Clients) { 
                client.Tcp.SendData(packet);
            }
        }
        
        public static void SendTcpDataToRoom(Room room, Packet packet, int withClient) {
            SendTcpDataToRoom(room, packet);
            Server.Clients[withClient].Tcp.SendData(packet);
        }

        public static void SendTcpDataToRoom(Room room, int exceptClient, Packet packet) {
            packet.WriteLength();
            foreach (var client in room.Clients.Where(client => client.Id != exceptClient)) {
                client.Tcp.SendData(packet);
            }
        }

        public static void SendTcpDataToAll(Packet packet, Func<Client, bool> condition) {
            packet.WriteLength();
            foreach (var client in Server.Clients.Values.Where(condition)) {
                client.Tcp.SendData(packet);
            }
        }
        
        public static void SendTcpDataToAll(Packet packet) {
            SendTcpDataToAll(packet, _ => true);
        }

        public static void SendTcpDataToAll(int exceptClient, Packet packet) {
            packet.WriteLength();
            for (var i = 1; i <= Server.MaxPlayers; i++)
                if (i != exceptClient)
                    Server.Clients[i].Tcp.SendData(packet);
        }

        public static void SendUdpData(int toClient, Packet packet) {
            packet.WriteLength();
            Server.Clients[toClient].Udp.SendData(packet);
        }
        
        public static void SendUdpDataToAll(Room room, Packet packet) {
            packet.WriteLength();
            foreach (var client in room.Clients) { 
                client.Udp.SendData(packet);
            }
        }

        public static void SendUdpDataToAll(Room room, int exceptClient, Packet packet) {
            packet.WriteLength();
            foreach (var client in room.Clients.Where(client => client.Id != exceptClient)) {
                client.Udp.SendData(packet);
            }
        }

        public static void SendUdpDataToAll(Packet packet) {
            packet.WriteLength();
            for (var i = 1; i <= Server.MaxPlayers; i++)
                Server.Clients[i].Udp.SendData(packet);
        }

        public static void SendUdpDataToAll(int exceptClient, Packet packet) {
            packet.WriteLength();
            for (var i = 1; i <= Server.MaxPlayers; i++)
                if (i != exceptClient)
                    Server.Clients[i].Udp.SendData(packet);
        }
    }
}
