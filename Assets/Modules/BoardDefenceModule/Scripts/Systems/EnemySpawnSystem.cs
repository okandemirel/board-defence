using Strada.Core.Bootstrap;
using Strada.Core.ECS.Systems;
using Strada.Core.ECS.World;
using Strada.Core.Modules;
using BoardDefence.Signals;
using BoardDefence.Components;
using BoardDefence.Data;
using BoardDefence.Events;

namespace BoardDefence.Systems
{
    [StradaSystem(
        Module = "BoardDefence",
        Category = "Spawning",
        Description = "Spawns enemy entities in response to signals",
        Phase = UpdatePhase.Update,
        Order = 60)]
    public class EnemySpawnSystem : SystemBase
    {
        private GameConfigData _gameConfig;
        private float _cellSize;
        private int _rows;

        protected override void OnInitialize()
        {
            _gameConfig = GameBootstrapper.Services.Get<GameConfigData>();
            RegisterSignalHandler<SpawnEnemySignal>(SpawnEnemy);
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (_cellSize == 0f)
            {
                ForEach<BoardConfigComponent>((int idx, ref BoardConfigComponent config) =>
                {
                    _cellSize = config.CellSize;
                    _rows = config.Rows;
                });
            }
        }

        private void SpawnEnemy(SpawnEnemySignal signal)
        {
            if (!_gameConfig.Enemies.TryGetValue(signal.EnemyKey, out var data))
                return;

            float worldX = signal.Column * _cellSize;
            float worldZ = _rows * _cellSize;

            var entity = CreateEntity();

            EntityManager.AddComponent(entity, new EnemyTag());
            EntityManager.AddComponent(entity, new EnemyTypeComponent
            {
                TypeIndex = data.Id,
                Damage = data.Damage,
                ScoreValue = data.ScoreValue
            });
            EntityManager.AddComponent(entity, new GridPositionComponent
            {
                Column = signal.Column,
                Row = _rows,
                WorldX = worldX,
                WorldY = 0f,
                WorldZ = worldZ
            });
            EntityManager.AddComponent(entity, new MoveSpeedComponent
            {
                BlocksPerSecond = data.MoveSpeed
            });
            EntityManager.AddComponent(entity, new HealthComponent
            {
                Current = data.MaxHealth,
                Max = data.MaxHealth
            });

            var handle = HandleRegistry.Register(entity);

            Publish(new EnemySpawnedEvent
            {
                Handle = handle,
                Column = signal.Column,
                EnemyTypeIndex = data.Id
            });
        }
    }
}
