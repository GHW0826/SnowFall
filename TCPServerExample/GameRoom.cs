using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPServerExample.Packet;
using TCPServerExample.Session;

namespace TCPServerExample;

public class GameRoom
{
    List<ClientSession> _sessions = new();
    object _lock = new();

    public void Broadcast(ClientSession session, string chat)
    {
        S_Chat packet = new();
        // packet.playerId = session.SessionId;
        packet.chat = chat + $" test ";
        ArraySegment<byte> data = packet.Write();

        lock (_lock)
        {
            foreach (var s in _sessions) 
            { 
                s.Send(data);
            }
        }
    }

    public void Enter(ClientSession session)
    {
        lock (_lock)
        {
            _sessions.Add(session);
            session.Room = this;
        }
    }

    public void Leave(ClientSession session)
    {
        lock (_lock)
        {
            _sessions.Remove(session);
        }
    }
}
