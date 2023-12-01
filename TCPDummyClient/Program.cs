
using System.Net;
using System.Net.Sockets;
using System.Text;
using TCPServerCore;

class Packet
{
    public ushort size;
    public ushort packetId;
}


class GameSession : Session
{
    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected: {endPoint}");

        Packet packet = new Packet() { size = 4, packetId = 7 };

        try
        {
            for (int i = 0; i < 5; i++)
            {
                var openSegment = SendBufferHelper.Open(4096);
                byte[] buffer1 = BitConverter.GetBytes(packet.size);
                byte[] buffer12 = BitConverter.GetBytes(packet.packetId);
                Array.Copy(buffer1, 0, openSegment.Array, openSegment.Offset, buffer1.Length);
                Array.Copy(buffer12, 0, openSegment.Array, openSegment.Offset + buffer1.Length, buffer12.Length);
                var sendSegment = SendBufferHelper.Close(packet.size);

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