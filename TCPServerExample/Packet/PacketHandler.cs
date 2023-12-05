﻿using System;
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

    public static void C_Chathandler(PacketSession session, IPacket packet)
    {
        C_Chat chatPacket = packet as C_Chat;
        ClientSession clientSession = session as ClientSession;
        if (clientSession.Room == null)
            return;

        clientSession.Room.Broadcast(clientSession, chatPacket.name);
        Console.WriteLine($"");
    }
}