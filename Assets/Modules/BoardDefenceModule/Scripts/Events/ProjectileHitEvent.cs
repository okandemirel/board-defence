using Strada.Core.Sync;

namespace BoardDefence.Events
{
    public struct ProjectileHitEvent
    {
        public EntityHandle ProjectileHandle;
        public EntityHandle TargetHandle;
        public int Damage;
    }
}