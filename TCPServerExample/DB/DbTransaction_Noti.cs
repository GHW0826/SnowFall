
using TCPServerExample.Game;

namespace TCPServerExample.DB
{
    public partial class DbTransaction : JobSerializer
    {
        public static void EquipItemNoti(Player player, Item item)
        {
            if (player == null || item == null)
                return;


            ItemDb itemDb = new()
            {
                ItemDbId = item.ItemDbId,
                Equipped = item.Equipped
            };

            // You (Db)
            Instance.Push(() =>
            {
                using (AppDbContext db = new())
                {
                    db.Entry(itemDb).State = Microsoft.EntityFrameworkCore.EntityState.Unchanged;
                    db.Entry(itemDb).Property(nameof(itemDb.Equipped)).IsModified = true;

                    bool success = db.SaveChangesEx();
                    if (success)
                    {
                        // 실패 처리
                    }
                }
            });
        }
    }
}
