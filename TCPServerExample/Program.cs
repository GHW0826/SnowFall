

using System.Net;
using System.Text;
using TCPServerCore;

namespace TCPServerExample;

class GameSession : Session
{
    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected: {endPoint}");


        try
        {
            byte[] sendBuff = Encoding.UTF8.GetBytes("Welcom to server");
            Send(sendBuff);

            Thread.Sleep(100);
            Disconnect();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

    }

    public override void OnDisConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnDisconnected: {endPoint}");
    }

    public override void OnRecv(ArraySegment<byte> buffer)
    {
        string recvString = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
        Console.WriteLine($"[from client]: {recvString}");
    }

    public override void OnSend(int numOfBytes)
    {
        Console.WriteLine($"BytesTransferred: {numOfBytes}");
    }
}


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

            listener.BackingNumber = 10;
            Console.WriteLine("listening...");

            listener.Init(endPoint, () => { return new GameSession(); });
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

