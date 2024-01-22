using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient.Session
{
    public class SessionManager
    {
        public static SessionManager Instance { get; } = new();
        HashSet<ServerSession> _sessions = new();
        object _lock = new();
        int _dummyId = 1;

        public ServerSession Generate()
        {
            lock (_lock)
            {
                ServerSession session = new();
                session.DummyId = _dummyId;
                _dummyId++;

                _sessions.Add(session);
                Console.WriteLine($"Connected {_sessions.Count} Players");
                return session;
            }
        }

        public void Remove(ServerSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session);
                Console.WriteLine($"Connected {_sessions.Count} Players");
            }
        }
    }
}
