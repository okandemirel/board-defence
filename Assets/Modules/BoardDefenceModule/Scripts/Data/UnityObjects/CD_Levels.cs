using System.Collections.Generic;
using Strada.Core.Data;
using UnityEngine;

namespace BoardDefence.Data
{
    [CreateAssetMenu(fileName = "Levels", menuName = "BoardDefence/Levels")]
    public class CD_Levels : ConfigData
    {
        [SerializeField] private List<LevelData> _levels = new();

        public IReadOnlyList<LevelData> Levels => _levels;

#if UNITY_EDITOR
        public void SetLevels(List<LevelData> levels) => _levels = levels ?? new List<LevelData>();
#endif
    }
}
