
using System;
using TCPServerExample.Game;
using TCPServerExample.Migrations;
using TCPServerExample.Utils;

namespace TCPServerExample.DB
{
    public class DbTransaction : JobSerializer
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
    }
}
