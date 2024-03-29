﻿
using SharedDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TCPServerCore;
using TCPServerExample.Data;
using TCPServerExample.DB;
using TCPServerExample.Game;

namespace TCPServerExample
{
    class Program
    {
        static Listener listener = new ();
        static List<System.Timers.Timer> _timers = new();

        static void TickRoom(GameRoom room, int tick = 100)
        {
            var timer = new System.Timers.Timer();
            timer.Interval = tick;
            timer.Elapsed += ((s, e) => { room.Update(); });
            timer.AutoReset = true;
            timer.Enabled = true;

            _timers.Add(timer);
        }

        static void StattServerInfoTask()
        {
            var t = new System.Timers.Timer();
            t.AutoReset = true;
            t.Elapsed += new System.Timers.ElapsedEventHandler((s, e) =>
            {
                // TODO
                using (SharedDbContext shared = new SharedDbContext())
                {
                    ServerDb serverDb = shared.Servers.Where(s => s.Name == Name).FirstOrDefault();
                    if (serverDb != null)
                    {
                        serverDb.IpAddress = IpAddress;
                        serverDb.Port = Port;
                        serverDb.BusyScore = SessionManager.Instance.GetBusyScore();
                        shared.SaveChangesEx();
                    }
                    else
                    {
                        serverDb = new ServerDb()
                        {
                            Name = Program.Name,
                            IpAddress = Program.IpAddress,
                            Port = Program.Port,
                            BusyScore = SessionManager.Instance.GetBusyScore(),
                        };
                        shared.Servers.Add(serverDb);
                        shared.SaveChangesEx();
                    }
                }
            });
            t.Interval = 10 * 1000;
            t.Start();
        }

        public static string Name { get; } = "Server1";
        public static int Port { get; } = 7777;
        public static string IpAddress { get; set; }

        static void Main(string[] args)
        {
            ConfigManager.LoadConfig();
            DataManager.LoadData();

            GameRoom room = RoomManager.Instance.Add(1);
            TickRoom(room, 50);

            // DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);
            IpAddress = ipAddr.ToString();
            try
            {
                Listener listener = new();
                Console.WriteLine("listening...");

                StattServerInfoTask();

                listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); }, 1);

                while (true)
                {
                    DbTransaction.Instance.Flush();
                    // Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }


}

