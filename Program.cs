using System;
using System.Threading;
using System.Threading.Tasks;
using GameServer.Arch;
using GameServer.Management;

namespace GameServer {
    internal static class Program {
        private static void Main() {
            Console.Title = "Game Server";
            Console.SetOut(new DatePrefix());
            
            ThreadManager.IsRunning = true;
            
            Server.Start(50, 26950);
            
            var mainThread = new Thread(ThreadManager.MainThread);
            mainThread.Start();
        }
    }
}
