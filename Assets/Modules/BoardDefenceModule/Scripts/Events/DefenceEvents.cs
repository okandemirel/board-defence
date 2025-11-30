namespace BoardDefence.Events
{
    public struct DefenceSelectedEvent
    {
        public int DefenceIndex;
    }

    public struct PlacementValidEvent
    {
        public int Row;
        public int Column;
        public bool IsValid;
    }
}
