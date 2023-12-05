using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPServerCore;
using TCPServerExample.Packet;
using TCPServerExample.Session;

namespace TCPServerExample;

public class GameRoom : IJobQueue
{
    List<ClientSession> _sessions = new();
    JobQueue _jobQueue = new();

    public void Push(Action job)
    {
        _jobQueue.Push(job);
    }

    public void Broadcast(ClientSession session, string chat)
    {
        S_Chat packet = new();
        // packet.playerId = session.SessionId;
        packet.chat = chat + $" test ";
        ArraySegment<byte> data = packet.Write();

        foreach (var s in _sessions) 
        { 
            s.Send(data);
        }
    }

    public void Enter(ClientSession session)
    {
        _sessions.Add(session);
        session.Room = this;
    }

    public void Leave(ClientSession session)
    {
        _sessions.Remove(session);
    }

}
