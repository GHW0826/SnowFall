
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using TCPServerCore;
using TCPServerExample.Data;
using TCPServerExample.DB;
using TCPServerExample.Game;
using TCPServerExample.Session;
using TCPServerExample.Utils;

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

            // Test
            using (AppDbContext db = new AppDbContext ())
            {
                PlayerDb? player = db.Players.FirstOrDefault();
                if (player != null)
                {
                    db.Items.Add(new ItemDb()
                    {
                        TemplateId = 1,
                        Count = 1,
                        Slot = 0,
                        Owner = player
                    });

                    db.Items.Add(new ItemDb()
                    {
                        TemplateId = 100,
                        Count = 1,
                        Slot = 1,
                        Owner = player
                    });

                    db.Items.Add(new ItemDb()
                    {
                        TemplateId = 101,
                        Count = 1,
                        Slot = 2,
                        Owner = player
                    });

                    db.Items.Add(new ItemDb()
                    {
                        TemplateId = 200,
                        Count = 10,
                        Slot = 5,
                        Owner = player
                    });

                    bool success = db.SaveChangesEx();
                    if (success == false)
                        return;
                }
            }

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

