using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPDummyClient.Packet;

namespace TCPDummyClient
{
    public class SessionManager
    {
        static SessionManager _session = new();
        public static SessionManager Instance { get { return _session; } }

        List<ServerSession> _sessions = new();
        Random _random = new Random();
        object _lock = new();

        public void SendForEach()
        {
            lock (_lock)
            {
                foreach (var s in _sessions)
                {
                    C_Move movePacket = new();
                    movePacket.posX = _random.Next(-50, 50);
                    movePacket.posY = 0;
                    movePacket.posZ = _random.Next(-50, 50);
                    s.Send(movePacket.Write());
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

}
