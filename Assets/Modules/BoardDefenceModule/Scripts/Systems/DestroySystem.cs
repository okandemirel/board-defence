using System.Collections.Generic;
using Strada.Core.ECS;
using Strada.Core.ECS.Systems;
using Strada.Core.ECS.World;
using Strada.Core.Modules;
using BoardDefence.Components;

namespace BoardDefence.Systems
{
    [StradaSystem(
        Module = "BoardDefence",
        Category = "Lifecycle",
        Description = "Destroys entities marked with DestroyTag",
        Phase = UpdatePhase.LateUpdate,
        Order = 1000)]
    public class DestroySystem : SystemBase
    {
        private List<Entity> _toDestroy = new(32);

        protected override void OnUpdate(float deltaTime)
        {
            _toDestroy.Clear();

            ForEach<DestroyTag>((int entityIndex, ref DestroyTag tag) =>
            {
                _toDestroy.Add(EntityManager.GetEntity(entityIndex));
            });

            foreach (var entity in _toDestroy)
            {
                EntityManager.DestroyEntity(entity);
            }
        }
    }
}
