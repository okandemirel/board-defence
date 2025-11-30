using Strada.Core.ECS;
using BoardDefence.Data;

namespace BoardDefence.Components
{
    public struct AttackStatsComponent : IComponent
    {
        public int Damage;
        public float Range;
        public AttackDirection Direction;
        public float ProjectileSpeed;
    }
}
