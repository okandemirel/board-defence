using System.Collections.Generic;
using Strada.Core.ECS;
using Strada.Core.ECS.Systems;
using Strada.Core.ECS.World;
using Strada.Core.Modules;
using BoardDefence.Components;
using BoardDefence.Events;

namespace BoardDefence.Systems
{
    [StradaSystem(
        Module = "BoardDefence",
        Category = "Lifecycle",
        Description = "Detects dead entities and marks them for destruction",
        Phase = UpdatePhase.Update,
        Order = 500)]
    public class DeathDetectionSystem : SystemBase
    {
        private List<Entity> _deadEntities = new(32);

        protected override void OnUpdate(float deltaTime)
        {
            _deadEntities.Clear();

            ForEach<EnemyTag, HealthComponent>((int entityIndex, ref EnemyTag tag, ref HealthComponent health) =>
            {
                if (health.Current <= 0)
                {
                    _deadEntities.Add(EntityManager.GetEntity(entityIndex));
                }
            });

            foreach (var entity in _deadEntities)
            {
                if (!EntityManager.HasComponent<DestroyTag>(entity))
                {
                    EntityManager.AddComponent<DestroyTag>(entity);

                    int scoreValue = 10;
                    if (EntityManager.HasComponent<EnemyTypeComponent>(entity))
                    {
                        var enemyType = EntityManager.GetComponent<EnemyTypeComponent>(entity);
                        scoreValue = enemyType.ScoreValue;
                    }

                    var handle = HandleRegistry.Register(entity);
                    Publish(new EnemyKilledEvent { Handle = handle, ScoreValue = scoreValue });
                    Publish(new EntityDestroyedEvent { Handle = handle });
                }
            }
        }
    }
}