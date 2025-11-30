using System;
using System.Collections.Generic;

namespace BoardDefence.Data
{
    [Serializable]
    public class WaveData
    {
        public int WaveNumber = 1;
        public float DelayBeforeWave = 2f;
        public List<EnemySpawnEntry> Enemies = new();
        public float SpawnInterval = 1.5f;
    }
}
