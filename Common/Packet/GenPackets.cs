using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using TCPServerCore;

public enum PacketID
{
    C_Chat = 1,
	S_Chat = 2,
	S_BroadcaseEnterGame = 3,
	C_LeaveGame = 4,
	S_BroadcastLeaveGame = 5,
	S_PlayerList = 6,
	C_Move = 7,
	S_BroadcastMove = 8,
	
}

public interface IPacket
{
    ushort Protocol { get; }
    void Read(ArraySegment<byte> segment);
    ArraySegment<byte> Write();
}


public class C_Chat : IPacket
{
    public string chat;

    public ushort Protocol { get { return (ushort)PacketID.C_Chat; } }
  

    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);
        ushort chatLen = (ushort)BitConverter.ToInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.name = Encoding.Unicode.GetString(s.Slice(count, chatLen));
		count += chatLen;
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
        ushort chatLen = (ushort)Encoding.Unicode.GetBytes(
		   this.chat, 0, this.chat.Length, segment.Array, segment.Offset + count + sizeof(ushort)
		  );
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), chatLen);
		count += sizeof(ushort);
		count += chatLen;

        success &= BitConverter.TryWriteBytes(s, count);

        if (success == false)
            return null;
        var sendBuff = SendBufferHelper.Close(count);
        return sendBuff;
    }
}


public class S_Chat : IPacket
{
    public int playerId;
	public string chat;

    public ushort Protocol { get { return (ushort)PacketID.S_Chat; } }
  

    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);
        this.playerId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		ushort chatLen = (ushort)BitConverter.ToInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		this.name = Encoding.Unicode.GetString(s.Slice(count, chatLen));
		count += chatLen;
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
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
		count += sizeof(int);
		ushort chatLen = (ushort)Encoding.Unicode.GetBytes(
		   this.chat, 0, this.chat.Length, segment.Array, segment.Offset + count + sizeof(ushort)
		  );
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), chatLen);
		count += sizeof(ushort);
		count += chatLen;

        success &= BitConverter.TryWriteBytes(s, count);

        if (success == false)
            return null;
        var sendBuff = SendBufferHelper.Close(count);
        return sendBuff;
    }
}


public class S_BroadcaseEnterGame : IPacket
{
    public int playerId;
	public float posX;
	public float posY;
	public float posZ;

    public ushort Protocol { get { return (ushort)PacketID.S_BroadcaseEnterGame; } }
  

    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);
        this.playerId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.posX = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.posY = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.posZ = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
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
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.posX);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.posY);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.posZ);
		count += sizeof(float);

        success &= BitConverter.TryWriteBytes(s, count);

        if (success == false)
            return null;
        var sendBuff = SendBufferHelper.Close(count);
        return sendBuff;
    }
}


public class C_LeaveGame : IPacket
{
    

    public ushort Protocol { get { return (ushort)PacketID.C_LeaveGame; } }
  

    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);
        
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
        

        success &= BitConverter.TryWriteBytes(s, count);

        if (success == false)
            return null;
        var sendBuff = SendBufferHelper.Close(count);
        return sendBuff;
    }
}


public class S_BroadcastLeaveGame : IPacket
{
    public int playerId;

    public ushort Protocol { get { return (ushort)PacketID.S_BroadcastLeaveGame; } }
  

    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);
        this.playerId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
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
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), Protocol);
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
		count += sizeof(int);

        success &= BitConverter.TryWriteBytes(s, count);

        if (success == false)
            return null;
        var sendBuff = SendBufferHelper.Close(count);
        return sendBuff;
    }
}


public class S_PlayerList : IPacket
{
    
	public struct Player
	{
	    public bool isSelf;
		public int playerId;
		public float posX;
		public float posY;
		public float posZ;
	
	    public void Read(ReadOnlySpan<byte> s, ref ushort count)
	    {
	        this.isSelf = BitConverter.ToBoolean(s.Slice(count, s.Length - count));
			count += sizeof(bool);
			this.playerId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
			count += sizeof(int);
			this.posX = BitConverter.ToSingle(s.Slice(count, s.Length - count));
			count += sizeof(float);
			this.posY = BitConverter.ToSingle(s.Slice(count, s.Length - count));
			count += sizeof(float);
			this.posZ = BitConverter.ToSingle(s.Slice(count, s.Length - count));
			count += sizeof(float);
	    }
	
	    public bool Write(Span<byte> s, ref ushort count)
	    {
	        bool success = true;
	        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.isSelf);
			count += sizeof(bool);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
			count += sizeof(int);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.posX);
			count += sizeof(float);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.posY);
			count += sizeof(float);
			success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.posZ);
			count += sizeof(float);
	
	        return success;
	    }
	    public Player()
	    {}
	}
	public List<Player> players = new();
	

    public ushort Protocol { get { return (ushort)PacketID.S_PlayerList; } }
  

    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);
        this.players.Clear();
		ushort playerLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
		count += sizeof(ushort);
		for (int i = 0; i < playerLen; i++)
		{
		    Player player = new();
		    player.Read(s, ref count);
		    players.Add(player);
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
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.players.Count);
		count += sizeof(ushort);
		foreach (var player in players)
		{
		    // TODO
		    player.Write(s, ref count);
		}
		

        success &= BitConverter.TryWriteBytes(s, count);

        if (success == false)
            return null;
        var sendBuff = SendBufferHelper.Close(count);
        return sendBuff;
    }
}


public class C_Move : IPacket
{
    public float posX;
	public float posY;
	public float posZ;

    public ushort Protocol { get { return (ushort)PacketID.C_Move; } }
  

    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);
        this.posX = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.posY = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.posZ = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
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
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.posX);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.posY);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.posZ);
		count += sizeof(float);

        success &= BitConverter.TryWriteBytes(s, count);

        if (success == false)
            return null;
        var sendBuff = SendBufferHelper.Close(count);
        return sendBuff;
    }
}


public class S_BroadcastMove : IPacket
{
    public int playerId;
	public float posX;
	public float posY;
	public float posZ;

    public ushort Protocol { get { return (ushort)PacketID.S_BroadcastMove; } }
  

    public void Read(ArraySegment<byte> segment)
    {
        ushort count = 0;
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);
        this.playerId = BitConverter.ToInt32(s.Slice(count, s.Length - count));
		count += sizeof(int);
		this.posX = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.posY = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
		this.posZ = BitConverter.ToSingle(s.Slice(count, s.Length - count));
		count += sizeof(float);
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
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
		count += sizeof(int);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.posX);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.posY);
		count += sizeof(float);
		success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.posZ);
		count += sizeof(float);

        success &= BitConverter.TryWriteBytes(s, count);

        if (success == false)
            return null;
        var sendBuff = SendBufferHelper.Close(count);
        return sendBuff;
    }
}


