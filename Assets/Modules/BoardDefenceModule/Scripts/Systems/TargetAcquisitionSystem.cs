using System.Collections.Generic;
using Strada.Core.ECS;
using Strada.Core.ECS.Systems;
using Strada.Core.ECS.World;
using Strada.Core.Modules;
using BoardDefence.Signals;
using BoardDefence.Components;
using BoardDefence.Data;
using UnityEngine;

namespace BoardDefence.Systems
{
    [StradaSystem(
        Module = "BoardDefence",
        Category = "Combat",
        Description = "Acquires targets and fires projectiles from defence items",
        Phase = UpdatePhase.Update,
        Order = 300)]
    public class TargetAcquisitionSystem : SystemBase
    {

        private readonly List<(Entity entity, GridPositionComponent pos, HealthComponent health)> _enemies = new(32);
        private readonly List<(Entity entity, GridPositionComponent pos, AttackStatsComponent stats)> _readyDefences = new(16);

        protected override void OnUpdate(float deltaTime)
        {
            _enemies.Clear();
            _readyDefences.Clear();

            ForEach<EnemyTag, GridPositionComponent, HealthComponent>(
                (int entityIndex, ref EnemyTag tag, ref GridPositionComponent pos, ref HealthComponent health) =>
                {
                    var entity = EntityManager.GetEntity(entityIndex);
                    if (health.Current > 0 && !EntityManager.HasComponent<DestroyTag>(entity))
                    {
                        _enemies.Add((entity, pos, health));
                    }
                });

            ForEach<DefenceItemTag, GridPositionComponent, AttackStatsComponent>(
                (int entityIndex, ref DefenceItemTag tag, ref GridPositionComponent pos, ref AttackStatsComponent stats) =>
                {
                    var entity = EntityManager.GetEntity(entityIndex);
                    if (EntityManager.HasComponent<ReadyToFireTag>(entity))
                    {
                        _readyDefences.Add((entity, pos, stats));
                    }
                });

            foreach (var defence in _readyDefences)
            {
                var target = FindTarget(defence.pos, defence.stats);
                if (!target.entity.IsNull)
                {
                    FireProjectile(defence.entity, defence.pos, defence.stats, target.entity, target.pos);
                }
            }
        }

        private (Entity entity, GridPositionComponent pos) FindTarget(GridPositionComponent defencePos, AttackStatsComponent stats)
        {
            Entity closestEntity = Entity.Null;
            GridPositionComponent closestPos = default;
            float closestDist = float.MaxValue;

            foreach (var enemy in _enemies)
            {
                if (stats.Direction == AttackDirection.Forward)
                {
                    if (enemy.pos.Column != defencePos.Column) continue;
                    float dist = enemy.pos.WorldZ - defencePos.WorldZ;
                    if (dist > 0 && dist <= stats.Range)
                    {
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestEntity = enemy.entity;
                            closestPos = enemy.pos;
                        }
                    }
                }
                else
                {
                    float dx = enemy.pos.WorldX - defencePos.WorldX;
                    float dz = enemy.pos.WorldZ - defencePos.WorldZ;
                    float dist = Mathf.Sqrt(dx * dx + dz * dz);
                    if (dist <= stats.Range)
                    {
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestEntity = enemy.entity;
                            closestPos = enemy.pos;
                        }
                    }
                }
            }

            return (closestEntity, closestPos);
        }

        private void FireProjectile(Entity defenceEntity, GridPositionComponent defencePos,
                                    AttackStatsComponent stats, Entity targetEntity, GridPositionComponent targetPos)
        {
            Send(new SpawnProjectileSignal
            {
                StartX = defencePos.WorldX,
                StartY = 0.5f,
                StartZ = defencePos.WorldZ,
                Target = targetEntity,
                TargetX = targetPos.WorldX,
                TargetY = 0.5f,
                TargetZ = targetPos.WorldZ,
                Damage = stats.Damage,
                Speed = stats.ProjectileSpeed
            });

            var cooldown = EntityManager.GetComponent<AttackCooldownComponent>(defenceEntity);
            cooldown.CurrentTime = 0f;
            EntityManager.SetComponent(defenceEntity, cooldown);
            EntityManager.RemoveComponent<ReadyToFireTag>(defenceEntity);
        }
    }
}
