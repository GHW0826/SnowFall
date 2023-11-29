using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPServerCore.leature;

internal class Session
{
    object _lock = new ();
    Queue<byte[]> _sendQueue = new();
    // bool _pending = false;

    Socket _socket;
    int _disconnected = 0;

    List<ArraySegment<byte>> _pendingList = new();
    SocketAsyncEventArgs _sendArgs = new ();
    SocketAsyncEventArgs _recvArgs = new ();

    public void start(Socket socket)
    {
        _socket = socket;

        _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
        _recvArgs.SetBuffer(new byte[1024], 0, 1024);

        _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

        RegisterRecv();
    }

    public void Send(byte[] sendBuff)
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
            byte[] buff = _sendQueue.Dequeue();
            _pendingList.Add(new ArraySegment<Byte>(buff, 0, buff.Length));
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

                    Console.WriteLine($"BytesTransferred: {_sendArgs.BytesTransferred}");

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
            // TODO
            try
            {
                string recvString = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                Console.WriteLine($"[from client]: {recvString}");

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
