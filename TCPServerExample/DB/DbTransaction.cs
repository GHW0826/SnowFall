
using Google.Protobuf.Protocol;
using System;
using TCPServerExample.Data;
using TCPServerExample.Game;
using TCPServerExample.Migrations;
using TCPServerExample.Utils;

namespace TCPServerExample.DB
{
    public partial class DbTransaction : JobSerializer
    {
        public static DbTransaction Instance { get; set; } = new();

        // Me (GameRoom) -> You (Db) -> Me (GameRoom)
        public static void SavePlayerStatus_AllInOne(Player player, GameRoom room)
        {
            if (player == null || room == null)
                return;

            // Me (GameRoom)
            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;
            playerDb.Hp = player.Stat.Hp;

            // You (Db)
            Instance.Push(() =>
            {
                using (AppDbContext db = new())
                {
                    db.Entry(playerDb).State = Microsoft.EntityFrameworkCore.EntityState.Unchanged;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                        // Me
                        room.Push(() =>
                        {
                            Console.WriteLine($"Hp Saved:{playerDb.Hp}");
                        });
                    }
                }
            });
        }

        // Me (GameRoom)
        public static void SavePlayerStatus_Step1(Player player, GameRoom room)
        {
            if (player == null || room == null)
                return;

            // Me (GameRoom)
            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;
            playerDb.Hp = player.Stat.Hp;
            Instance.Push<PlayerDb, GameRoom>(SavePlayerStatus_Step2, playerDb, room);
        }

        // You (Db)
        public static void SavePlayerStatus_Step2(PlayerDb playerDb, GameRoom room)
        {
            Instance.Push(() =>
            {
                using (AppDbContext db = new())
                {
                    db.Entry(playerDb).State = Microsoft.EntityFrameworkCore.EntityState.Unchanged;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
                    bool success = db.SaveChangesEx();
                    if (success)
                        room.Push(SavePlayerStatus_Step3, playerDb.Hp);
                }
            });
        }

        // Me(GameRoom)
        public static void SavePlayerStatus_Step3(int hp)
        {
            Console.WriteLine($"Hp Saved:{hp}");
        }

        public static void RewardPlayer(Player player, RewardData rewardData, GameRoom room)
        {
            if (player == null || rewardData == null || room == null)
                return;

            // TODO : 살짝 문제가 있긴 하다.
            // DB 선 적용, 메모리 후적용 일땐 타이밍 이슈 발생할 수 있음
            // 동시에 몬스터를 잡으면 같은 Slot을 볼 수 있음
            // 1) DB에다가 저장 요청
            // 2) DB 저장 OK 
            // 3) 메모리에 적용
            int? slot = player._inven.GetEmptySlot();
            if (slot == null)
                return;

            ItemDb itemDb = new()
            {
                TemplateId = rewardData.itemId,
                Count = rewardData.count,
                Slot = slot.Value,
                OwnerDbId = player.PlayerDbId
            };

            // You (Db)
            Instance.Push(() =>
            {
                using (AppDbContext db = new())
                {
                    db.Items.Add(itemDb);
                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                        room.Push(() =>
                        {
                            Item newItem = Item.MakeItem(itemDb);
                            player._inven.Add(newItem);

                            // TODO Client Noti
                            {
                                S_AddItem itemPacket = new S_AddItem();
                                ItemInfo itemInfo = new ItemInfo();
                                itemInfo.MergeFrom(newItem.info);
                                itemPacket.Items.Add(itemInfo);

                                player.Session.Send(itemPacket);
                            }
                        });
                    }
                }
            });
        }
    }
}
