using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Linq;
using System.Reflection.Metadata;
using TCPServerCore;
using TCPServerExample.DB;
using TCPServerExample.Game;
using TCPServerExample.Session;

class PacketHandler
{
	public static void C_MoveHandler(PacketSession session, IMessage packet)
	{
		C_Move movePacket = packet as C_Move;
		ClientSession clientSession = session as ClientSession;

       // Console.WriteLine("C_Mpve1");
        Player player = clientSession.MyPlayer;
		if (player == null)
			return;
       // Console.WriteLine("C_Mpve2");
        GameRoom room = player.Room;
		if (room == null)
			return;
       // Console.WriteLine("C_Mpve3");
        room.Push(room.HandleMove, player, movePacket);
    }

    public static void C_SkillHandler(PacketSession session, IMessage packet)
	{

        C_Skill skillPacket = packet as C_Skill;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.Push(room.HandleSkill, player, skillPacket);
    }

    public static void C_LoginHandler(PacketSession session, IMessage packet)
    {
        C_Login loginPacket = packet as C_Login;
        ClientSession clientSession = session as ClientSession;
        clientSession.HandleLogin(loginPacket);
    }

    public static void C_CreatePlayerHandler(PacketSession session, IMessage packet)
    {
        C_CreatePlayer createPlayerPacket = packet as C_CreatePlayer;
        ClientSession clientSession = session as ClientSession;
        clientSession.HandleCreatePlayer(createPlayerPacket);
    }

    public static void C_EnterGameHandler(PacketSession session, IMessage packet)
    {
        C_EnterGame enterPacket = packet as C_EnterGame;
        ClientSession clientSession = session as ClientSession;
        clientSession.HandleEnterGame(enterPacket);
    }

}
