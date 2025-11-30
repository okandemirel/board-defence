using Strada.Core.ECS;

namespace BoardDefence.Components
{
    public struct HealthComponent : IComponent
    {
        public int Current;
        public int Max;
    }
}
