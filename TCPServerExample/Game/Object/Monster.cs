using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using TCPServerExample.Data;
using TCPServerExample.DB;
using TCPServerExample.Game.Room;

namespace TCPServerExample.Game
{
    public class Monster : GameObject
    {
        public int TemplateId { get; private set; }
        public Monster()
        {
            ObjectType = GameObjectType.Monster;

        }

        public void Init(int templateId)
        {
            TemplateId = templateId;

            MonsterData monsterData = null;
            DataManager.MonsterDict.TryGetValue(templateId, out monsterData);
            Stat.MergeFrom(monsterData.stat);
            Stat.Hp = monsterData.stat.MaxHp;
            State = CreatureState.Idle;
        }

        // FSM (Finit State Machine)
        public override void Update()
        {
            switch (State)
            {
                case CreatureState.Idle:
                    UpdateIdle();
                    break;
                case CreatureState.Moving:
                    UpdateMoving();
                    break;
                case CreatureState.Skill:
                    UpdateSkill();
                    break;
                case CreatureState.Dead:
                    UpdateDead();
                    break;
            }
        }

        Player _target;
        int _searchCellDist = 10;
        int _chaseCellDist = 20;
        long _nextSearchTick = 0;
        protected virtual void UpdateIdle()
        {
            if (_nextSearchTick > Environment.TickCount64)
                return;
            _nextSearchTick = Environment.TickCount64 + 1000;

            Player target = Room.FindPlyer(p =>
            {
                Vector2Int dir = p.CellPos - CellPos;
                return dir.cellDistFromZero <= _searchCellDist;
            });
            if (target == null)
                return;

            _target = target;
            State = CreatureState.Moving;
        }

        int _skillRange = 1;
        long _nextMoveTick = 0;
        protected virtual void UpdateMoving()
        {
            if (_nextMoveTick > Environment.TickCount64)
                return;

            int moveTick = (int)(1000 / Speed);
            _nextSearchTick = Environment.TickCount64 + moveTick;
            if (_target == null || _target.Room != Room)
            {
                _target = null;
                State = CreatureState.Idle;
                BroadCastMove();
                return;
            }

            Vector2Int dir = _target.CellPos - CellPos;
            int dist = dir.cellDistFromZero;
            if (dist == 0 || dist > _chaseCellDist)
            {
                _target = null;
                State = CreatureState.Idle;
                BroadCastMove();
                return;
            }

            List<Vector2Int> path = Room.Map.FindPath(CellPos, _target.CellPos, checkObjects: false);
            if (path.Count < 2 || path.Count > _chaseCellDist)
            {
                _target = null;
                State = CreatureState.Idle;
                BroadCastMove();
                return;
            }

            // 스킬로 넘어갈지 체크
            if (dist <= _skillRange && (dir.x == 0 || dir.y == 0))
            {
                _coolTick = 0;
                State = CreatureState.Skill;
                return;
            }

            // 이동
            Dir = GetDirFromVec(path[1] - CellPos);
            Room.Map.ApplyMove(this, path[1]);
            BroadCastMove();
        }

        void BroadCastMove()
        {
            // 다른 플레이어한테도 알려줌
            S_Move movePacket = new S_Move();
            movePacket.ObjectId = id;
            movePacket.PosInfo = PosInfo;
            Room.BroadCast(movePacket);
        }

        long _coolTick = 0;
        protected virtual void UpdateSkill()
        {
            if (_coolTick == 0)
            {
                // 유효한 타겟인지
                if (_target == null || _target.Room != Room || _target.Hp == 0)
                {
                    _target = null;
                    State = CreatureState.Moving;
                    BroadCastMove();
                    return;
                }
                // 스킬이 사용 가능한지
                Vector2Int dir = (_target.CellPos - CellPos);
                int dist = dir.cellDistFromZero;
                bool canUseSkill = (dist <= _skillRange && (dir.x == 0 || dir.y == 0));
                if (canUseSkill == false)
                {
                    _target = null;
                    State = CreatureState.Moving;
                    BroadCastMove();
                    return;
                }

                // 타게킹 방향 주시
                MoveDir lookDir = GetDirFromVec(dir);
                if (Dir != lookDir)
                {
                    Dir = lookDir;
                    BroadCastMove();
                }

                Skill skillData = null;
                DataManager.SkillDict.TryGetValue(1, out skillData);

                // 데미지 판정
                _target.OnDamaged(this, skillData.damage + TotalAttack);

                // 스킬 사용 BroadCast
                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.ObjectId = id;
                skill.Info.SkillId = skillData.id;
                Room.BroadCast(skill);

                // 스킬 쿨타임 적용
                int coolTick = (int)(1000 * skillData.cooldown);
                _coolTick = Environment.TickCount64 + coolTick;

                if (_coolTick > Environment.TickCount64)
                    return;

                _coolTick = 0;
            }

        }
        protected virtual void UpdateDead()
        {

        }

        public override void OnDead(GameObject attacker)
        {
            base.OnDead(attacker);

            if (Room == null)
                return;

            GameObject owner = attacker.GetOwner();
            if (attacker.ObjectType == GameObjectType.Player)
            {
                RewardData rewardData = GetRandomReward();
                if (rewardData != null)
                {
                    Player player = (Player)owner;
                    DbTransaction.RewardPlayer(player, rewardData, Room);
                    //player._inven.Add();
                }
            }
        }

        RewardData GetRandomReward()
        {
            MonsterData monsterData = null;
            DataManager.MonsterDict.TryGetValue(TemplateId, out monsterData);


            // rand = 0 ~ 100
            int sum = 0;
            int rand = new Random().Next(0, 101);
            foreach (RewardData rewardData in monsterData.rewards)
            {
                sum += rewardData.probability;
                if (rand <= sum)
                    return rewardData;
            }
            return null;
        }
    }
}
