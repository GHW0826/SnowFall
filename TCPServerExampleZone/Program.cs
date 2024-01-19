
using SharedDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TCPServerCore;
using TCPServerExampleZone.Data;
using TCPServerExampleZone.DB;
using TCPServerExampleZone.Game;

namespace TCPServerExampleZone
{
    // 1. GameRoom 방식 간단한 동기화
    // 2. 넓은 영역 관리
    // 3. 심리스 MO

    // (thread 개수)
    // 1. Recv (N개)
    // 2. GameRoomMamger (1) -> GameLogic으로 수정(일단 단일 스레드로 수정)
    //      - 몬스터 도 따로 파도됨.
    //      - 데미지 피격 판정등 간단한것만 하면 좋다.
    //      -> 방이 많아지면 어떻게 처리할까 (고민)
    // 3. Send 담당 Task 추가 (1) (Network Task)
    // 4. DB (1)

    class Program
    {
        static Listener listener = new ();

        static void GameLogicTask()
        {
            while (true)
            {
                GameLogic.Instance.Update();
                Thread.Sleep(0);
            }
        }

        static void DbTask()
        {
            while (true)
            {
                DbTransaction.Instance.Flush();
                Thread.Sleep(0);
            }
        }
        
        static void NetworkTask()
        {
            while (true)
            {
                List<ClientSession> sessions = SessionManager.Instance.GetSessions();
                foreach (ClientSession session in sessions)
                {
                    session.FlushSend();
                }
                Thread.Sleep(0);
            }
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

            GameLogic.Instance.Push(() =>
            {
                GameRoom room = GameLogic.Instance.Add(1);
            });

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


                // DbTask();
                // DbTask
                {
                    Thread t = new Thread(DbTask);
                    t.Name = "DB";
                    t.Start();
                }

                /*
                    Task networkTask = new Task(NetworkTask, TaskCreationOptions.LongRunning);
                    networkTask.Start();
                */
                // NetworkTask
                {
                    Thread t = new Thread(NetworkTask);
                    t.Name = "Network Send";
                    t.Start();
                }

                // GameLogic
                Thread.CurrentThread.Name = "GameLogic";
                GameLogicTask();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }


}

