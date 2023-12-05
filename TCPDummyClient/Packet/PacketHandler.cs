using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPServerCore;

namespace TCPDummyClient.Packet;

public class PacketHandler
{
    public static void PlayerInfoReqHandler(PacketSession session, IPacket packet)
    {
        PlayerInfoReq p = packet as PlayerInfoReq;
        Console.WriteLine($"PlayerInfoReq: {p.playerId}: {p.name}");
        foreach (var skill in p.skills)
        {
            Console.WriteLine($"skill info: {skill.id}: {skill.level} : {skill.duration}");
            foreach (var att in skill.attributes)
            {
                Console.WriteLine($"attr : {att.attr}");
            }
        }
    }

    public static void S_Chathandler(PacketSession session, IPacket packet)
    {
        S_Chat chatPacket = packet as S_Chat;
        ServerSession clientSession = session as ServerSession;

        Console.WriteLine(chatPacket.chat);
    }
}
