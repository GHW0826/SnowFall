using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPServerCore;

namespace TCPDummyClient.Packet;

public class PacketHandler
{
    public static void C_PlayerInfoReqHandler(PacketSession session, IPacket packet)
    {
    }

    public static void S_ChatHandler(PacketSession session, IPacket packet)
    {
        S_Chat chatPacket = packet as S_Chat;
        ServerSession clientSession = session as ServerSession;

        Console.WriteLine($"chat[{chatPacket.playerId}]: {chatPacket.chat}");
    }

    public static void S_TestHandler(PacketSession session, IPacket packet)
    {
    }
}
