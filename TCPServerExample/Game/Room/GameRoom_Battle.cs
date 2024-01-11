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

    }
}
