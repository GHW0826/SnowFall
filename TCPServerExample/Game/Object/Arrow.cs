using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TCPServerExample.Game.Object;
using TCPServerExample.Game.Room;

namespace TCPServerExample.Game
{
    public class Arrow : Projectile
    {
        public GameObject Owner { get; set; }

        long _nextMoveTick = 0;
        public override void Update()
        {
            if (Data == null || Data.projectile == null || Owner == null || Room == null)
                return;

            if (_nextMoveTick >= Environment.TickCount64)
                return;

            long tick = (long)(1000 / Data.projectile.speed);
            _nextMoveTick = Environment.TickCount64 + tick;

            Vector2Int destPos = GetFrontCellPos();
            Console.WriteLine($"Arow pos: {destPos.x},{destPos.y}");
            if (Room.Map.CanGo(destPos))
            {
                CellPos = destPos;

                S_Move movePacket = new S_Move();
                movePacket.ObjectId = id;
                movePacket.PosInfo = PosInfo;
                Room.BroadCast(movePacket);

                Console.WriteLine("Move Arrow");
            }
            else
            {
                GameObject target = Room.Map.Find(destPos);
                if (target != null)
                {
                    // TODO 피격 판정
                    target.OnDamaged(this, Data.damage + Owner.Stat.Attack);
                }

                // 소멸
                GameRoom room = Room;
                room.Push(room.LeaveGame, id);
                Console.WriteLine($"destroy: {id}");
            }
        }
    }
}
