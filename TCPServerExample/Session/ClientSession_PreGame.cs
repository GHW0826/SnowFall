using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using TCPServerCore;
using TCPServerExample.Data;
using TCPServerExample.DB;
using TCPServerExample.Game;
using TCPServerExample.Migrations;
using TCPServerExample.Utils;

namespace TCPServerExample.Session
{
    public partial class ClientSession : PacketSession
    {
        public int AccountDbId { get; private set; }
        public List<LobbyPlayerInfo> LobbyPlayers { get; set; } = new();

        public void HandleLogin(C_Login loginPacket)
        {
            Console.WriteLine($"uniqueid({loginPacket.UniqueId})");

            // TODO 보안체크
            if (ServerState != PlayerServerState.ServerStateLogin)
                return;

            LobbyPlayers.Clear();

            // - 동시에 다른 사람이 같은 unique id 보내면?
            // - 악의적으로 여러번 보낸다.
            // - 프로세스 로직상 맞지 않은 순서의 호출
            using (AppDbContext db = new())
            {
                AccountDb? findAccount = db.Accounts
                                        .Include(a => a.Players)
                                        .Where(a => a.AccountName == loginPacket.UniqueId).FirstOrDefault();
                if (findAccount != null)
                {
                    // AccountDbID 메모리에 기억
                    AccountDbId = findAccount.AccountDbId;

                    S_Login loginOk = new S_Login()
                    {
                        LoginOk = 1
                    };
                    foreach (PlayerDb playerDb in findAccount.Players)
                    {
                        LobbyPlayerInfo lobbyPlayer = new()
                        {
                            PlayerDbId = playerDb.AccountDbId,
                            Name = playerDb.PlayerName,
                            StatInfo = new ()
                            {
                                Level = playerDb.Level,
                                Hp = playerDb.Hp,
                                MaxHp = playerDb.MaxHp,
                                Attack = playerDb.Attack,
                                Speed = playerDb.Speed,
                                TotalExp = playerDb.TotalExp
                            }
                        };
                        // 메모리에 들고 있는다.
                        LobbyPlayers.Add(lobbyPlayer);

                        // 패킷에 넣는다.
                        loginOk.Players.Add(lobbyPlayer);
                    } 

                    Send(loginOk);

                    // 로비로 이동
                    ServerState = PlayerServerState.ServerStateLobby;
                }
                else
                {
                    AccountDb newAccount = new AccountDb()
                    {
                        AccountName = loginPacket.UniqueId
                    };
                    db.Accounts.Add(newAccount);
                    bool success = db.SaveChangesEx(); // TODO exception
                    if (success == false)
                        return;

                    AccountDbId = newAccount.AccountDbId;

                    S_Login loginOk = new S_Login()
                    {
                        LoginOk = 1
                    };
                    Send(loginOk);
                    ServerState = PlayerServerState.ServerStateLobby;
                }
            }
        }

        public void HandleCreatePlayer(C_CreatePlayer createPacket)
        {
            // TODO 보안체크
            if (ServerState != PlayerServerState.ServerStateLobby)
                return;

            using (AppDbContext db = new AppDbContext())
            {
                PlayerDb? findPlayer = db.Players
                    .Where(p => p.PlayerName == createPacket.Name).FirstOrDefault();

                if (findPlayer != null)
                {
                    // 이름이 겹친다.
                    Send(new S_CreatePlayer());
                }
                else
                {
                    // 1레벨 스탯 정보 추출
                    StatInfo stat = null;
                    DataManager.StatDict.TryGetValue(1, out stat);

                    // DB에 플레이어 만들어 줘야 함.
                    PlayerDb newPlayerDb = new PlayerDb()
                    {
                        PlayerName = createPacket.Name,
                        Level = stat.Level,
                        Hp = stat.Hp,
                        MaxHp = stat.MaxHp,
                        Attack = stat.Attack,
                        Speed = stat.Speed,
                        TotalExp = 0,
                        AccountDbId = AccountDbId
                    };
                    db.Players.Add(newPlayerDb);
                    bool success = db.SaveChangesEx(); // TODO Exception handling
                    if (success == false)
                        return;

                    // 메모리에 추가
                    LobbyPlayerInfo lobbyPlayer = new()
                    {
                        PlayerDbId = newPlayerDb.PlayerDbId,
                        Name = createPacket.Name,
                        StatInfo = new()
                        {
                            Level = stat.Level,
                            Hp = stat.Hp,
                            MaxHp = stat.MaxHp,
                            Attack = stat.Attack,
                            Speed = stat.Speed,
                            TotalExp = 0
                        }
                    };
                    // 메모리에 들고 있는다.
                    LobbyPlayers.Add(lobbyPlayer);

                    // 클라에 전송
                    S_CreatePlayer newPlayer = new()
                    {
                        Player = new LobbyPlayerInfo() 
                    };
                    newPlayer.Player.MergeFrom(lobbyPlayer);
                    Send(newPlayer);
                }
            }
        }

        public void HandleEnterGame(C_EnterGame enterPacket)
        {
            if (ServerState != PlayerServerState.ServerStateLobby) 
                return;

            LobbyPlayerInfo? playerInfo = LobbyPlayers.Find(p => p.Name == enterPacket.Name);
            if (playerInfo == null) 
                return;

            // 로비에서 캐릭터 선택시 생성
            MyPlayer = ObjectManager.Instance.Add<Player>();
            {
                MyPlayer.PlayerDbId = playerInfo.PlayerDbId;
                MyPlayer.Info.Name = playerInfo.Name;
                MyPlayer.Info.PosInfo.State = CreatureState.Idle;
                MyPlayer.Info.PosInfo.MoveDir = MoveDir.Down;
                MyPlayer.Info.PosInfo.PoxX = 0;
                MyPlayer.Info.PosInfo.PoxY = 0;
                MyPlayer.Stat.MergeFrom(playerInfo.StatInfo);
                MyPlayer.Session = this;

                S_ItemList itemListPacket = new();
                // 아이템 목록 가져오기
                using (AppDbContext db = new AppDbContext())
                {
                    List<ItemDb> list = db.Items.Where(i => i.OwnerDbId == playerInfo.PlayerDbId).ToList();

                    foreach (ItemDb itemDb in list)
                    {
                        // TODO 인벤토리
                        Item item = Item.MakeItem(itemDb);
                        if (item != null)
                        {
                            MyPlayer._inven.Add(item);

                            ItemInfo info = new();
                            info.MergeFrom(item.info);
                            itemListPacket.Items.Add(info);
                        }
                    }

                    // TODO 클라에게 아이템 목록 전달
                }

                Send(itemListPacket);
            }

            // 입장 요청 들어오면 실행
            GameRoom room = RoomManager.Instance.Find(1);
            room.Push(room.EnterGame, MyPlayer);

            ServerState = PlayerServerState.ServerStateGame;
        }
    }
}

