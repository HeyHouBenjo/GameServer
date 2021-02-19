using System;
using BasicServer.Arch;
using BasicServer.Game;
using GameServer.Management;

namespace BasicServer.Management {
    public class Client {

        public readonly int Id;
        public readonly TcpManager Tcp;
        public readonly UdpManager Udp;
        
        public string Name { get; set; }
        public Player Player;
        public Room Room { get; set; }

        public Client(int clientId) {
            Id = clientId;
            Tcp = new TcpManager(Id);
            Udp = new UdpManager(Id);
        }

        public void OnConnect() {
            ServerSend.Welcome(Id, "Welcome to the server!");
            if (Tcp.Socket.Client.RemoteEndPoint != null)
                _endpoint = Tcp.Socket.Client.RemoteEndPoint.ToString();
        }
        
        public void Disconnect() {
            Tcp.Disconnect();
            Udp.Disconnect();
            
            if (Room != null)
                Server.LeaveRoom(this);
            
            Player = null;
            
            Console.WriteLine($"{this} has disconnected.");
        }

        private string _endpoint = "Client";
        public override string ToString() {
            return $"{{\"{Name}\" | {_endpoint}}}";
        }
    }
}
