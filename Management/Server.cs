using System;
using System.Collections.Generic;
using GameServer.Arch;
using GameServer.Management;
using GameServer.PacketTypes;

namespace GameServer {

    internal static class Server {
        public delegate void PacketHandler(int fromClient, Packet packet);
        public static readonly Dictionary<int, Client> Clients = new();
        public static readonly Dictionary<string, Room> Rooms = new();
        public static Dictionary<int, Dictionary<int, PacketHandler>> PacketHandlers;

        public static int MaxPlayers { get; private set; }

        public static int Port { get; private set; }
        
        public static void Start(int maxPlayers, int port){
            MaxPlayers = maxPlayers;
            Port = port;

            Console.WriteLine("Starting server...");
            InitializeServerData();
            
            Listener.Start();

            Console.WriteLine($"Server started on port {Port}.");
        }

        private static void InitializeServerData(){
            for (var i = 1; i <= MaxPlayers; i++) {
                Clients.Add(i, new Client(i));
            }

            PacketHandlers = new Dictionary<int, Dictionary<int, PacketHandler>> {
                {(int)PacketType.Default, new Dictionary<int, PacketHandler> {
                    {(int)ClientDefaultPacket.DWelcomeReceived, ServerHandle.WelcomeReceived},
                }},
                {(int)PacketType.Room, new Dictionary<int, PacketHandler> {
                    {(int)ClientRoomPacket.RList, RoomHandle.RoomList},
                    {(int)ClientRoomPacket.RCreate, RoomHandle.RoomCreate},
                    {(int)ClientRoomPacket.RJoin, RoomHandle.RoomJoin},
                    {(int)ClientRoomPacket.RLeave, RoomHandle.RoomLeave},
                    {(int)ClientRoomPacket.RKick, RoomHandle.RoomKick},
                    {(int)ClientRoomPacket.RLeader, RoomHandle.RoomLeader},
                    {(int)ClientRoomPacket.RReady, RoomHandle.RoomReady},
                    {(int)ClientRoomPacket.RColor, RoomHandle.RoomColor},
                    {(int)ClientRoomPacket.RStart, RoomHandle.RoomStart},
                }}
            };
            
            Console.WriteLine("Initialized packets.");
        }

        public static void CreateRoom(int leaderId, string name, string password, int maxPlayers) {
            var leader = Clients[leaderId];

            if (leader.Room != null) {
                RoomSend.CreateFailed(leaderId, "Failed to create room!");
                Console.WriteLine($"{leader} tried to create a room while already being in one!");
                return;
            }

            string id;
            do {
                id = Guid.NewGuid().ToString();
            } while (Rooms.ContainsKey(id));
            
            var room = new Room(id, leader, name, password, maxPlayers);
            Rooms.Add(id, room);

            RoomSend.Created(leaderId, room);
            RoomSend.ListUpdate();
            
            Console.WriteLine($"{leader} created a room {room}.");
        }

        public static void JoinRoom(Client client, Room room, string password) {
            if (room.IsFull) {
                RoomSend.JoinFailed(client.Id, room, "Room is full!");
                Console.WriteLine($"{client} tried to join full room {room}.");
            } else if (!room.Password.Equals(password)) {
                RoomSend.JoinFailed(client.Id, room,"Wrong password!");
                Console.WriteLine($"{client} entered wrong password for room {room}.");
            } else {
                room.AddClient(client);
                RoomSend.Joined(client.Id, room);
                RoomSend.ListUpdate();
                Console.WriteLine($"{client} joined the room {room}.");
            }
        }

        public static void LeaveRoom(Client client) {
            Room room = client.Room;
            bool isEmpty = !room.RemoveClient(client);
            
            Console.WriteLine($"{client} has left room {room}");
            
            if (isEmpty) {
                Rooms.Remove(room.Id);
                Console.WriteLine($"Room {room} was deleted because every client left.");
            }

            RoomSend.Left(client.Id, room);
            RoomSend.ListUpdate();
        }

        public static void KickFromRoom(Client fromClient, Client kickClient) {
            Room room = kickClient.Room;
            if (!fromClient.Equals(room.Leader)) {
                RoomSend.KickFailed(fromClient.Id, "Only the room lead can kick others!");
                Console.WriteLine($"{fromClient} tried to kick {kickClient} while not being the leader!");
                return;
            }

            room.KickClient(kickClient);
            
            Console.WriteLine($"{kickClient} was kicked from room {room}");
            
            RoomSend.Left(kickClient.Id, room);
            RoomSend.ListUpdate();
        }

        public static void StartRoom(Room room) {
            room.IsLocked = true;
            room.StartGame();
            
            RoomSend.Start(room);
            Console.WriteLine($"Room {room} started the game.");
            
            RoomSend.ListUpdate();
        }
        
    }

}
