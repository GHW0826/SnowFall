

using System.Net;
using System.Net.Sockets;
using System.Text;
using TCPServerCore;
using TCPServerCore.leature;

namespace ServerCore;

class Program
{
    static Listener2 listener = new Listener2();

    static async Task OnAcceptHandler(Socket clientSocket)
    {
        try
        {
            Session2 session2 = new Session2();
            session2.start(clientSocket);

            byte[] sendBuff = Encoding.UTF8.GetBytes("Welcom to server");
            session2.Send(sendBuff);
            await session2.RegisterRecv();
            Thread.Sleep(100);
            session2.Disconnect();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    static async Task Main2(string[] args)
    {
        // DNS
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        try
        {
            Listener2 listener = new Listener2();

            listener.BackingNumber = 10;

            listener.Init(endPoint);


            while (true)
            {
                Console.WriteLine("listening...");

                Socket clientSocket = default!;
                try
                {
                    clientSocket = await listener.AcceptAsync();
                }
                catch (Exception e)
                {
                    await Console.Out.WriteLineAsync($"acceptAsync exception: {e.ToString()}");
                }
                await OnAcceptHandler(clientSocket);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}

