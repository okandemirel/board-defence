using Strada.Core.ECS;
using Strada.Core.ECS.Systems;
using Strada.Core.ECS.World;
using Strada.Core.Modules;
using Strada.Core.Sync;
using BoardDefence.Components;
using BoardDefence.Events;

namespace BoardDefence.Systems
{
    [StradaSystem(
        Module = "BoardDefence",
        Category = "Movement",
        Description = "Moves enemies toward the player base",
        Phase = UpdatePhase.Update,
        Order = 100)]
    public class EnemyMovementSystem : SystemBase
    {
        private float _cellSize;

        protected override void OnUpdate(float deltaTime)
        {
            if (_cellSize == 0f)
            {
                ForEach<BoardConfigComponent>((int idx, ref BoardConfigComponent config) =>
                {
                    _cellSize = config.CellSize;
                });
            }

            var cellSize = _cellSize;

            ForEach<EnemyTag, GridPositionComponent, MoveSpeedComponent>(
                (int entityIndex, ref EnemyTag tag, ref GridPositionComponent pos, ref MoveSpeedComponent speed) =>
                {
                    pos.WorldZ -= speed.BlocksPerSecond * cellSize * deltaTime;
                    pos.Row = (int)(pos.WorldZ / cellSize);

                    if (pos.WorldZ < 0)
                    {
                        var entity = EntityManager.GetEntity(entityIndex);

                        int damage = 1;
                        if (EntityManager.HasComponent<EnemyTypeComponent>(entity))
                        {
                            var enemyType = EntityManager.GetComponent<EnemyTypeComponent>(entity);
                            damage = enemyType.Damage;
                        }

                        var handle = HandleRegistry.Register(entity);
                        Publish(new EnemyReachedBaseEvent { Handle = handle, Damage = damage });
                        Publish(new EntityDestroyedEvent { Handle = handle });
                        EntityManager.AddComponent<DestroyTag>(entity);
                    }
                });
        }
    }
}
