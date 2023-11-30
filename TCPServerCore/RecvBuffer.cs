using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServerCore;

public class RecvBuffer
{
    // [ ReadSegment ][   WriteSegment    ]
    // [r][ ][][][][ ][w][ ][ ][ ][ ][ ][ ]
    // [  Data Size  ][      Free Size    ]
    ArraySegment<byte> _buffer;
    int _readPos;
    int _writePos;

    public RecvBuffer(int bufferSize)
    {
        _buffer = new (new byte[bufferSize], 0, bufferSize);
    }

    public int DataSize { get { return _writePos - _readPos; } }
    public int FreeSize { get { return _buffer.Count - _writePos; } }

    public ArraySegment<byte> ReadSegment
    {
        get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
    }
    public ArraySegment<byte> WriteSegment
    {
        get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
    }

    public void Clean()
    {
        int dataSize = DataSize;
        if (dataSize == 0)
        {
            // 남은 데이터가 없으면 복사하지 않고 커서 위치만 변경
            _readPos = _writePos = 0;
        }
        else
        {
            // 남은 더미가 있으면 시작 위치로 복사
            Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
            _readPos = 0;
            _writePos = dataSize;
        }
    }

    // 컨텐츠코드에서 데이터 가공 처리후 성공적일때 호출
    // 커서 위치 변경
    public bool OnRead(int numOfByte)
    {
        if (numOfByte > DataSize)
            return false;
        _readPos += numOfByte;
        return true;
    }

    // recv를 했을때 write 커서를 옮겨주는 역할
    public bool OnWrite(int numOfByte)
    {
        if (numOfByte > FreeSize)
            return false;
        _writePos += numOfByte;
        return true;
    }
}
