using Strada.Core.Communication;
using Strada.Core.ECS.World;
using Strada.Core.DI.Attributes;
using Strada.Core.ECS.Core;
using Strada.Core.Patterns.Interfaces;
using Strada.Core.Sync;
using BoardDefence.Signals;
using BoardDefence.Components;
using BoardDefence.Data;
using BoardDefence.Events;

namespace BoardDefence.Systems
{
    public class DefenceSpawnSystem : IInitializable
    {
        [Inject] private GameConfigData _gameConfig;
        [Inject] private BoardData _boardData;
        [Inject] private EventBus _eventBus;
        [Inject] private EntityHandleRegistry _handleRegistry;

        private EntityManager EntityManager => World.Current?.EntityManager;

        public void Initialize()
        {
            _eventBus.RegisterSignalHandler<SpawnDefenceSignal>(SpawnDefence);
        }

        private void SpawnDefence(SpawnDefenceSignal signal)
        {
            var entityManager = EntityManager;
            if (entityManager == null) return;

            if (!_gameConfig.DefenceItems.TryGetValue(signal.DefenceKey, out var data))
                return;

            float worldX = signal.Column * _boardData.CellSize;
            float worldZ = signal.Row * _boardData.CellSize;

            var entity = entityManager.CreateEntity();

            entityManager.AddComponent(entity, new DefenceItemTag());

            entityManager.AddComponent(entity, new DefenceTypeComponent
            {
                TypeIndex = data.Id
            });

            entityManager.AddComponent(entity, new GridPositionComponent
            {
                Column = signal.Column,
                Row = signal.Row,
                WorldX = worldX,
                WorldY = 0f,
                WorldZ = worldZ
            });

            entityManager.AddComponent(entity, new AttackStatsComponent
            {
                Damage = data.Damage,
                Range = data.Range,
                Direction = data.Direction,
                ProjectileSpeed = data.ProjectileSpeed
            });

            entityManager.AddComponent(entity, new AttackCooldownComponent
            {
                CurrentTime = 0f,
                Interval = data.AttackInterval
            });

            var handle = _handleRegistry.Register(entity);

            _eventBus.Publish(new DefencePlacedEvent
            {
                Handle = handle,
                Column = signal.Column,
                Row = signal.Row,
                DefenceTypeIndex = data.Id
            });
        }
    }
}
