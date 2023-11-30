
using System.Net;
using System.Net.Sockets;
using System.Text;
using TCPServerCore;

class GameSession : Session
{
    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected: {endPoint}");


        try
        {
            for (int i = 0; i < 5; ++i)
            {
                // send
                byte[] sendBuff = Encoding.UTF8.GetBytes($"Hello World server:{i} ");
               Send(sendBuff);
            }
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
    
    // ex) 이동 패킷 (3, 2) 좌표로 이동 (15번 패킷)
    // 15 3 2
    public override int OnRecv(ArraySegment<byte> buffer)
    {
        string recvString = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
        Console.WriteLine($"[from server]: {recvString}");
        return buffer.Count;
    }

    public override void OnSend(int numOfBytes)
    {
        Console.WriteLine($"BytesTransferred: {numOfBytes}");
    }
}


class Program
{
    static void Main(string[] args)
    {
        // DNS
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        Connector connector = new Connector();

       connector.Connect(endPoint, () => { return new GameSession(); });
       while (true)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}