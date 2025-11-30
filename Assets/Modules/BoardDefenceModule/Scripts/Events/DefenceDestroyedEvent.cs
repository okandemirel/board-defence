using Strada.Core.Sync;

namespace BoardDefence.Events
{
    public struct DefenceDestroyedEvent
    {
        public EntityHandle Handle;
        public int Column;
        public int Row;
    }
}
