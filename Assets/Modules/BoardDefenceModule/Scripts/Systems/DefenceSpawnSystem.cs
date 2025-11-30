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
        Description = "Spawns defence entities in response to signals",
        Phase = UpdatePhase.Update,
        Order = 50)]
    public class DefenceSpawnSystem : SystemBase
    {
        private GameConfigData _gameConfig;
        private float _cellSize;

        protected override void OnInitialize()
        {
            _gameConfig = GameBootstrapper.Services.Get<GameConfigData>();
            RegisterSignalHandler<SpawnDefenceSignal>(SpawnDefence);
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (_cellSize == 0f)
            {
                ForEach<BoardConfigComponent>((int idx, ref BoardConfigComponent config) =>
                {
                    _cellSize = config.CellSize;
                });
            }
        }

        private void SpawnDefence(SpawnDefenceSignal signal)
        {
            if (!_gameConfig.DefenceItems.TryGetValue(signal.DefenceKey, out var data))
                return;

            float worldX = signal.Column * _cellSize;
            float worldZ = signal.Row * _cellSize;

            var entity = CreateEntity();

            EntityManager.AddComponent(entity, new DefenceItemTag());
            EntityManager.AddComponent(entity, new DefenceTypeComponent { TypeIndex = data.Id });
            EntityManager.AddComponent(entity, new GridPositionComponent
            {
                Column = signal.Column,
                Row = signal.Row,
                WorldX = worldX,
                WorldY = 0f,
                WorldZ = worldZ
            });
            EntityManager.AddComponent(entity, new AttackStatsComponent
            {
                Damage = data.Damage,
                Range = data.Range,
                Direction = data.Direction,
                ProjectileSpeed = data.ProjectileSpeed
            });
            EntityManager.AddComponent(entity, new AttackCooldownComponent
            {
                CurrentTime = 0f,
                Interval = data.AttackInterval
            });

            var handle = HandleRegistry.Register(entity);

            Publish(new DefencePlacedEvent
            {
                Handle = handle,
                Column = signal.Column,
                Row = signal.Row,
                DefenceTypeIndex = data.Id
            });
        }
    }
}
