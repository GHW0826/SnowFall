

using System.Net;
using System.Net.Sockets;
using System.Text;
using TCPServerCore;

namespace TCPServerCore.leature;

class Program2
{
    static Listener listener = new Listener();

    static void OnAcceptHandler(Socket clientSocket)
    {
        try
        {

            Session session = new Session();
            session.start(clientSocket);

            byte[] sendBuff = Encoding.UTF8.GetBytes("Welcom to server");
            session.Send(sendBuff);

            Thread.Sleep(100);
            session.Disconnect();

            /*
            // Receive
            byte[] recvBuff = new byte[1024];
            int recvBytes = clientSocket.Receive(recvBuff);
            string recvString = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
            Console.WriteLine($"[from client]: {recvString}");

            // send
            byte[] sendBuff = Encoding.UTF8.GetBytes("Welcom to server");
            clientSocket.Send(sendBuff);

            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
            */
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    static async Task Main(string[] args)
    {
        // DNS
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        try
        {
            Listener listener = new Listener();

            listener.BackingNumber = 10;
            Console.WriteLine("listening...");
            // 
            listener.Init(endPoint, OnAcceptHandler);
            while (true)
            {
                // accept
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}

