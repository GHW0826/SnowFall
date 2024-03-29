﻿using System;
using System.Net;
using System.Net.Sockets;

namespace TCPServerCore
{
    public class Listener
    {
        Socket ListenerSocket;
        Func<Session> _sessionFactory;

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

            try
            {
                bool pending = ListenerSocket.AcceptAsync(args);
                if (pending == false)
                    OnAcceptCompleted(null, args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        void OnAcceptCompleted(object? sender, SocketAsyncEventArgs args)
        {
            try
            {
                if (args.SocketError == SocketError.Success)
                {
                    Session session = _sessionFactory.Invoke();
                    session.Start(args.AcceptSocket);
                    session.OnConnected(args.AcceptSocket.RemoteEndPoint);
                }
                else
                    Console.WriteLine(args.SocketError.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            RegisterAccept(args);
        }
    }
}
