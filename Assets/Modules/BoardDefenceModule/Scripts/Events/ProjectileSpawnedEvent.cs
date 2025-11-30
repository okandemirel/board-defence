using Strada.Core.Sync;

namespace BoardDefence.Events
{
    public struct ProjectileSpawnedEvent
    {
        public EntityHandle Handle;
        public EntityHandle TargetHandle;
    }
}