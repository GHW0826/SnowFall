using System.Net;
using System.Net.Sockets;

namespace TCPServerCore.leature;

public class Listener
{
    public int BackingNumber { get; set; }

    public Socket? ListenerSocket;
    Action<Socket>? OnAcceptHandler;

    public void Init(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
    {
        ListenerSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        OnAcceptHandler += onAcceptHandler;

        // bind
        ListenerSocket.Bind(endPoint);

        // listen
        // backing : 최대 대기수
        ListenerSocket.Listen(BackingNumber);

        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
        RegisterAccept(args);
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
