using Strada.Core.Patterns.Interfaces;
using BoardDefence.Data;

namespace BoardDefence.Services
{
    public interface ISpawnService : IService
    {
        bool IsSpawning { get; }
        int RemainingEnemies { get; }

        void StartWave(WaveData waveData);
        void StopWave();
    }
}
