using System.Collections.Generic;
using Strada.Core.Patterns;
using Strada.Core.Sync;
using Strada.Core.DI.Attributes;
using BoardDefence.Data;

namespace BoardDefence.Models
{
    public class LevelModel : Model, ILevelModel
    {
        [Inject] private GameConfigData _gameConfig;

        private ReactiveProperty<LevelData> _currentLevel;
        private ReactiveProperty<int> _currentWaveIndex;
        private Dictionary<int, int> _defenceCounts = new();

        public IReadOnlyReactiveProperty<LevelData> CurrentLevel => _currentLevel;
        public IReadOnlyReactiveProperty<int> CurrentWaveIndex => _currentWaveIndex;

        protected override void OnInitialize()
        {
            _currentLevel = CreateProperty<LevelData>(null);
            _currentWaveIndex = CreateProperty(0);
        }

        public void LoadLevel(LevelData levelData)
        {
            _currentLevel.Value = levelData;
            _currentWaveIndex.Value = 0;
            _defenceCounts.Clear();

            for (int i = 0; i < levelData.AvailableDefences.Count; i++)
            {
                _defenceCounts[i] = levelData.AvailableDefences[i].Count;
            }
        }

        public bool ConsumeDefence(int defenceTypeIndex)
        {
            if (!_defenceCounts.TryGetValue(defenceTypeIndex, out var count))
                return false;

            if (count <= 0)
                return false;

            _defenceCounts[defenceTypeIndex] = count - 1;
            return true;
        }

        public int GetRemainingCount(int defenceTypeIndex)
        {
            if (_defenceCounts.TryGetValue(defenceTypeIndex, out var count))
                return count;
            return 0;
        }

        public string GetDefenceKey(int defenceTypeIndex)
        {
            var level = _currentLevel.Value;
            if (level == null) return null;

            if (defenceTypeIndex < 0 || defenceTypeIndex >= level.AvailableDefences.Count)
                return null;

            return level.AvailableDefences[defenceTypeIndex].DefenceKey;
        }

        public DefenceItemData GetDefenceData(int defenceTypeIndex)
        {
            var defenceKey = GetDefenceKey(defenceTypeIndex);
            if (string.IsNullOrEmpty(defenceKey)) return null;

            if (_gameConfig.DefenceItems.TryGetValue(defenceKey, out var data))
                return data;

            return null;
        }

        public void SetWaveIndex(int waveIndex)
        {
            _currentWaveIndex.Value = waveIndex;
        }

        public void Reset()
        {
            var level = _currentLevel.Value;
            if (level == null) return;

            _currentWaveIndex.Value = 0;
            _defenceCounts.Clear();

            for (int i = 0; i < level.AvailableDefences.Count; i++)
            {
                _defenceCounts[i] = level.AvailableDefences[i].Count;
            }
        }
    }
}
