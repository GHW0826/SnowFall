
using System;
using System.Collections.Generic;
using TCPServerCore;
using TCPServerExample.Session;

namespace TCPServerExample.Drecated
{
    public class GameRoom : IJobQueue
    {
        List<ClientSession> _sessions = new();
        List<ArraySegment<byte>> _pendingList = new();

        // Zone 방식은 zone 마다 JobQueue
        // Seamless 방식은 오브젝트마다 JobQueue를 생성해 각자 처리
        JobQueue _jobQueue = new();

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }
        public void Flush()
        {
            // N ^ 2
            foreach (ClientSession s in _sessions)
                s.Send(_pendingList);

            //Console.WriteLine($"Flushed {_pendingList.Count} items");
            _pendingList.Clear();
        }

        /*
        public void Move(ClientSession clientSession, C_Move packet)
        {
            // 좌표 바꿔주고
            clientSession.PosX = packet.posX;
            clientSession.PosY = packet.posY;
            clientSession.PosZ = packet.posZ;

            // 모두에게 알린다
            S_BroadcastMove move = new S_BroadcastMove();
            move.playerId = clientSession.SessionId;
            move.posX = clientSession.PosX;
            move.posY = clientSession.PosY;
            move.posZ = clientSession.PosZ;
            Broadcast(move.Write());
        }
        */
        public void Broadcast(ArraySegment<byte> segment)
        {
            _pendingList.Add(segment);
        }

        public void Enter(ClientSession session)
        {
            /*
            // Player 추가
            //Console.WriteLine($"Room Enter: {session.SessionId}");
            _sessions.Add(session);
            //session.Room = this;

            // 새 유저에 모든 플레이어 목록 전송
            S_PlayerList players = new();
            foreach (ClientSession s in _sessions)
            {
                players.players.Add(new S_PlayerList.Player()
                {
                    isSelf = (s == session),
                    playerId = s.SessionId,
                    posX = s.PosX,
                    posY = s.PosY,
                    posZ = s.PosZ
                });
            }
            session.Send(players.Write());

            // 새 유저 입장을 모두에게 알림
            S_BroadcaseEnterGame enter = new();
            enter.playerId = session.SessionId;
            enter.posX = 0;
            enter.posY = 0;
            enter.posZ = 0;
            Broadcast(enter.Write());
            */
        }

        public void Leave(ClientSession session)
        {
            // 플레이어 제거
            _sessions.Remove(session);

            // 모두에게 아림
            /*
            S_BroadcastLeaveGame leave = new();
            leave.playerId = session.SessionId;
            Broadcast(leave.Write());
            */
        }
    }

}
