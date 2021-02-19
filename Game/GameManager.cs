using System.Collections.Generic;
using GameServer.Management;

namespace GameServer.Game {
    public class GameManager {

        public bool IsRunning { get; set; }
        
        private Room Room { get; }
        private Dictionary<int, Player> Players { get; } = new();

        public GameManager(Room room) {
            Room = room;
            foreach (var client in room.Clients) {
                Players.Add(client.Id, new Player(client.Id, client.Name));
            }
        }

        public void Start() {
            foreach (var player in Players.Values) {
                player.Start();
            }
        }

        public void Update() {
            foreach (var player in Players.Values) {
                player.Update();
            }
        }

    }
}
