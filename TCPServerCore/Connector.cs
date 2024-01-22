using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCPServerCore
{
    public class Connector
    {
        Func<Session> _sessionFactory;

        public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _sessionFactory = sessionFactory;

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += OnConnectComplete;
                args.RemoteEndPoint = endPoint;
                args.UserToken = socket;

                RegisterConnect(args);

                Thread.Sleep(30);
            }
        }

        void RegisterConnect(SocketAsyncEventArgs args)
        {
            Socket socket = args.UserToken as Socket;
            if (socket == null)
                return;

            try
            {
                bool pending = socket.ConnectAsync(args);
                if (pending == false)
                    OnConnectComplete(null, args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        void OnConnectComplete(object? sender, SocketAsyncEventArgs args)
        {
            try
            {
                if (args.SocketError == SocketError.Success)
                {
                    Session session = _sessionFactory.Invoke();
                    session.Start(args.ConnectSocket);
                    session.OnConnected(args.RemoteEndPoint);
                }
                else
                {
                    Console.WriteLine($"OnConnectComplete socket error: {args.SocketError}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
