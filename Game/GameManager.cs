using BasicServer.Management;

namespace BasicServer.Game {
    public class GameManager {

        private Room Room { get; }

        private bool IsStarted { get; set; }

        public GameManager(Room room) {
            Room = room;
        }

        public void Start() {
            IsStarted = true;
        }

        public void Update() {
            if (!IsStarted)
                return;
            
            
        }

    }
}
