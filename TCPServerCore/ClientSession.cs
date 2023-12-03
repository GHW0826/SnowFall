using System.Net;
using TCPServerExample.Packet;

namespace TCPServerCore;

public abstract class Packet
{
    public ushort size;
    public ushort packetId;

    public abstract ArraySegment<byte> Write();
    public abstract void Read(ArraySegment<byte> s);
}



public class ClientSession : PacketSession
{

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        ushort count = 0;
        // [size(2)][packetId(2)][ ... ]
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        switch ((PacketID)id)
        {
            case PacketID.PlayerInfoReq:
                {
                    count += 8;
                    PlayerInfoReq p = new();
                    p.Read(buffer);
                    Console.WriteLine($"PlayerInfoReq: {p.playerId}: {p.name}");
                    foreach(var skill in p.skills)
                    {
                        Console.WriteLine($"skill info: {skill.id}: {skill.level} : {skill.duration}");
                        foreach (var att in skill.attributes)
                        {
                            Console.WriteLine($"attr : {att.attr}");
                        }
                    }
                }
                break;
        }


        Console.WriteLine($"RecvPacketId: {id}, Size {size}");
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
