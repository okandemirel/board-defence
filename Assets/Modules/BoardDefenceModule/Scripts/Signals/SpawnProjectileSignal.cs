using Strada.Core.ECS;

namespace BoardDefence.Signals
{
    public struct SpawnProjectileSignal
    {
        public float StartX;
        public float StartY;
        public float StartZ;
        public Entity Target;
        public float TargetX;
        public float TargetY;
        public float TargetZ;
        public int Damage;
        public float Speed;
    }
}
