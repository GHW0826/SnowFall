using DummyClient.Session;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using TCPServerCore;

class PacketHandler
{

    // Step1
    public static void S_ConnectedHandler(PacketSession session, IMessage packet)
    {
        C_Login loginPacket = new();
        
        ServerSession serverSession = (ServerSession)session;
        loginPacket.UniqueId = $"DummyClient_{serverSession.DummyId.ToString("0000")}";
        serverSession.Send(loginPacket);
    }
    // Step2
    // 로그인 OK + 캐릭터 목록
    public static void S_LoginHandler(PacketSession session, IMessage packet)
    {
        S_Login loginPacket = packet as S_Login;
        
        ServerSession serverSession = (ServerSession)session;
        if (loginPacket.Players != null || loginPacket.Players.Count == 0)
        {
            C_CreatePlayer createPacket = new();
            createPacket.Name = $"Player_{serverSession.DummyId.ToString("0000")}";
            serverSession.Send(createPacket);
        }
        else
        {
            LobbyPlayerInfo info = loginPacket.Players[0];
            C_EnterGame enterGamePacket = new();
            enterGamePacket.Name = info.Name;
            serverSession.Send(enterGamePacket);
        }
    }
    // step3
    public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
    {
        S_CreatePlayer createPlayerPacket = packet as S_CreatePlayer;
        ServerSession serverSession = (ServerSession)session;

        if (createPlayerPacket.Player != null)
        {
            // 생략
        }
        else
        {
            C_EnterGame enterGamePacket = new();
            enterGamePacket.Name = createPlayerPacket.Player.Name;
            serverSession.Send(enterGamePacket);
        }
    }
    // step 4
    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame entergamePacket = packet as S_EnterGame;
    }





    public static void S_DieHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame entergamePacket = packet as S_EnterGame;
    }
    public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame entergamePacket = packet as S_EnterGame;
    }
    public static void S_SkillHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame entergamePacket = packet as S_EnterGame;
    }
    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame entergamePacket = packet as S_EnterGame;
    }
    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame entergamePacket = packet as S_EnterGame;
    }
    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame entergamePacket = packet as S_EnterGame;
    }
    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame entergamePacket = packet as S_EnterGame;
    }

    public static void S_ItemlistHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame entergamePacket = packet as S_EnterGame;
    }
    public static void S_AdditemHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame entergamePacket = packet as S_EnterGame;
    }
    public static void S_EquipItemHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame entergamePacket = packet as S_EnterGame;
    }
    public static void S_ChangeStatHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame entergamePacket = packet as S_EnterGame;
    }
    public static void S_PingHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame entergamePacket = packet as S_EnterGame;
    }
}
