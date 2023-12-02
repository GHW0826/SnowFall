using System.Net;
using System.Text;

namespace TCPServerCore;

public abstract class Packet
{
    public ushort size;
    public ushort packetId;

    public abstract ArraySegment<byte> Write();
    public abstract void Read(ArraySegment<byte> s);
}

public class PlayerInfoReq : Packet
{
    public long playerId;
    public string name;

    public List<SkillInfo> skills = new();

    public struct SkillInfo
    {
        public int id;
        public short level;
        public float duration;

        public bool Write(Span<byte> s, ref ushort count)
        {
            bool success = true;
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), id);
            count += sizeof(int);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), level);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), duration);
            count += sizeof(float);

            return success;
        }

        public void Read(ReadOnlySpan<byte> s, ref ushort count)
        {
            id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
            count += sizeof(int);
            level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
            count += sizeof(short);
            duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
            count += sizeof(float);
        }
    }


    public PlayerInfoReq()
    {
        this.packetId = (ushort)PacketID.PlayerInfoReq;
    }

    public override void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        //ushort size = BitConverter.ToUInt16(s.Array, s.Offset);
        count += sizeof(ushort);
        //ushort id = BitConverter.ToUInt16(s.Array, s.Offset + count);
        count += sizeof(ushort);
        long playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
        count += sizeof(long);


        ushort nameLen = (ushort)BitConverter.ToInt16(s.Slice(count, s.Length - count));
        count += sizeof(ushort);
        this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
        count += nameLen;

        // skill list
        ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
        count += sizeof(ushort);
        skills.Clear();
        for (int i = 0; i < skillLen; i++)
        {
            SkillInfo skill = new SkillInfo();
            skill.Read(s, ref count);
            skills.Add(skill);
        }

        this.playerId = playerId;
    }

    public override ArraySegment<byte> Write()
    {
        // openSegment
        var segment = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool success = true;
        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);
        // success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), packet.size); 
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.packetId);
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
        count += sizeof(long);

        // string
        // string len[2]
        // byte []
        // UTF16을 이용한 길이
        ushort nameLen = (ushort)Encoding.Unicode.GetByteCount(this.name);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
        count += sizeof(ushort);
        Array.Copy(Encoding.Unicode.GetBytes(this.name), 0, segment.Array, count, nameLen);
        count += nameLen;



        // skillList
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)skills.Count);
        count += sizeof(ushort);
        foreach (var skill in skills)
        {
            // TODO
            skill.Write(s, ref count);
        }


        // count
        success &= BitConverter.TryWriteBytes(s, count);

        if (success == false)
            return null;
        var sendBuff = SendBufferHelper.Close(count);
        return sendBuff;
    }
}

public enum PacketID
{
    PlayerInfoReq = 1,
    PlayerInfoOk = 2
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
