using Strada.Core.Sync;

namespace BoardDefence.Events
{
    public struct EnemySpawnedEvent
    {
        public EntityHandle Handle;
        public int Column;
        public int EnemyTypeIndex;
    }
}