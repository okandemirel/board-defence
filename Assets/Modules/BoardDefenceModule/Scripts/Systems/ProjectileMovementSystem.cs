using Strada.Core.ECS;
using Strada.Core.ECS.Systems;
using Strada.Core.ECS.World;
using Strada.Core.Modules;
using BoardDefence.Components;
using BoardDefence.Events;
using UnityEngine;

namespace BoardDefence.Systems
{
    [StradaSystem(
        Module = "BoardDefence",
        Category = "Combat",
        Description = "Moves projectiles toward targets and handles hit detection",
        Phase = UpdatePhase.Update,
        Order = 400)]
    public class ProjectileMovementSystem : SystemBase
    {
        private const float HIT_DISTANCE = 0.3f;

        protected override void OnUpdate(float deltaTime)
        {
            ForEach<ProjectileTag, GridPositionComponent, ProjectileComponent>(
                (int entityIndex, ref ProjectileTag tag, ref GridPositionComponent pos, ref ProjectileComponent projectile) =>
                {
                    float dx = projectile.TargetX - pos.WorldX;
                    float dy = projectile.TargetY - pos.WorldY;
                    float dz = projectile.TargetZ - pos.WorldZ;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);

                    if (dist < HIT_DISTANCE)
                    {
                        var entity = EntityManager.GetEntity(entityIndex);
                        ProcessHit(entity, projectile);
                        return;
                    }

                    float moveAmount = projectile.Speed * deltaTime;
                    if (moveAmount >= dist)
                    {
                        pos.WorldX = projectile.TargetX;
                        pos.WorldY = projectile.TargetY;
                        pos.WorldZ = projectile.TargetZ;
                        var entity = EntityManager.GetEntity(entityIndex);
                        ProcessHit(entity, projectile);
                    }
                    else
                    {
                        float t = moveAmount / dist;
                        pos.WorldX += dx * t;
                        pos.WorldY += dy * t;
                        pos.WorldZ += dz * t;
                    }
                });
        }

        private void ProcessHit(Entity projectileEntity, ProjectileComponent projectile)
        {
            if (!EntityManager.HasComponent<ProjectileTag>(projectileEntity))
                return;

            if (!projectile.TargetEntity.IsNull && EntityManager.Exists(projectile.TargetEntity))
            {
                if (EntityManager.HasComponent<HealthComponent>(projectile.TargetEntity))
                {
                    ref var health = ref EntityManager.GetComponentRef<HealthComponent>(projectile.TargetEntity);
                    health.Current -= projectile.Damage;

                    var targetHandle = HandleRegistry.Register(projectile.TargetEntity);
                    var projectileHandle = HandleRegistry.Register(projectileEntity);

                    Publish(new EnemyDamagedEvent
                    {
                        Handle = targetHandle,
                        Damage = projectile.Damage,
                        RemainingHealth = health.Current
                    });

                    Publish(new ProjectileHitEvent
                    {
                        ProjectileHandle = projectileHandle,
                        TargetHandle = targetHandle,
                        Damage = projectile.Damage
                    });
                }
            }

            var destroyHandle = HandleRegistry.Register(projectileEntity);
            Publish(new EntityDestroyedEvent { Handle = destroyHandle });
            EntityManager.AddComponent<DestroyTag>(projectileEntity);
        }
    }
}
