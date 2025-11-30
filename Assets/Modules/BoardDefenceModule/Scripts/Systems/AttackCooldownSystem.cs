using Strada.Core.ECS;
using Strada.Core.ECS.Systems;
using Strada.Core.ECS.World;
using Strada.Core.Modules;
using BoardDefence.Components;

namespace BoardDefence.Systems
{
    [StradaSystem(
        Module = "BoardDefence",
        Category = "Combat",
        Description = "Manages attack cooldown timers for defence items",
        Phase = UpdatePhase.Update,
        Order = 200)]
    public class AttackCooldownSystem : SystemBase
    {
        protected override void OnUpdate(float deltaTime)
        {
            ForEach<DefenceItemTag, AttackCooldownComponent>(
                (int entityIndex, ref DefenceItemTag tag, ref AttackCooldownComponent cooldown) =>
                {
                    cooldown.CurrentTime += deltaTime;

                    if (cooldown.CurrentTime >= cooldown.Interval)
                    {
                        var entity = EntityManager.GetEntity(entityIndex);
                        if (!EntityManager.HasComponent<ReadyToFireTag>(entity))
                        {
                            EntityManager.AddComponent<ReadyToFireTag>(entity);
                        }
                    }
                });
        }
    }
}
