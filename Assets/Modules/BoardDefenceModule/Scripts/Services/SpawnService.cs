using System.Collections.Generic;
using Strada.Core.Patterns;
using Strada.Core.DI.Attributes;
using BoardDefence.Data;
using BoardDefence.Models;
using BoardDefence.Signals;

namespace BoardDefence.Services
{
    public class SpawnService : TickableService, ISpawnService
    {
        [Inject] private IBoardModel _boardModel;

        private Queue<EnemySpawnEntry> _spawnQueue = new();
        private float _spawnTimer;
        private float _spawnInterval;
        private bool _isSpawning;

        public bool IsSpawning => _isSpawning;
        public int RemainingEnemies => _spawnQueue.Count;

        public void StartWave(WaveData waveData)
        {
            _spawnInterval = waveData.SpawnInterval;
            _spawnTimer = _spawnInterval;
            _isSpawning = true;

            _spawnQueue.Clear();

            foreach (var entry in waveData.Enemies)
            {
                for (int i = 0; i < entry.Count; i++)
                {
                    _spawnQueue.Enqueue(entry);
                }
            }
        }

        public void StopWave()
        {
            _isSpawning = false;
            _spawnQueue.Clear();
        }

        public override void Tick(float deltaTime)
        {
            if (!_isSpawning || _spawnQueue.Count == 0) return;

            _spawnTimer += deltaTime;

            if (_spawnTimer >= _spawnInterval)
            {
                _spawnTimer = 0f;
                SpawnNextEnemy();
            }
        }

        private void SpawnNextEnemy()
        {
            if (_spawnQueue.Count == 0)
            {
                _isSpawning = false;
                return;
            }

            var entry = _spawnQueue.Dequeue();
            int column = entry.SpawnColumn >= 0 ? entry.SpawnColumn : UnityEngine.Random.Range(0, _boardModel.Columns);

            Send(new SpawnEnemySignal
            {
                EnemyKey = entry.EnemyKey,
                Column = column
            });

            if (_spawnQueue.Count == 0)
            {
                _isSpawning = false;
            }
        }
    }
}