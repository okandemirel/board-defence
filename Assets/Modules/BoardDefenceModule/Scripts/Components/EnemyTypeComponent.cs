using Strada.Core.ECS;

namespace BoardDefence.Components
{
    public struct EnemyTypeComponent : IComponent
    {
        public int TypeIndex;
        public int Damage;
        public int ScoreValue;
    }
}
