using System;

namespace BoardDefence.Data
{
    [Serializable]
    public class EnemyData
    {
        public int Id;
        public int MaxHealth;
        public float MoveSpeed;
        public int Damage = 1;
        public int ScoreValue = 10;
    }

    [Serializable]
    public class EnemyEntry
    {
        public string Key;
        public EnemyData Value;
    }
}
