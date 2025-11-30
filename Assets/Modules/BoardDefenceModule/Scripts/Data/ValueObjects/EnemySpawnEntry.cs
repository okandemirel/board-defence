using System;

namespace BoardDefence.Data
{
    [Serializable]
    public class EnemySpawnEntry
    {
        public string EnemyKey;
        public int Count = 1;
        public int SpawnColumn = -1;
    }
}
