using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPServerCore;
using TCPServerExample.Packet;

namespace TCPServerExample.Packet;

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
}
