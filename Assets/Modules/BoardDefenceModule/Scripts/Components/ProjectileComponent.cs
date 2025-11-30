using Strada.Core.ECS;

namespace BoardDefence.Components
{
    public struct ProjectileComponent : IComponent
    {
        public Entity TargetEntity;
        public int Damage;
        public float Speed;
        public float TargetX;
        public float TargetY;
        public float TargetZ;
    }
}
