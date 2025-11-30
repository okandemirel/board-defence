using System;
using System.Collections.Generic;

namespace BoardDefence.Data
{
    [Serializable]
    public class LevelData
    {
        public string LevelName;
        public List<DefenceAllocation> AvailableDefences = new();
        public List<WaveData> Waves = new();
    }
}