using Strada.Core.Sync;

namespace BoardDefence.Events
{
    public struct EnemyKilledEvent
    {
        public EntityHandle Handle;
        public int ScoreValue;
    }
}