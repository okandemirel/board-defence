using Strada.Core.Patterns.Interfaces;
using Strada.Core.Sync;
using BoardDefence.Data;

namespace BoardDefence.Models
{
    public interface ILevelModel : IModel
    {
        IReadOnlyReactiveProperty<LevelData> CurrentLevel { get; }
        IReadOnlyReactiveProperty<int> CurrentWaveIndex { get; }

        void LoadLevel(LevelData levelData);
        bool ConsumeDefence(int defenceTypeIndex);
        int GetRemainingCount(int defenceTypeIndex);
        string GetDefenceKey(int defenceTypeIndex);
        DefenceItemData GetDefenceData(int defenceTypeIndex);
        void SetWaveIndex(int waveIndex);
        void Reset();
    }
}
