using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPServerExample.Game.Room;

namespace TCPServerExample.Game
{
    public class GameObject
    {

        public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;

        // [Unused(1)][Type(7)][ID(24)]
        // [........] [........] [........] [........]
        public int id
        {
            get { return Info.ObjectId; }
            set { Info.ObjectId = value; }
        }
        public GameRoom Room { get; set; }
        public ObjectInfo Info { get; set; } = new() { PosInfo = new() };
        public PositionInfo PosInfo { get; private set; } = new();
        public StatInfo Stat { get; private set; } = new();

        public virtual int TotalAttack { get { return Stat.Attack; } }
        public virtual int TotalDefence { get { return 0; } }

        public float Speed
        { 
            get { return Stat.Speed; }
            set { Stat.Speed = value; }
        }
        public int Hp
        {
            get { return Stat.Hp; }
            set { Stat.Hp = Math.Clamp(value, 0, Stat.MaxHp); ; }
        }
        public MoveDir Dir
        {
            get { return PosInfo.MoveDir; }
            set { PosInfo.MoveDir = value; }
        }

        public CreatureState State
        {
            get { return PosInfo.State; } 
            set {  PosInfo.State = value; }
        }
        public GameObject()
        {
            Info.PosInfo = PosInfo;
            Info.StatInfo = Stat;
        }

        public virtual void Update()
        {

        }

        public Vector2Int CellPos
        {
            get { return new Vector2Int(PosInfo.PoxX, PosInfo.PoxY); }
            set
            {
                PosInfo.PoxX = value.x;
                PosInfo.PoxY = value.y;
            }
        }

        public Vector2Int GetFrontCellPos() { return GetFrontCellPos(PosInfo.MoveDir); }

        public Vector2Int GetFrontCellPos(MoveDir dir)
        {
            Vector2Int cellPos = CellPos;
            switch (dir)
            {
                case MoveDir.Up:
                    cellPos += Vector2Int.up;
                    break;
                case MoveDir.Down:
                    cellPos += Vector2Int.down;
                    break;
                case MoveDir.Left:
                    cellPos += Vector2Int.left;
                    break;
                case MoveDir.Right:
                    cellPos += Vector2Int.right;
                    break;
            }
            return cellPos;
        }

        public static MoveDir GetDirFromVec(Vector2Int dir)
        {
            if (dir.x > 0)
                return MoveDir.Right;
            else if (dir.x < 0)
                return MoveDir.Left;
            else if (dir.y > 0)
                return MoveDir.Up;
            else
                return MoveDir.Down;
        }

        public virtual void OnDamaged(GameObject attacker, int damage)
        {
            if (Room == null)
                return;

            damage = Math.Max((damage - TotalDefence), 0);
            Stat.Hp = Math.Max(Stat.Hp - damage, 0);

            // change HP broadCast
            S_ChangeHp changeHpPacket = new S_ChangeHp();
            changeHpPacket.ObjectId = id;
            changeHpPacket.Hp = Stat.Hp;
            Room.BroadCast(changeHpPacket);

            if (Stat.Hp <= 0)
                OnDead(attacker);
        }

        public virtual void OnDead(GameObject attacker)
        {
            if (Room == null)
                return;

            S_Die diePacket = new();
            diePacket.ObjectId = id;
            diePacket.AttackerId = attacker.id;
            Room.BroadCast(diePacket);

            GameRoom room = Room;
            room.LeaveGame(id);

            Stat.Hp = Stat.MaxHp;
            PosInfo.State = CreatureState.Idle;
            PosInfo.MoveDir = MoveDir.Down;
            PosInfo.PoxX = 0;
            PosInfo.PoxY = 0;

            room.EnterGame(this);
        }

        public virtual GameObject GetOwner()
        {
            return this;
        }
    }
}
