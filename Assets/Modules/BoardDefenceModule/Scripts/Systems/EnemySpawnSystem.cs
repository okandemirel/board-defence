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
    public class EnemySpawnSystem : IInitializable
    {
        [Inject] private GameConfigData _gameConfig;
        [Inject] private BoardData _boardData;
        [Inject] private EventBus _eventBus;
        [Inject] private EntityHandleRegistry _handleRegistry;

        private EntityManager EntityManager => World.Current?.EntityManager;

        public void Initialize()
        {
            _eventBus.RegisterSignalHandler<SpawnEnemySignal>(SpawnEnemy);
        }

        private void SpawnEnemy(SpawnEnemySignal signal)
        {
            var entityManager = EntityManager;
            if (entityManager == null) return;

            if (!_gameConfig.Enemies.TryGetValue(signal.EnemyKey, out var data))
                return;

            float worldX = signal.Column * _boardData.CellSize;
            float worldZ = _boardData.Rows * _boardData.CellSize;

            var entity = entityManager.CreateEntity();

            entityManager.AddComponent(entity, new EnemyTag());

            entityManager.AddComponent(entity, new EnemyTypeComponent
            {
                TypeIndex = data.Id,
                Damage = data.Damage,
                ScoreValue = data.ScoreValue
            });

            entityManager.AddComponent(entity, new GridPositionComponent
            {
                Column = signal.Column,
                Row = _boardData.Rows,
                WorldX = worldX,
                WorldY = 0f,
                WorldZ = worldZ
            });

            entityManager.AddComponent(entity, new MoveSpeedComponent
            {
                BlocksPerSecond = data.MoveSpeed
            });

            entityManager.AddComponent(entity, new HealthComponent
            {
                Current = data.MaxHealth,
                Max = data.MaxHealth
            });

            var handle = _handleRegistry.Register(entity);

            _eventBus.Publish(new EnemySpawnedEvent
            {
                Handle = handle,
                Column = signal.Column,
                EnemyTypeIndex = data.Id
            });
        }
    }
}
