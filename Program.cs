using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using GameServer;

namespace GameServer
{
    class Program
    {
        private static bool isRunning = false;
        private static void Main(string[] args)
        {
            Console.Title = "Shooter Game Server";
            isRunning = true;

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            Server.Start(50, 25569);
        }

        private static void MainThread() {
            Console.WriteLine($"Main thread started. Running at {Constants.TICKS_PER_SEC} ticks per second");
            DateTime _nextLoop = DateTime.Now;

            while(isRunning) {
                while(_nextLoop < DateTime.Now) {
                    GameLogic.Update();

                    _nextLoop = _nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    if(_nextLoop > DateTime.Now) {
                        Thread.Sleep(_nextLoop - DateTime.Now);
                    }
                }
            }
        }
    }
}
