using System;
using System.Collections.Generic;
using System.Linq;
using BasicServer.Game;

namespace BasicServer.Management {
    public class Room {

        public class ClientProperties { 
            public bool IsLeader;
            public bool IsReady;
            public int ColorId;
        }
        
        public string Id { get; }
        public string Name { get; }
        public string Password { get; }
        public int MaxPlayers { get; }
        public int CurrentPlayers => Clients.Count;
        public bool IsFull => CurrentPlayers == MaxPlayers;

        public bool IsLocked { get; set; }

        public GameManager Game { get; private set; }

        public void StartGame() {
            Game = new GameManager(this);
        }
        
        public readonly Dictionary<int, ClientProperties> ClientPropertiesMap = new();
        
        public Client Leader {
            get {
                return CurrentPlayers == 0 ? null 
                    : 
                    Server.Clients[ClientPropertiesMap.Single(pair => pair.Value.IsLeader).Key];
            }
            set {
                foreach (var clientId in ClientPropertiesMap.Keys) {
                    ClientPropertiesMap[clientId].IsLeader = false;
                }
                ClientPropertiesMap[value.Id].IsLeader = true;
            }
        }

        private readonly List<int> _clientIds = new();
        public List<Client> Clients {
            get {
                var list = new List<Client>();
                foreach (int clientId in _clientIds) {
                    list.Add(Server.Clients[clientId]);
                }
                return list;
            }
        }

        public void SetReady(Client client, bool isReady) {
            ClientPropertiesMap[client.Id].IsReady = isReady;
        }

        public void SetColor(Client client, int colorId) {
            if (ClientPropertiesMap.Values.Any(properties => properties.ColorId.Equals(colorId)))
                return;
            
            ClientPropertiesMap[client.Id].ColorId = colorId;
        }

        public Room(string id, Client leader, string name, string password, int maxPlayers) {
            Id = id;
            AddClient(leader);
            Leader = leader;
            Name = name;
            Password = password;
            MaxPlayers = maxPlayers;
            IsLocked = false;
        }

        public void AddClient(Client client) {
            _clientIds.Add(client.Id);
            client.Room = this;
            for (int i = 0; i < 10; i++) {
                if (ClientPropertiesMap.Values.Any(prop => prop.ColorId.Equals(i)))
                    continue;
                
                ClientPropertiesMap.Add(client.Id, new ClientProperties {
                    IsReady = false, 
                    ColorId = i,
                    IsLeader = false
                });
                break;
            }
        }

        public bool RemoveClient(Client leftClient) {
            var leader = Leader;
            
            _clientIds.Remove(leftClient.Id);
            leftClient.Room = null;
            ClientPropertiesMap.Remove(leftClient.Id);

            if (CurrentPlayers == 0) {
                return false;
            }

            if (!leftClient.Equals(leader))
                return true;
            
            Leader = Clients.First();
            Console.WriteLine($"{Leader} is the new leader of room {this}");

            return true;
        }

        public void KickClient(Client kickClient) {
            _clientIds.Remove(kickClient.Id);
            kickClient.Room = null;
            ClientPropertiesMap.Remove(kickClient.Id);
        }

        public override string ToString() {
            return $"{{\"{Name}\" | \"{Id.Substring(0, 10)}...\" | ({CurrentPlayers}/{MaxPlayers})}}";
        }
    }
}
