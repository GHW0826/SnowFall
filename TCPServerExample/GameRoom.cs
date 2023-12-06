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
    List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

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

        Console.WriteLine($"Flushed {_pendingList.Count} items");
        _pendingList.Clear();
    }

    public void Broadcast(ClientSession session, string chat)
    {
        S_Chat packet = new();
        packet.playerId = session.SessionId;
        packet.chat = chat + $" chat : {packet.playerId}";
        ArraySegment<byte> segment = packet.Write();

        _pendingList.Add(segment);
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
