
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using TCPServerCore;
using TCPServerExample.Data;
using TCPServerExample.DB;
using TCPServerExample.Game;
using TCPServerExample.Session;

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

        static void Main(string[] args)
        {
            ConfigManager.LoadConfig();
            DataManager.LoadData();


            var d = DataManager.StatDict;

            GameRoom room = RoomManager.Instance.Add(1);
            TickRoom(room, 50);

            // DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            try
            {
                Listener listener = new();
                Console.WriteLine("listening...");
                listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); }, 1);

                while (true)
                {
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }


}

