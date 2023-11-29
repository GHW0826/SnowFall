using System.Net;
using System.Net.Sockets;

namespace TCPServerCore;

public class Listener2
{
    public int BackingNumber { get; set; }

    public Socket? ListenerSocket;
    Action<Socket>? OnAcceptHandler;

    public void Init(IPEndPoint endPoint)
    {
        ListenerSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        // bind
        ListenerSocket.Bind(endPoint);

        // listen
        // backing : 최대 대기수
        ListenerSocket.Listen(BackingNumber);

        /*
        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
        RegisterAccept(args);
        */
    }

    public async Task<Socket> AcceptAsync()
    {
        return await ListenerSocket.AcceptAsync();
    }

    void RegisterAccept(SocketAsyncEventArgs args)
    {
        args.AcceptSocket = null;
        if (ListenerSocket != null)
        {
            bool pending = ListenerSocket.AcceptAsync(args);
            if (pending == false)
            {
                OnAcceptCompleted(null, args);
            }
        }
    }

    void OnAcceptCompleted(object? sender, SocketAsyncEventArgs args)
    {
        if (args.SocketError == SocketError.Success)
        {
            // TODO
            if (args.AcceptSocket != null && OnAcceptHandler != null)
                OnAcceptHandler.Invoke(args.AcceptSocket);
        }
        else
            Console.WriteLine(args.SocketError.ToString());

        RegisterAccept(args);
    }



    /// <summary>
    /// sync
    /// </summary>
    /// <returns></returns>
    public Socket Accept()
    {
        return ListenerSocket.Accept();
    }
}
