using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPServerCore;

internal class Session2
{
    Socket _socket;
    int _disconnected = 0;

    public void start(Socket socket)
    {
        _socket = socket;
    }

    public void Send(byte[] sendBuff)
    {
        _socket.Send(sendBuff);
    }

    public void Disconnect()
    {

        if (Interlocked.Exchange(ref _disconnected, 1) == 1)
            return;

        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
    }

    public async Task RegisterRecv()
    {
        ArraySegment<byte> segment = new ArraySegment<byte>(new byte[1024], 0, 1024);
        var bytesTransferred = await _socket.ReceiveAsync(segment);
        if (bytesTransferred > 0)
        {
            try
            {
                string recvString = Encoding.UTF8.GetString(segment.Array, 0, bytesTransferred);
                Console.WriteLine($"[from client]: {recvString}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnRecvCompleted Faile: {ex.ToString()}");
            }
        }
    }
}
