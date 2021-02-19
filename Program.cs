using System;
using System.Threading;
using GameServer.Arch;
using GameServer.Management;

namespace GameServer {
    internal static class Program {
        private static void Main() {
            Console.Title = "Game Server";
            Console.SetOut(new DatePrefix());
            
            Server.Start(50, 26950);
            
            ThreadManager.IsRunning = true;
            var mainThread = new Thread(ThreadManager.MainThread);
            mainThread.Start();
        }
    }
}
