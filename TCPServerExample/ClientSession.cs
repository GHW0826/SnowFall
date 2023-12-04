using System.Net;
using TCPServerCore;
using TCPServerExample.Packet;

namespace TCPServerExample;


public class ClientSession : PacketSession
{

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        PacketManager.Instance.OnRecvPacket(this, buffer);
    }

    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected: {endPoint}");


        try
        {
            /*
            
                class Knight
                {
                    public int hp;
                    public int attack;
                    public string name;
                    public List<int> skills = new();
                }

            // Knight knight = new Knight() { hp = 100, attack = 10 };
            // byte[] sendBuff = new byte[1024];
            // byte[] buffer = BitConverter.GetBytes(knight.hp);
            // byte[] buffer2 = BitConverter.GetBytes(knight.attack);
            // Array.Copy(buffer, 0, sendBuff, 0, buffer.Length);
            // Array.Copy(buffer2, 0, sendBuff, buffer.Length, buffer2.Length);
            // 
            // var openSegment = SendBufferHelper.Open(4096);
            // byte[] buffer1 = BitConverter.GetBytes(knight.hp);
            // byte[] buffer12 = BitConverter.GetBytes(knight.attack);
            // Array.Copy(buffer1, 0, openSegment.Array, openSegment.Offset, buffer1.Length);
            // Array.Copy(buffer12, 0, openSegment.Array, openSegment.Offset + buffer1.Length, buffer12.Length);
            // var sendSegment = SendBufferHelper.Close(buffer1.Length + buffer12.Length);

            // byte[] sendBuff = Encoding.UTF8.GetBytes("Welcom to server");
            // Send(sendSegment);
            */

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
