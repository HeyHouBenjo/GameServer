using System;
using GameServer.Arch;

namespace GameServer.Management {
    public static class ServerHandle {

        public static void WelcomeReceived(int fromClientId, Packet packet) {
            string username = packet.ReadString();

            Client client = Server.Clients[fromClientId];
            client.Name = username;
            
            Console.WriteLine($"{client} connected successfully!");
        }

        

    }
}
