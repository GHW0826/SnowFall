using System;
using Google.Protobuf.Protocol;
using TCPServerExample.DB;
using TCPServerExample.Session;

namespace TCPServerExample.Game
{
    public class Player : GameObject
    {
        public int PlayerDbId { get; set; }
        public ClientSession Session { get; set; }
        public Inventory _inven { get; private set; } = new();

        public Player()
        {
            ObjectType = GameObjectType.Player;
            Speed = 20;
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);
            Console.WriteLine($"DOTO OnDamaged : {damage}");
        }

        public override void OnDead(GameObject attacker)
        {
            base.OnDead(attacker);
        }

        public void OnLeaveGame()
        {
            // 1) 오래 접속된 유저, 서버 다운시 저장되지 않은 정보 날아감
            // 2) 코드 흐름을 다 막아버림. (로직이 있는 Game Room에서 호출되기 때문)
            // - 비동기로(Async) 수정?
            // - 다른 스레드로 (DB Job 생성)
            // -- 결과를 받아서 처리해야한느 경우 어떻게 처리할까
            // -- 아이템 생성 (생성 전에 사용하면 문제가 됨)


            // serving 담당
            // 결제 담당
            DbTransaction.SavePlayerStatus_Step1(this, Room);

            /*
            using (AppDbContext db = new())
            {

                PlayerDb playerDb = new PlayerDb();
                playerDb.PlayerDbId = PlayerDbId;
                playerDb.Hp = Stat.Hp;

                db.Entry(playerDb).State = Microsoft.EntityFrameworkCore.EntityState.Unchanged;
                db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
                bool success = db.SaveChangesEx();
                if (success == false)
                    return;

                Console.WriteLine($"Hp Saved:{playerDb.Hp}");
            }
            */
        }
    }
}
