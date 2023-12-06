using System.Net;
using System.Net.Sockets;

namespace TCPServerCore;

public class Listener
{
    public Socket? ListenerSocket;
    public Func<Session>? _sessionFactory;

    public void Init(IPEndPoint endPoint, Func<Session> sessionFactory, int register = 10, int backlog = 100)
    {
        ListenerSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _sessionFactory += sessionFactory;

        // bind
        ListenerSocket.Bind(endPoint);

        // listen
        // backing : 최대 대기수
        ListenerSocket.Listen(backlog);

        for (int i = 0; i < register; i++)
        {
            // accept
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);
        }
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
            if (args.AcceptSocket != null && _sessionFactory != null)
            {
                Session session = _sessionFactory.Invoke();
                session.start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
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
