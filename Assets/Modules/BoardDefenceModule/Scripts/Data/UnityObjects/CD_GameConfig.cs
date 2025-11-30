using Strada.Core.Data;
using UnityEngine;

namespace BoardDefence.Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "BoardDefence/Game Config")]
    public class CD_GameConfig : ConfigData
    {
        [SerializeField] private CD_DefenceUnits _defenceUnits;
        [SerializeField] private CD_Enemies _enemies;
        [SerializeField] private CD_Levels _levels;

        private GameConfigData _cachedData;

        public GameConfigData Data
        {
            get
            {
                if (_cachedData == null)
                    BuildData();
                return _cachedData;
            }
        }

        private void BuildData()
        {
            _cachedData = new GameConfigData();

            if (_defenceUnits != null)
            {
                foreach (var kvp in _defenceUnits.Units)
                    _cachedData.DefenceItems[kvp.Key] = kvp.Value;
            }

            if (_enemies != null)
            {
                foreach (var kvp in _enemies.Enemies)
                    _cachedData.Enemies[kvp.Key] = kvp.Value;
            }

            if (_levels != null)
                _cachedData.Levels.AddRange(_levels.Levels);
        }

        public void InvalidateCache() => _cachedData = null;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _cachedData = null;
        }
#endif
    }
}
