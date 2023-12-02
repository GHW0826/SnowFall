using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TCPServerCore;

namespace TCPDummyClient;
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


    public PlayerInfoReq ()
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
        // ushort nameLen = (ushort)Encoding.Unicode.GetByteCount(this.name);
        // success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
        // count += sizeof(ushort);
        // Array.Copy(Encoding.Unicode.GetBytes(this.name), 0, segment.Array, count, nameLen);
        // count += nameLen;
        ushort nameLen = (ushort)Encoding.Unicode.GetBytes(
            this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort)
            );
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
        count += sizeof(ushort);
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


public class ServerSession : Session
{

    static unsafe void ToBytes(byte[] array, int offset, ulong value)
    {
        fixed (byte* ptr = &array[offset])
        {
            *(ulong*)ptr = value;
        }
    }


    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected: {endPoint}");

        // Packet packet = new Packet() { size = 4, packetId = 7 };
        PlayerInfoReq packet = new PlayerInfoReq()
        {
            playerId = 1001,
            name = "test_plater",
            skills = new() { 
                new() { id = 1, level = 5, duration = 0.1f },
                new() { id = 2, level = 10, duration = 20.1f },
                new() { id = 3, level = 15, duration = 15.1f }

            }
        };

        try
        {
            // for (int i = 0; i < 5; i++)
            {
                var sendBuff = packet.Write();
                if (sendBuff != null)
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
