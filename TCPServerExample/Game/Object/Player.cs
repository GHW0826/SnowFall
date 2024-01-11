using System;
using System.Numerics;
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

        public int WeaponDamage { get; private set; }
        public int ArmorDefence { get; private set; }


        public override int TotalAttack { get { return Stat.Attack + WeaponDamage; } }
        public override int TotalDefence { get { return ArmorDefence; } }


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

        public void HandleEquipItem(C_EquipItem equipItem)
        {
            Item item = _inven.Get(equipItem.ItemDbId);
            if (item == null)
                return;
            if (item.ItemType == ItemType.Consumable)
                return;

            // 작용 요청이라면, 겹치는 부위는 해제
            if (equipItem.Equipped)
            {
                Item unequipItem = null;
                if (item.ItemType == ItemType.Weapon)
                {
                    unequipItem = _inven.Find(i => i.Equipped && i.ItemType == ItemType.Weapon);
                }
                else if (item.ItemType == ItemType.Armor)
                {
                    ArmorType armorType = ((Armor)item).ArmorType;
                    unequipItem = _inven.Find(
                        i => i.Equipped &&
                        i.ItemType == ItemType.Armor &&
                        ((Armor)i).ArmorType == armorType);
                }

                if (unequipItem != null)
                {
                    // 메모리 선 적용
                    unequipItem.Equipped = false;

                    // DB에 Noti
                    DbTransaction.EquipItemNoti(this, unequipItem);

                    // 클라에 통보
                    S_EquipItem equipItemPacket = new S_EquipItem();
                    equipItem.ItemDbId = unequipItem.ItemDbId;
                    equipItem.Equipped = unequipItem.Equipped;
                    Session.Send(equipItem);
                }
            }
            {
                // 메모리 선 적용
                item.Equipped = equipItem.Equipped;

                // DB에 Noti
                DbTransaction.EquipItemNoti(this, item);

                // 클라에 통보
                S_EquipItem equipItemPacket = new S_EquipItem();
                equipItem.ItemDbId = item.ItemDbId;
                equipItem.Equipped = equipItem.Equipped;
                Session.Send(equipItem);
            }

            RefreshAdditionalStat();
        }

        public void RefreshAdditionalStat()
        {
            WeaponDamage = 0;
            ArmorDefence = 0;

            foreach (Item item in _inven.Items.Values)
            {
                if (item.Equipped == false)
                    continue;

                switch (item.ItemType)
                {
                    case ItemType.Weapon:
                        WeaponDamage += ((Weapon)item).Damage;
                        break;

                    case ItemType.Armor:
                        ArmorDefence += ((Armor)item).Defence;
                        break;
                }
            }
        }
    }
}
