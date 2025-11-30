using System.Collections.Generic;
using System.Linq;
using Strada.Core.Data;
using UnityEngine;

namespace BoardDefence.Data
{
    [CreateAssetMenu(fileName = "Enemies", menuName = "BoardDefence/Enemies")]
    public class CD_Enemies : ConfigData
    {
        [SerializeField] private List<EnemyEntry> _entries = new();

        private Dictionary<string, EnemyData> _cache;

        public IReadOnlyDictionary<string, EnemyData> Enemies
        {
            get
            {
                if (_cache == null)
                    BuildCache();
                return _cache;
            }
        }

        private void BuildCache()
        {
            _cache = _entries
                .Where(e => e != null && !string.IsNullOrEmpty(e.Key) && e.Value != null)
                .ToDictionary(e => e.Key, e => e.Value);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _cache = null;
        }
#endif
    }
}
