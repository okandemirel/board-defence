using System;
using System.Collections.Generic;

namespace BoardDefence.Data
{
    [Serializable]
    public class GameConfigData
    {
        public Dictionary<string, DefenceItemData> DefenceItems = new();
        public Dictionary<string, EnemyData> Enemies = new();
        public List<LevelData> Levels = new();
    }
}
