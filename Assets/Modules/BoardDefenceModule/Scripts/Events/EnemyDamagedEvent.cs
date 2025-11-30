using Strada.Core.Sync;

namespace BoardDefence.Events
{
    public struct EnemyDamagedEvent
    {
        public EntityHandle Handle;
        public int Damage;
        public int RemainingHealth;
    }
}