using System;
using BasicServer.Arch;
using BasicServer.Management;

namespace BasicServer {
    public static class ServerHandle {

        public static void WelcomeReceived(int fromClientId, Packet packet) {
            string username = packet.ReadString();

            Client client = Server.Clients[fromClientId];
            client.Name = username;
            
            Console.WriteLine($"{client} connected successfully!");
        }

        

    }
}
