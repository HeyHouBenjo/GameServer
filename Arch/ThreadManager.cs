using System;
using System.Collections.Generic;
using System.Threading;
using GameServer.Game;
using GameServer.Management;

namespace GameServer.Arch {
    
    internal static class ThreadManager {
        private static readonly List<Action> ToExecuteOnMainThread = new();
        private static readonly List<Action> ExecuteCopiedOnMainThread = new();
        private static bool _actionToExecuteOnMainThread;

        /// <summary>Sets an action to be executed on the main thread.</summary>
        /// <param name="action">The action to be executed on the main thread.</param>
        public static void ExecuteOnMainThread(Action action) {
            if (action == null) {
                Console.WriteLine("No action to execute on main thread!");
                return;
            }

            lock (ToExecuteOnMainThread) {
                ToExecuteOnMainThread.Add(action);
                _actionToExecuteOnMainThread = true;
            }
        }

        /// <summary>Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.</summary>
        private static void UpdateMain() {
            if (!_actionToExecuteOnMainThread)
                return;

            ExecuteCopiedOnMainThread.Clear();
            lock (ToExecuteOnMainThread) {
                ExecuteCopiedOnMainThread.AddRange(ToExecuteOnMainThread);
                ToExecuteOnMainThread.Clear();
                _actionToExecuteOnMainThread = false;
            }

            foreach (var t in ExecuteCopiedOnMainThread)
                t();
        }

        public static bool IsRunning;

        public static void MainThread() {
            Console.WriteLine($"Main thread started. Running at {Constants.TicksPerSec} ticks per second.");
            var nextLoop = DateTime.Now;
            
            Server.Start(50, 26950);

            while (IsRunning)
            while (nextLoop < DateTime.Now) {
                Tick();

                nextLoop = nextLoop.AddMilliseconds(Constants.MsPerTick);

                if (nextLoop > DateTime.Now)
                    Thread.Sleep(nextLoop - DateTime.Now);
            }
        }

        private static void Tick() {
            foreach (var room in Server.Rooms.Values) {
                room.Game?.Update();
            }
            
            UpdateMain();
        }
    }
}
