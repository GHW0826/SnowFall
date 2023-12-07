

using System.Net;
using System.Text;
using TCPServerCore;
using TCPServerExample.Packet;
using TCPServerExample.Session;

namespace TCPServerExample
{
    class Program
    {
        static Listener listener = new ();
        public static GameRoom Room = new();

        static void FlushRoom()
        {
            Room.Push(() => Room.Flush());
            JobTimer.Instance.Push(FlushRoom, 250);
        }

        static void Main(string[] args)
        {
            // DNS
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            try
            {
                Listener listener = new();

                Console.WriteLine("listening...");

                listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });

                //FlushRoom();
                JobTimer.Instance.Push(FlushRoom);

                //int roomTick = 0;
                while (true)
                {
                    JobTimer.Instance.Flush();

                    /*
                    int now = System.Environment.TickCount;
                    if (roomTick < now)
                    {
                        Room.Push(() => Room.Flush());
                        roomTick = now + 250;
                    }
                    */
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }


}

