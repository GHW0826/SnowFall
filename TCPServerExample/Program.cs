

using System.Net;
using System.Text;
using TCPServerCore;

namespace TCPServerExample;


class Packet
{
    public ushort size;
    public ushort packetId;
}

class LoginOkPacket : Packet 
{
}


class GameSession : PacketSession
{
    

    class Knight
    {
        public int hp;
        public int attack;
        public string name;
        public List<int> skills = new();
    }

    public void OnRecvPacket(ArraySegment<byte> buffer)
    {
        // [size(2)][packetId(2)][ ... ]
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2);
        Console.WriteLine($"RecvPacketId: {id}, Size {size}");
    }

    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected: {endPoint}");


        try
        {
            Knight knight = new Knight() { hp = 100, attack = 10 };
            byte[] sendBuff = new byte[1024];
            byte[] buffer = BitConverter.GetBytes(knight.hp);
            byte[] buffer2 = BitConverter.GetBytes(knight.attack);
            Array.Copy(buffer, 0, sendBuff, 0, buffer.Length);
            Array.Copy(buffer2, 0, sendBuff, buffer.Length, buffer2.Length);

            var openSegment = SendBufferHelper.Open(4096);
            byte[] buffer1 = BitConverter.GetBytes(knight.hp);
            byte[] buffer12 = BitConverter.GetBytes(knight.attack);
            Array.Copy(buffer1, 0, openSegment.Array, openSegment.Offset, buffer1.Length);
            Array.Copy(buffer12, 0, openSegment.Array, openSegment.Offset + buffer1.Length, buffer12.Length);
            var sendSegment = SendBufferHelper.Close(buffer1.Length + buffer12.Length);

            // byte[] sendBuff = Encoding.UTF8.GetBytes("Welcom to server");
            Send(sendSegment);

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
    /*
    public override int OnRecv(ArraySegment<byte> buffer)
    {
        string recvString = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
        Console.WriteLine($"[from client]: {recvString}");
        return buffer.Count;
    }
    */
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

