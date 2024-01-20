using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServerExampleZone.Game.Room
{
    public class VisionCube
    {
        public Player Owner { get; private set; }
        public HashSet<GameObject> PreviousObjects { get; private set; } = new();

        public VisionCube(Player owner)
        {
            Owner = owner;
        }

        public HashSet<GameObject> GaterObjects()
        {
            if (Owner == null || Owner.Room == null)
                return null;

            HashSet<GameObject> objects = new HashSet<GameObject>();

            Vector2Int cellPos = Owner.CellPos;
            List<Zone> zones = Owner.Room.GetAdjacentZones(cellPos);

            foreach (Zone zone in zones)
            {
                foreach (Player player in zone.Players)
                {
                    // 시야 각 범위 체크
                    int dx = player.CellPos.x - cellPos.x;
                    int dy = player.CellPos.y - cellPos.y;
                    if (Math.Abs(dx) > GameRoom.VisionCells)
                        continue;
                    if (Math.Abs(dy) > GameRoom.VisionCells)
                        continue;

                    objects.Add(player);
                }
                foreach (Monster monster in zone.Monsters)
                {
                    // 시야 각 범위 체크
                    int dx = monster.CellPos.x - cellPos.x;
                    int dy = monster.CellPos.y - cellPos.y;
                    if (Math.Abs(dx) > GameRoom.VisionCells)
                        continue;
                    if (Math.Abs(dy) > GameRoom.VisionCells)
                        continue;

                    objects.Add(monster);
                }
                foreach (Projectile projectile in zone.Projectiles)
                {
                    // 시야 각 범위 체크
                    int dx = projectile.CellPos.x - cellPos.x;
                    int dy = projectile.CellPos.y - cellPos.y;
                    if (Math.Abs(dx) > GameRoom.VisionCells)
                        continue;
                    if (Math.Abs(dy) > GameRoom.VisionCells)
                        continue;

                    objects.Add(projectile);
                }
            }


            return objects;
        }

        public void Update()
        {
            if (Owner == null || Owner.Room == null)
                return;

            HashSet<GameObject> currentObjects = new();

            // 기존에 없었는데 새로 생긴 애들 Spawn 처리
            List<GameObject> added = currentObjects.Except(PreviousObjects).ToList();
            if (added.Count > 0)
            {
                S_Spawn spawnPacket = new();
                foreach (GameObject go in added)
                {
                    ObjectInfo info = new();
                    info.MergeFrom(go.Info);
                    spawnPacket.Objects.Add(info);
                }

                Owner.Session.Send(spawnPacket);
            }

            // 기존에 있었는데 사라진 애들 Despawn 처리
            List<GameObject> removed = PreviousObjects.Except(currentObjects).ToList();
            if (removed.Count > 0)
            {
                S_Despawn despawnPacket = new();
                foreach (GameObject go in removed)
                {
                    despawnPacket.ObjectIds.Add(go.Id);
                }
                Owner.Session.Send(despawnPacket);
            }

            PreviousObjects = currentObjects;

            Owner.Room.PushAfter(500, Update);
        }
    }
}
