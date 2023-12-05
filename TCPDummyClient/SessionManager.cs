using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPDummyClient.Packet;

namespace TCPDummyClient;

public class SessionManager
{
    static SessionManager _session = new();
    public static SessionManager Instance { get { return _session; } }

    List<ServerSession> _sessions = new();
    object _lock = new();

    public void SendForEach()
    {
        lock (_lock) 
        { 
            foreach (var s in _sessions)
            {
                C_Chat chatPacket = new();
                chatPacket.name = $"Hello Server !";
                ArraySegment<byte> segment = chatPacket.Write();
                s.Send(segment);
            }
        }
    }

    public ServerSession Generate()
    {
        lock (_lock) 
        {
            ServerSession session = new();
            _sessions.Add(session);
            return session;
        }
    }
}
