

using System.Net;
using System.Text;
using TCPServerCore;
using TCPServerExample.Packet;

namespace TCPServerExample;



class Program
{
    static Listener listener = new Listener();

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

            PacketManager.Instance.Register();

            listener.BackingNumber = 10;
            Console.WriteLine("listening...");

            listener.Init(endPoint, () => { return new ClientSession(); });
            while (true)
            {
                // no exit
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}

