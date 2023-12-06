using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPServerCore;
using TCPServerExample.Packet;
using TCPServerExample.Session;

namespace TCPServerExample.Packet;

public class PacketHandler
{
    public static void C_PlayerInfoReqHandler(PacketSession session, IPacket packet)
    {
    }

    public static void C_ChatHandler(PacketSession session, IPacket packet)
    {
        C_Chat chatPacket = packet as C_Chat;
        ClientSession clientSession = session as ClientSession;
        if (clientSession.Room == null)
            return;

        GameRoom room = clientSession.Room as GameRoom;
        room.Push(
            () => room.Broadcast(clientSession, chatPacket.chat)
        );
    }

    public static void S_TestHandler(PacketSession session, IPacket packet)
    {
    }
}
