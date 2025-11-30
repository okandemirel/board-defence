using Strada.Core.Communication;
using Strada.Core.ECS.World;
using Strada.Core.DI.Attributes;
using Strada.Core.ECS.Core;
using Strada.Core.Patterns.Interfaces;
using Strada.Core.Sync;
using BoardDefence.Signals;
using BoardDefence.Components;
using BoardDefence.Events;

namespace BoardDefence.Systems
{
    public class ProjectileSpawnSystem : IInitializable
    {
        [Inject] private EventBus _eventBus;
        [Inject] private EntityHandleRegistry _handleRegistry;

        private EntityManager EntityManager => World.Current?.EntityManager;

        public void Initialize()
        {
            _eventBus.RegisterSignalHandler<SpawnProjectileSignal>(SpawnProjectile);
        }

        private void SpawnProjectile(SpawnProjectileSignal signal)
        {
            var entityManager = EntityManager;
            if (entityManager == null) return;

            var entity = entityManager.CreateEntity();

            entityManager.AddComponent(entity, new ProjectileTag());

            entityManager.AddComponent(entity, new GridPositionComponent
            {
                Column = 0,
                Row = 0,
                WorldX = signal.StartX,
                WorldY = signal.StartY,
                WorldZ = signal.StartZ
            });

            entityManager.AddComponent(entity, new ProjectileComponent
            {
                TargetEntity = signal.Target,
                Damage = signal.Damage,
                Speed = signal.Speed,
                TargetX = signal.TargetX,
                TargetY = signal.TargetY,
                TargetZ = signal.TargetZ
            });

            var handle = _handleRegistry.Register(entity);
            var targetHandle = _handleRegistry.Register(signal.Target);

            _eventBus.Publish(new ProjectileSpawnedEvent
            {
                Handle = handle,
                TargetHandle = targetHandle
            });
        }
    }
}
