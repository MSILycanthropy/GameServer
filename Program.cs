using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using GameServer;

namespace GameServer
{   
    ///<summary>
    ///     The main program of the server.
    ///</summary>
    class Program
    {
        //Bool to make sure the server is running.
        private static bool isRunning = false;

        ///<summary>
        ///     The main loop. Wraps everything together.
        ///</summary>
        private static void Main(string[] args)
        {
            Console.Title = "Shooter Game Server";
            isRunning = true;
            //Create the main thread and start it. Along with actually starting the server.
            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();
            Server.Start(50, 25569);
        }

        ///<summary>
        ///     The main loop that runs on the thread while the server is running <see cref="Program.isRunning"/>
        ///</summary>
        private static void MainThread() {
            Console.WriteLine($"Main thread started. Running at {Constants.TICKS_PER_SEC} ticks per second");

            //Get the time
            DateTime _nextLoop = DateTime.Now;

            //Main game loop
            while(isRunning) {
                while(_nextLoop < DateTime.Now) {
                    //Update the game.
                    GameLogic.Update();

                    //Add when the next loop is supposed to be
                    _nextLoop = _nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    //If the next loop is in the future, sleep the thread to save CPU power.
                    if(_nextLoop > DateTime.Now) {
                        Thread.Sleep(_nextLoop - DateTime.Now);
                    }
                }
            }
        }
    }
}
