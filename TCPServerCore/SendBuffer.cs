using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServerCore;

public class SendBufferHelper
{
    public static ThreadLocal<SendBuffer> CurrentBuffer =new ThreadLocal<SendBuffer>(() => { return new (4096); });

    public static int ChunkSize { get; set; } = 65535 * 100;

    public static ArraySegment<byte> Open(int reserveSize)
    {
        if (CurrentBuffer.Value == null)
        {
            CurrentBuffer.Value = new(ChunkSize);
        }

        if (CurrentBuffer.Value.FreeSize < reserveSize)
        {
            CurrentBuffer.Value = new (ChunkSize);
        }
        return CurrentBuffer.Value.Open(reserveSize);
    }
    public static ArraySegment<byte> Close(int usedSize)
    {
        return CurrentBuffer.Value.Close(usedSize);
    }
}

public class SendBuffer
{
    // [u][][][][][][][][][]
    byte[] _buffer;
    int _usedSize = 0;

    public int FreeSize { get { return _buffer.Length - _usedSize; } }
    
    public SendBuffer(int chunkSize)
    {
        _buffer = new byte[chunkSize];
    }

    public ArraySegment<byte> Open(int reserveSize)
    {
        if (reserveSize > FreeSize)
            return null;

        return new(_buffer, _usedSize, reserveSize);
    }
    public ArraySegment<byte> Close(int usedSize)
    {
        ArraySegment<byte> segment = new (_buffer, _usedSize, usedSize);
        _usedSize += usedSize;
        return segment;
    }
}
