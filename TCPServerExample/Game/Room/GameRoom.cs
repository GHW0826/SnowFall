using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using TCPServerExample.Data;
using TCPServerExample.Game.Object;
using TCPServerExample.Game.Room;

namespace TCPServerExample.Game
{
    public partial class GameRoom : JobSerializer
    {
        public int RoomId { get; set; }

        Dictionary<int, Player> _players = new ();
        Dictionary<int, Monster> _monsters = new ();
        Dictionary<int, Projectile> _projectile = new ();

        public Map Map { get; private set; } = new();

        public void Init(int mapId)
        {
            Map.LoadMap(mapId, "../../../../Common/MapData/");

            // temp
            Monster monster = ObjectManager.Instance.Add<Monster>();
            monster.Init(monster.TemplateId);
            monster.CellPos = new Vector2Int(5, 5);
            EnterGame(monster);
        }

        // 누군가 주기적으로 호출해줘야 한다.
        public void Update()
        {
            foreach (Monster monster in _monsters.Values)
            {
                monster.Update();
            }

            Flush();
        }

        public void EnterGame(GameObject newObject)
        {
            if (newObject == null)
                return;

            GameObjectType type = ObjectManager.GetObjectTypeById(newObject.id);

            if (type == GameObjectType.Player)
            {
                Player player = newObject as Player;
                _players.Add(newObject.id, player);
                player.Room = this;

                player.RefreshAdditionalStat();

                Map.ApplyMove(player, new Vector2Int(player.CellPos.x, player.CellPos.y));

                // 본인한테 정보 전송
                {
                    S_EnterGame enterPacket = new();
                    enterPacket.Player = player.Info;
                    player.Session.Send(enterPacket);

                    S_Spawn spawnPacket = new();
                    foreach (Player p in _players.Values)
                    {
                        if (player != p)
                            spawnPacket.Objects.Add(p.Info);
                    }
                    foreach (Monster m in _monsters.Values)
                        spawnPacket.Objects.Add(m.Info);
                    foreach (Projectile p in _projectile.Values)
                        spawnPacket.Objects.Add(p.Info);

                    player.Session.Send(spawnPacket);
                }
            }
            else if (type == GameObjectType.Monster)
            {
                Monster monster = newObject as Monster;
                _monsters.Add(newObject.id, monster);
                monster.Room = this;

                Map.ApplyMove(monster, new Vector2Int(monster.CellPos.x, monster.CellPos.y));
            }
            else if (type == GameObjectType.Projectile)
            {
                Projectile projectile = newObject as Projectile;
                _projectile.Add(newObject.id, projectile);
                projectile.Room = this;

                projectile.Update();
            }

            // 타인한테 정보 전송
            {
                S_Spawn spawnPacket = new();
                spawnPacket.Objects.Add(newObject.Info);
                foreach (Player p in _players.Values)
                {
                    if (newObject.id != p.id)
                        p.Session.Send(spawnPacket);
                }
            }
        }

        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            if (type == GameObjectType.Player)
            {
                Player player = null;
                if (_players.Remove(objectId, out player) == false)
                    return;

                player.OnLeaveGame();
                Map.ApplyLeave(player);
                player.Room = null;

                // 본인한테 정보 전송
                {
                    S_LeaveGame leavePacket = new();
                    player.Session.Send(leavePacket);
                }
            }
            else if (type == GameObjectType.Monster)
            {
                Monster monster = null;
                if (_monsters.Remove(objectId, out monster) == false)
                    return;

                Map.ApplyLeave(monster);
                monster.Room = null;
            }
            else if (type == GameObjectType.Projectile)
            {
                Projectile projectile = null;
                if (_projectile.Remove(objectId, out projectile) == false)
                    return;
                projectile.Room = null;
            }

            // 타인한테 정보 전송
            {
                S_Despawn despawnPacket = new();
                despawnPacket.ObjectIds.Add(objectId);
                foreach (Player p in _players.Values)
                {
                    if (objectId != p.id)
                        p.Session.Send(despawnPacket);
                }
            }
        }
        // TODO
        public Player FindPlyer(Func<GameObject, bool> condition)
        {
            foreach (Player player in _players.Values)
            {
                if (condition.Invoke(player))
                    return player;
            }
            return null;
        }

        public void BroadCast(IMessage packet)
        {
            foreach (Player p in _players.Values)
            {
                p.Session.Send(packet);
            }
        }


    }
}
