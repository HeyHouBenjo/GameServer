using System;
using GameServer;
using GameServer.Arch;
using GameServer.Management;

namespace GameServer.Management {
    public static class RoomHandle {
        public static void RoomList(int fromClientId, Packet packet) {
            RoomSend.List(fromClientId);
            
            Client client = Server.Clients[fromClientId];
            
            Console.WriteLine($"{client} requested a list of rooms.");
        }

        public static void RoomCreate(int fromClientId, Packet packet) {
            string roomName = packet.ReadString();
            string roomPassword = packet.ReadString();
            int maxPlayers = packet.ReadInt();

            Server.CreateRoom(fromClientId, roomName, roomPassword, maxPlayers);
        }
        
        public static void RoomJoin(int fromClientId, Packet packet) {
            string roomId = packet.ReadString();
            string password = packet.ReadString();
            
            Client client = Server.Clients[fromClientId];
            Room room = Server.Rooms[roomId];
            
            if (room == null)
                return;

            if (room.IsLocked)
                return;
            
            Server.JoinRoom(client, room, password);
        }

        public static void RoomLeave(int fromClientId, Packet packet) {
            Client client = Server.Clients[fromClientId];

            if (client.Room == null)
                return;

            if (client.Room.IsLocked)
                return;
            
            Server.LeaveRoom(client);
        }

        public static void RoomKick(int fromClientId, Packet packet) {
            int kickId = packet.ReadInt();
            
            Client leaderClient = Server.Clients[fromClientId];

            if (leaderClient.Room == null)
                return;

            Client kickClient = Server.Clients[kickId];

            if (kickClient.Room == null)
                return;
            
            if (!kickClient.Room.Equals(leaderClient.Room))
                return;
            
            Server.KickFromRoom(leaderClient, kickClient);
        }

        public static void RoomLeader(int fromClientId, Packet packet) {
            var nextLeaderId = packet.ReadInt();
            var nextLeader = Server.Clients[nextLeaderId];
            var fromClient = Server.Clients[fromClientId];
            if (fromClient.Room == null || nextLeader.Room == null || fromClient.Room != nextLeader.Room)
                return;

            if (!fromClient.Room.Leader.Id.Equals(fromClientId))
                return;

            fromClient.Room.Leader = nextLeader;
            
            RoomSend.Properties(fromClient.Room);
        }

        public static void RoomReady(int fromClientId, Packet packet) {
            Client client = Server.Clients[fromClientId];
            
            if (client.Room == null)
                return;
            
            bool isReady = packet.ReadBool();
            client.Room.SetReady(client, isReady);

            RoomSend.Properties(client.Room);
        }

        public static void RoomColor(int fromClientId, Packet packet) {
            var fromClient = Server.Clients[fromClientId];

            var colorId = packet.ReadInt();

            if (fromClient.Room == null)
                return;
            
            fromClient.Room.SetColor(fromClient, colorId);
            
            RoomSend.Properties(fromClient.Room);
        }

        public static void RoomStart(int fromClientId, Packet packet) {
            var fromClient = Server.Clients[fromClientId];
            
            if (fromClient.Room == null)
                return;

            if (!fromClient.Room.Leader.Id.Equals(fromClientId))
                return;

            Server.StartRoom(fromClient.Room);
        }
    }
}
