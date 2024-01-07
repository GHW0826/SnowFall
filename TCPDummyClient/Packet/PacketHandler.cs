using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPServerCore;

namespace TCPDummyClient.Packet;

public class PacketHandler
{

    public static void S_BroadcastEnterGameHandler(PacketSession session, IPacket packet)
    {
        S_BroadcaseEnterGame pkt = packet as S_BroadcaseEnterGame;
        ServerSession serverSession = session as ServerSession;
    }

    public static void S_BroadcastLeaveGameHandler(PacketSession session, IPacket packet)
    {
        S_BroadcastLeaveGame pkt = packet as S_BroadcastLeaveGame;
        ServerSession serverSession = session as ServerSession;
    }
    public static void S_PlayerListHandler(PacketSession session, IPacket packet)
    {
        S_PlayerList pkt = packet as S_PlayerList;
        ServerSession serverSession = session as ServerSession;
    }
    public static void S_BroadcastMoveHandler(PacketSession session, IPacket packet)
    {
        S_BroadcastMove pkt = packet as S_BroadcastMove;
        ServerSession serverSession = session as ServerSession;
    }



    public static void C_PlayerInfoReqHandler(PacketSession session, IPacket packet)
    {
    }

    public static void S_ChatHandler(PacketSession session, IPacket packet)
    {
        S_Chat chatPacket = packet as S_Chat;
        ServerSession serverSession = session as ServerSession;

        Console.WriteLine($"chat[{chatPacket.playerId}]: {chatPacket.chat}");
    }

    public static void S_TestHandler(PacketSession session, IPacket packet)
    {
    }
}
