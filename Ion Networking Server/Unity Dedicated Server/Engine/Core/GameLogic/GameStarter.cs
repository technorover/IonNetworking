﻿using IonServer.Content;
using IonServer.Engine.Core.CommandLine;
using IonServer.Engine.Core.Networking;
using System;
using System.Diagnostics;
using System.Threading;

namespace IonServer.Engine.Core.GameLogic
{
    public static class GameStarter
    {
        private static bool isRunning = false;

        public static void Start()
        {
            Console.WriteLine("Starting Game Logic");
            isRunning = true;

            //Run the start function
            Game.Start();

            //Start the timekeeper
            Game.Time = new Stopwatch();
            Game.Time.Start();

            StartUpdateLoop();
        }

        public static void Stop()
        {
            Console.WriteLine("Stopping Game Thread");
            StopUpdateLoop();

            Program._gameLogicThread.Join(); //Wait for last update to finish running and for the _gameLogicThread to terminate.

            Console.WriteLine("Game Thread Stopped");
            Console.WriteLine("Finishing Game Logic");
            Game.Stop();
        }

        //Starts the update loop for the game. This is where the _gameLogicThread will be occupied.
        private static void StartUpdateLoop()
        {
            long timeBetweenUpdates = 1000L/Game.UpdatesPerSecond;

            long nextUpdate = 0;
            long now;
            long delta;

            while (isRunning)
            {
                now = Game.Time.ElapsedMilliseconds;
                delta = nextUpdate - now;

                if (delta <= 0)
                {
                    nextUpdate = now + timeBetweenUpdates;
                    
                    Game.Update();
                }
                else if (delta > Game.AllottedPacketHandleTime)
                {
                    //Process commands and packets before the game update.
                    CommandManager.HandleQueue();

                    if (PacketHandler.HandleNextPacket())
                        continue;

                    Thread.Sleep(Game.AllottedPacketHandleTime - 1);
                }
            }
        }

        //Stops the game loop.
        private static void StopUpdateLoop()
        {
            Console.WriteLine("Stopping Update Loop");
            isRunning = false;
        }
    }
}
