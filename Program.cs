using System;
using System.Threading;
using System.Threading.Tasks;
using BasicServer.Arch;

namespace BasicServer {
    internal static class Program {
        private static void Main() {
            Console.Title = "Game Server";
            Console.SetOut(new DatePrefix());

            var mainThread = new Thread(ThreadManager.MainThread);
            mainThread.Start();

            ThreadManager.IsRunning = true;
        }
    }
}
