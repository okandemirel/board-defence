using Strada.Core.ECS.Systems;
using Strada.Core.ECS.World;
using Strada.Core.Modules;
using BoardDefence.Signals;
using BoardDefence.Components;
using BoardDefence.Events;

namespace BoardDefence.Systems
{
    [StradaSystem(
        Module = "BoardDefence",
        Category = "Spawning",
        Description = "Spawns projectile entities in response to signals",
        Phase = UpdatePhase.Update,
        Order = 100)]
    public class ProjectileSpawnSystem : SystemBase
    {
        protected override void OnInitialize()
        {
            RegisterSignalHandler<SpawnProjectileSignal>(SpawnProjectile);
        }

        protected override void OnUpdate(float deltaTime) { }

        private void SpawnProjectile(SpawnProjectileSignal signal)
        {
            var entity = CreateEntity();

            EntityManager.AddComponent(entity, new ProjectileTag());
            EntityManager.AddComponent(entity, new GridPositionComponent
            {
                Column = 0,
                Row = 0,
                WorldX = signal.StartX,
                WorldY = signal.StartY,
                WorldZ = signal.StartZ
            });
            EntityManager.AddComponent(entity, new ProjectileComponent
            {
                TargetEntity = signal.Target,
                Damage = signal.Damage,
                Speed = signal.Speed,
                TargetX = signal.TargetX,
                TargetY = signal.TargetY,
                TargetZ = signal.TargetZ
            });

            var handle = HandleRegistry.Register(entity);
            var targetHandle = HandleRegistry.Register(signal.Target);

            Publish(new ProjectileSpawnedEvent
            {
                Handle = handle,
                TargetHandle = targetHandle
            });
        }
    }
}
