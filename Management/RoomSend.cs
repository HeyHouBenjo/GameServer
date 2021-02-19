using System;
using GameServer.Arch;
using GameServer.PacketTypes;
using static GameServer.Arch.SendData;
using static GameServer.PacketTypes.ServerRoomPacket;

namespace GameServer.Management {
    public static class RoomSend {
        private static Packet CreatePacket(ServerRoomPacket type) {
            return new((int)PacketType.Room, (int)type);
        }
        
        public static void List(int toClient) {
            using var packet = CreatePacket(RList);
            
            packet.Write(Server.Rooms.Count);
            foreach (var room in Server.Rooms.Values)
                packet.Write(room);

            SendTcpData(toClient, packet);
        }

        public static void ListUpdate() {
            using var packet = CreatePacket(RList);
            
            packet.Write(Server.Rooms.Count);
            foreach (var room in Server.Rooms.Values)
                packet.Write(room);
            
            SendTcpDataToAll(packet, client => client.Room == null);
        }

        public static void Created(int toClient, Room room) {
            using var packet = CreatePacket(RCreated);
            
            packet.Write(room);
            packet.Write(room.ClientPropertiesMap);

            SendTcpData(toClient, packet);
        }

        public static void Joined(int joinedClient, Room room) {
            using var packet = CreatePacket(RJoined);

            packet.Write(room);
            packet.Write(joinedClient);
            packet.Write(room.ClientPropertiesMap);
            
            SendTcpDataToRoom(room, packet);
        }

        public static void Left(int leftClient, Room room) {
            using var packet = CreatePacket(RLeft);
            
            packet.Write(leftClient);
            packet.Write(room.ClientPropertiesMap);
            
            SendTcpDataToRoom(room, packet, leftClient);
        }

        public static void CreateFailed(int toClient, string message) {
            using var packet = CreatePacket(RCreateFailed);
            
            packet.Write(message);
            
            SendTcpData(toClient, packet);
        }

        public static void JoinFailed(int toClient, Room room, string message) {
            using var packet = CreatePacket(RJoinFailed);
            
            packet.Write(room.Id);
            packet.Write(message);
            
            SendTcpData(toClient, packet);
        }

        public static void KickFailed(int toClient, string message) {
            using var packet = CreatePacket(RKickFailed);
            
            packet.Write(message);
            
            SendTcpData(toClient, packet);
        }

        public static void Properties(Room room) {
            using var packet = CreatePacket(RProperties);
            
            packet.Write(room.ClientPropertiesMap);
            
            SendTcpDataToRoom(room, packet);
        }

        public static void Start(Room room) {
            using var packet = CreatePacket(RStart);
            
            DateTime startTime = DateTime.Now.AddSeconds(Constants.CountdownSeconds);
            packet.Write(startTime.Ticks);
            packet.Write(room);
            packet.Write(room.ClientPropertiesMap);

            SendTcpDataToRoom(room, packet);
        }
    }
}
