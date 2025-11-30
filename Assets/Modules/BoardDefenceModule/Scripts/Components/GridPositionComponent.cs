using Strada.Core.ECS;

namespace BoardDefence.Components
{
    public struct GridPositionComponent : IComponent
    {
        public int Column;
        public int Row;
        public float WorldX;
        public float WorldY;
        public float WorldZ;
    }
}
