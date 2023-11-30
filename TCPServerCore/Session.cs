using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPServerCore;

public abstract class Session
{
    object _lock = new();
    Queue<ArraySegment<byte>> _sendQueue = new();
    // bool _pending = false;

    RecvBuffer _recvBuffer = new RecvBuffer(1024);

    Socket _socket;
    int _disconnected = 0;

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
    }

    #region Network

    void RegisterSend()
    {
        // Send에서 락을 걸어놔서 안걸어도 됨.

        // var buff = _sendQueue.Dequeue();
        // _sendArgs.SetBuffer(buff, 0, buff.Length);

        while (_sendQueue.Count > 0)
        {
            var buff = _sendQueue.Dequeue();
            _pendingList.Add(buff);
        }
        _sendArgs.BufferList = _pendingList;
        bool pending = _socket.SendAsync(_sendArgs);
        if (pending == false)
        {
            OnSendCompleted(null, _sendArgs);
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
        _recvBuffer.Clean();
        var segment = _recvBuffer.WriteSegment;
        _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

        bool pending = _socket.ReceiveAsync(_recvArgs);
        if (pending == false)
        {
            OnRecvCompleted(null, _recvArgs);
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
