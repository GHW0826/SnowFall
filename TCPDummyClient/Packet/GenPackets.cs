using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPServerCore;

namespace TCPServerExample.Packet;


public enum PacketID
{
    PlayerInfoReq = 1,
    Test = 2,

}

public interface IPacket
{
   ushort Protocol { get; }
    void Read(ArraySegment<byte> segment);
    ArraySegment<byte> Write();
}


public class PlayerInfoReq : IPacket
{
    public byte testByte;
    public long playerId;
    public string name;

    public ushort Protocol { get { return (ushort)PacketID.PlayerInfoReq; } }

    public struct Skill
    {
        public int id;
        public short level;
        public float duration;

        public struct Attribute
        {
            public int attr;

            public void Read(ReadOnlySpan<byte> s, ref ushort count)
            {
                this.attr = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                count += sizeof(int);
            }

            public bool Write(Span<byte> s, ref ushort count)
            {
                bool success = true;
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.attr);
                count += sizeof(int);

                return success;
            }
            public Attribute()
            { }
        }
        public List<Attribute> attributes = new();


        public void Read(ReadOnlySpan<byte> s, ref ushort count)
        {
            this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
            count += sizeof(int);
            this.level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
            count += sizeof(short);
            this.duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
            count += sizeof(float);
            this.attributes.Clear();
            ushort attributeLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);
            for (int i = 0; i < attributeLen; i++)
            {
                Attribute attribute = new();
                attribute.Read(s, ref count);
                attributes.Add(attribute);
            }

        }

        public bool Write(Span<byte> s, ref ushort count)
        {
            bool success = true;
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.id);
            count += sizeof(int);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.level);
            count += sizeof(short);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.duration);
            count += sizeof(float);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.attributes.Count);
            count += sizeof(ushort);
            foreach (var attribute in attributes)
            {
                // TODO
                attribute.Write(s, ref count);
            }


            return success;
        }
        public Skill()
        { }
    }
    public List<Skill> skills = new();


    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);
        this.testByte = (byte)segment.Array[segment.Offset + count];
        count += sizeof(byte);
        this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
        count += sizeof(long);
        ushort nameLen = (ushort)BitConverter.ToInt16(s.Slice(count, s.Length - count));
        count += sizeof(ushort);
        this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
        count += nameLen;
        this.skills.Clear();
        ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
        count += sizeof(ushort);
        for (int i = 0; i < skillLen; i++)
        {
            Skill skill = new();
            skill.Read(s, ref count);
            skills.Add(skill);
        }

    }

    public ArraySegment<byte> Write()
    {
        // openSegment
        var segment = SendBufferHelper.Open(4096);
        ushort count = 0;
        bool success = true;
        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), Protocol);
        count += sizeof(ushort);
        segment.Array[segment.Offset + count] = (byte)this.testByte;
        count += sizeof(byte);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
        count += sizeof(long);
        ushort nameLen = (ushort)Encoding.Unicode.GetBytes(
           this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort)
          );
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
        count += sizeof(ushort);
        count += nameLen;
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.skills.Count);
        count += sizeof(ushort);
        foreach (var skill in skills)
        {
            // TODO
            skill.Write(s, ref count);
        }


        success &= BitConverter.TryWriteBytes(s, count);

        if (success == false)
            return null;
        var sendBuff = SendBufferHelper.Close(count);
        return sendBuff;
    }
}


public class Test
{
    public int testInt;

    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);
        this.testInt = BitConverter.ToInt32(s.Slice(count, s.Length - count));
        count += sizeof(int);
    }

    public ArraySegment<byte> Write()
    {
        // openSegment
        var segment = SendBufferHelper.Open(4096);
        ushort count = 0;
        bool success = true;
        Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.Test);
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.testInt);
        count += sizeof(int);

        success &= BitConverter.TryWriteBytes(s, count);

        if (success == false)
            return null;
        var sendBuff = SendBufferHelper.Close(count);
        return sendBuff;
    }
}