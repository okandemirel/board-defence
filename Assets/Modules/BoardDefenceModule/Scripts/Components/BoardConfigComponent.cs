using Strada.Core.ECS;

namespace BoardDefence.Components
{
    public struct BoardConfigComponent : IComponent
    {
        public int Columns;
        public int Rows;
        public int PlaceableRowsFromBottom;
        public float CellSize;
        public float CellSpacing;
    }
}
