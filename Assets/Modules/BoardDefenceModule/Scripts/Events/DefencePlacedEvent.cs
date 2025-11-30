using Strada.Core.Sync;

namespace BoardDefence.Events
{
    public struct DefencePlacedEvent
    {
        public EntityHandle Handle;
        public int Column;
        public int Row;
        public int DefenceTypeIndex;
    }
}