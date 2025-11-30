using Strada.Core.Sync;

namespace BoardDefence.Events
{
    public struct EnemyReachedBaseEvent
    {
        public EntityHandle Handle;
        public int Damage;
    }
}