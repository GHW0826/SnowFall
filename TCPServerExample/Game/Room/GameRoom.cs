using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using TCPServerExample.Data;
using TCPServerExample.Game.Object;
using TCPServerExample.Game.Room;

namespace TCPServerExample.Game
{
    public class GameRoom : JobSerializer
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
            // Monster monster = ObjectManager.Instance.Add<Monster>();
            // monster.CellPos = new Vector2Int(5, 5);
            // EnterGame(monster);
        }


        // 누군가 주기적으로 호출해줘야 한다.
        public void Update()
        {
            foreach (Projectile projectile in _projectile.Values)
            {
                projectile.Update();
            }

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
        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;

            PositionInfo movePosInfo = movePacket.PosInfo;
            ObjectInfo info = player.Info;

            // 다른 좌료포 이동할 경우, 갈 수 있는지 체크
            if (movePosInfo.PoxX != info.PosInfo.PoxX || movePosInfo.PoxY != info.PosInfo.PoxY)
            {
                if (Map.CanGo(new Vector2Int(movePosInfo.PoxX, movePosInfo.PoxY)) == false)
                    return;
            }
            //Console.WriteLine($"C_Mpve pos: {}, {}");
            info.PosInfo.State = movePosInfo.State;
            info.PosInfo.MoveDir = movePosInfo.MoveDir;
            Map.ApplyMove(player, new Vector2Int(movePosInfo.PoxX, movePosInfo.PoxY));

            // 다른 플레이어한테도 알려준다.
            S_Move resMovePacket = new();
            resMovePacket.ObjectId = player.Info.ObjectId;
            resMovePacket.PosInfo = movePacket.PosInfo;

            BroadCast(resMovePacket);
        }
        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null)
                return;

            ObjectInfo info = player.Info;
            if (info.PosInfo.State != CreatureState.Idle)
                return;

            // TODO : 스킬 사용 가능 여부 체크
            info.PosInfo.State = CreatureState.Skill;
            S_Skill skill = new() { Info = new SkillInfo() };
            skill.ObjectId = info.ObjectId;
            skill.Info.SkillId = skillPacket.Info.SkillId;
            BroadCast(skill);

            Data.Skill skillData = null;
            if (DataManager.SkillDict.TryGetValue(skillPacket.Info.SkillId, out skillData) == false)
                return;

            switch (skillData.skillType)
            {
                case SkillType.SkillAuto:
                    {
                        // 데미지 판정 (근접)
                        Vector2Int skillPos = player.GetFrontCellPos(info.PosInfo.MoveDir);
                        GameObject target = Map.Find(skillPos);
                        if (target != null)
                        {
                            Console.WriteLine("hit Game Object !");
                        }
                    }
                    break;
                case SkillType.SkillProjectile:
                    {
                        // TODO: Arrow
                        Arrow arrow = ObjectManager.Instance.Add<Arrow>();
                        if (arrow == null)
                            return;

                        //Console.WriteLine($"Gen Pos x: {player.PosInfo.PoxX}, y:{player.PosInfo.PoxY}");
                        arrow.Owner = player;
                        arrow.Data = skillData;
                        arrow.PosInfo.State = CreatureState.Moving;
                        arrow.PosInfo.MoveDir = player.PosInfo.MoveDir;
                        arrow.PosInfo.PoxX = player.PosInfo.PoxX;
                        arrow.PosInfo.PoxY = player.PosInfo.PoxY;
                        arrow.Speed = skillData.projectile.speed;
                        Push(EnterGame, arrow);
                    }
                    break;
            }

            // 통과
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
