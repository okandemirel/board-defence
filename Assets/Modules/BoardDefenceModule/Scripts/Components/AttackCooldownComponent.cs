using Strada.Core.ECS;

namespace BoardDefence.Components
{
    public struct AttackCooldownComponent : IComponent
    {
        public float CurrentTime;
        public float Interval;
    }
}
