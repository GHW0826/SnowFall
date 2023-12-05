using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPServerCore;

public abstract class PacketSession : Session
{
    public static readonly int HeaderSize = 2;

    // [size(2)][packetId(2)][ ... ][size(2)][packetId(2)][ ... ]
    public sealed override int OnRecv(ArraySegment<byte> buffer)
    {
        int processLen = 0;
        while (true)
        {
            // 최소한 헤더는 파싱할 수 있는지 확인
            if (buffer.Count < HeaderSize)
            {
                break;
            }

            // 패킷이 완전체로 도착했는지 확인
            // [size(2)][packetId(2)][ ... ] 
            ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            if (buffer.Count < dataSize)
            {
                break;
            }

            // 여기까지 왔으면 패킷 조립 가능
            OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));

            // 다음 [size(2)][packetId(2)][ ... ]를 찝어줌
            processLen += dataSize;
            buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
        }

        return processLen;
    }

    public abstract void OnRecvPacket(ArraySegment<byte> buffer);
}


public abstract class Session
{
    object _lock = new();
    // bool _pending = false;

    RecvBuffer _recvBuffer = new RecvBuffer(1024);

    Socket _socket;
    int _disconnected = 0;

    Queue<ArraySegment<byte>> _sendQueue = new();
    List<ArraySegment<byte>> _pendingList = new();
    SocketAsyncEventArgs _sendArgs = new();
    SocketAsyncEventArgs _recvArgs = new();

    // 1. Event Handler
    /*
    class SessionHandler
    {
        public void OnConnected(EndPoint endPoint) { }
        public void OnRecv(ArraySegment<byte> buffer) { }
        public void OnSend(int numOfBytes) { }
        public void OnDisConnected(EndPoint endPoint) { }
    }
    */

    // 2. Session 상속 (choice)
    public abstract void OnConnected(EndPoint endPoint);

    // return: 처리량
    public abstract int OnRecv(ArraySegment<byte> buffer);
    public abstract void OnSend(int numOfBytes);
    public abstract void OnDisConnected(EndPoint endPoint);

    void Clear()
    {
        lock (_lock)
        {
            _sendQueue.Clear();
            _pendingList.Clear();
        }
    }

    public void start(Socket socket)
    {
        _socket = socket;

        _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
        _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

        RegisterRecv();
    }

    public void Send(ArraySegment<byte> sendBuff)
    {
        lock (_lock)
        {
            _sendQueue.Enqueue(sendBuff);
            if (_pendingList.Count == 0)
            {
                RegisterSend();
            }
        }
    }

    public void Disconnect()
    {
        if (Interlocked.Exchange(ref _disconnected, 1) == 1)
            return;

        OnDisConnected(_socket.RemoteEndPoint);

        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
        Clear();
    }

    #region Network

    void RegisterSend()
    {
        // Send에서 락을 걸어놔서 안걸어도 됨.

        // var buff = _sendQueue.Dequeue();
        // _sendArgs.SetBuffer(buff, 0, buff.Length);
        if (_disconnected == 1)
            return;

        while (_sendQueue.Count > 0)
        {
            var buff = _sendQueue.Dequeue();
            _pendingList.Add(buff);
        }
        _sendArgs.BufferList = _pendingList;

        try
        {
            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
            {
                OnSendCompleted(null, _sendArgs);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RegisterSend Failed {ex}");
        }
    }

    void OnSendCompleted(object? sender, SocketAsyncEventArgs args)
    {
        // _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
        // 의 경우가 있어 락 걸어줘야함
        lock (_lock)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    _sendArgs.BufferList = null;
                    _pendingList.Clear();

                    OnSend(_sendArgs.BytesTransferred);

                    if (_sendQueue.Count > 0)
                    {
                        RegisterSend();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"OnSendCompleted Faile: {ex.ToString()}");
                }
            }
            else
            {
                Disconnect();
            }
        }
    }


    void RegisterRecv()
    {
        if (_disconnected == 1)
            return;

        _recvBuffer.Clean();
        var segment = _recvBuffer.WriteSegment;
        _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

        try
        {
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (pending == false)
            {
                OnRecvCompleted(null, _recvArgs);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RegisterRecv Failed {ex}");
        }
    }
    void OnRecvCompleted(object? state, SocketAsyncEventArgs args)
    {
        if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
        {
            try
            {
                // write cursor move
                if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                {
                    Disconnect();
                    return;
                }

                // 컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받음.
                int processLen = OnRecv(_recvBuffer.ReadSegment);
                if (processLen < 0 || _recvBuffer.DataSize < processLen)
                {
                    Disconnect();
                    return;
                }

                // Read 커서 이동
                if (_recvBuffer.OnRead(processLen) == false)
                {
                    Disconnect();
                    return;
                }

                RegisterRecv();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnRecvCompleted Faile: {ex.ToString()}");
            }
        }
        else
        {
            Disconnect();
        }
    }

    #endregion
}
