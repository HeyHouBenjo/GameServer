using System.Collections.Generic;
using GameServer.Management;

namespace GameServer.Game {
    public class GameManager {

        private Room Room { get; }

        private Dictionary<int, Player> Players { get; } = new();
        
        public bool IsRunning { get; private set; }

        public GameManager(Room room) {
            Room = room;
            foreach (var client in room.Clients) {
                Players.Add(client.Id, new Player(client.Id, client.Name));
            }

            Start();
        }

        private void Start() {
            foreach (var player in Players.Values) {
                player.Start();
            }
            
            IsRunning = true;
        }

        public void Update() {
            foreach (var player in Players.Values) {
                player.Update();
            }
        }

    }
}
