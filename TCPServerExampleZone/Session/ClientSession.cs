﻿using System;
using System.Net;
using Google.Protobuf.Protocol;
using Google.Protobuf;
using TCPServerCore;
using TCPServerExampleZone.Game;
using System.Collections.Generic;

namespace TCPServerExampleZone
{
    public partial class ClientSession : PacketSession
    {
        public PlayerServerState ServerState { get; private set; } = PlayerServerState.ServerStateLogin;

        public Player MyPlayer { get; set; }
        public int SessionId { get; set; }

        object _lock = new();
        List<ArraySegment<byte>> _reserveQueue = new();

        // 패킷 모아 보내기
        int _reservedSendBytes = 0;
        long _lastSendTick = 0;

        long _pingpongTick = 0;
        public void Ping()
        {
            if (_pingpongTick > 0)
            {
                long delta = (System.Environment.TickCount64 - _pingpongTick);
                if (delta > 30 * 1000)
                {
                    //Console.WriteLine("Disconnected by PingCheck");
                    Disconnect();
                    return;
                }
            }

            S_Ping pingPacket = new();
            Send(pingPacket);

            GameLogic.Instance.PushAfter(5000, Ping);
        }

        public void HandlerPong()
        {
            _pingpongTick = System.Environment.TickCount64;
        }

        #region Network

        // 예약만 하고 보내지는 않음
        public void Send(IMessage packet)
        {
            string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
            MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);
            ushort size = (ushort)packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 4];
            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

            lock (_lock)
            {
                _reserveQueue.Add(sendBuffer);
                _reservedSendBytes += sendBuffer.Length;
            }
            // Send(new ArraySegment<byte>(sendBuffer));
        }

        // 실제 Network IO를 보내는 부분
        public void FlushSend()
        {
            List<ArraySegment<byte>> sendList = null;
            lock (_lock)
            {
                // 0.1초가 지났거나, 패킷이 (1만 바이트 모였을때 보냄)
                long delta = (System.Environment.TickCount - _lastSendTick);
                if (delta < 100 && _reservedSendBytes < 10000)
                    return;
                if (_reserveQueue.Count == 0)
                    return;

                // 패킷 모아 보내기
                _reservedSendBytes = 0;
                _lastSendTick = System.Environment.TickCount;

                sendList = _reserveQueue;
                _reserveQueue = new();
            }

            Send(sendList);
        }

        public override void OnConnected(EndPoint endPoint)
        {
           // Console.WriteLine($"OnConnected : {endPoint}");

            {
                S_Connected connectedPacket = new S_Connected();
                Send(connectedPacket);
            }

            GameLogic.Instance.PushAfter(5000, Ping);
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            GameLogic.Instance.Push(() =>
            {
                if (MyPlayer == null)
                    return;
                GameRoom room = GameLogic.Instance.Find(1);
                room.Push(room.LeaveGame, MyPlayer.Info.ObjectId);
            });

            SessionManager.Instance.Remove(this);

            //Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            //Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
        #endregion
    }
}
