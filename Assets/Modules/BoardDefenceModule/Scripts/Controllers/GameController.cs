using Strada.Core.Patterns;
using Strada.Core.DI.Attributes;
using BoardDefence.Models;
using BoardDefence.Events;
using BoardDefence.Data;
using BoardDefence.Services;

namespace BoardDefence.Controllers
{
    public class GameController : Controller
    {
        [Inject] private IGameModel _gameModel;
        [Inject] private ILevelModel _levelModel;
        [Inject] private IBoardModel _boardModel;
        [Inject] private ISpawnService _spawnService;
        [Inject] private GameConfigData _config;

        private int _aliveEnemyCount;
        private int _currentLevelIndex;

        protected override void OnInitialize()
        {
            Subscribe<EnemySpawnedEvent>(OnEnemySpawned);
            Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            Subscribe<EnemyReachedBaseEvent>(OnEnemyReachedBase);

            RegisterSignalHandler<StartGameSignal>(OnStartGameSignal);
            RegisterSignalHandler<StartLevelSignal>(OnStartLevelSignal);
            RegisterSignalHandler<RestartLevelSignal>(OnRestartLevelSignal);
            RegisterSignalHandler<ReturnToMenuSignal>(OnReturnToMenuSignal);
            RegisterSignalHandler<NextLevelSignal>(OnNextLevelSignal);
        }

        private void OnStartGameSignal(StartGameSignal signal)
        {
            _gameModel.SetState(GameState.Menu);
        }

        private void OnStartLevelSignal(StartLevelSignal signal)
        {
            if (signal.LevelIndex < 0 || signal.LevelIndex >= _config.Levels.Count) return;

            CleanupCurrentLevel();

            _currentLevelIndex = signal.LevelIndex;
            _aliveEnemyCount = 0;

            var levelData = _config.Levels[signal.LevelIndex];
            _levelModel.LoadLevel(levelData);
            _boardModel.Reset();
            _gameModel.Reset();
            _gameModel.SetState(GameState.Playing);

            Publish(new LevelStartedEvent { LevelIndex = signal.LevelIndex });

            StartNextWave();
        }

        private void OnRestartLevelSignal(RestartLevelSignal signal)
        {
            CleanupCurrentLevel();

            _aliveEnemyCount = 0;
            var currentLevel = _levelModel.CurrentLevel.Value;
            if (currentLevel != null)
            {
                _levelModel.LoadLevel(currentLevel);
                _boardModel.Reset();
                _gameModel.Reset();
                _gameModel.SetState(GameState.Playing);

                Publish(new LevelStartedEvent { LevelIndex = _currentLevelIndex });

                StartNextWave();
            }
        }

        private void OnReturnToMenuSignal(ReturnToMenuSignal signal)
        {
            CleanupCurrentLevel();
            _gameModel.SetState(GameState.Menu);
        }

        private void CleanupCurrentLevel()
        {
            _spawnService.StopWave();
            Send(new CleanupLevelSignal());
        }

        private void OnNextLevelSignal(NextLevelSignal signal)
        {
            int nextLevelIndex = _currentLevelIndex + 1;

            if (nextLevelIndex >= _config.Levels.Count)
            {
                nextLevelIndex = 0;
            }

            Send(new StartLevelSignal { LevelIndex = nextLevelIndex });
        }

        private void OnEnemySpawned(EnemySpawnedEvent evt)
        {
            _aliveEnemyCount++;
        }

        private void OnEnemyKilled(EnemyKilledEvent evt)
        {
            _gameModel.AddScore(evt.ScoreValue);
            _aliveEnemyCount--;
            CheckWaveCompletion();
        }

        private void OnEnemyReachedBase(EnemyReachedBaseEvent evt)
        {
            _gameModel.TakeDamage(evt.Damage);
            _aliveEnemyCount--;

            if (_gameModel.BaseHealth.Value <= 0)
            {
                _spawnService.StopWave();
                _gameModel.SetState(GameState.GameOver);
                Publish(new GameOverEvent { Victory = false, FinalScore = _gameModel.Score.Value });
            }
            else
            {
                CheckWaveCompletion();
            }
        }

        private void CheckWaveCompletion()
        {
            if (_gameModel.State.Value != GameState.Playing) return;
            if (_spawnService.IsSpawning) return;
            if (_aliveEnemyCount > 0) return;

            var currentLevel = _levelModel.CurrentLevel.Value;
            if (currentLevel == null) return;

            int currentWave = _levelModel.CurrentWaveIndex.Value;
            int nextWave = currentWave + 1;

            Publish(new WaveCompletedEvent { WaveIndex = currentWave });

            if (nextWave >= currentLevel.Waves.Count)
            {
                _gameModel.SetState(GameState.Victory);
                Publish(new LevelCompletedEvent
                {
                    LevelIndex = _currentLevelIndex,
                    FinalScore = _gameModel.Score.Value
                });
                Publish(new GameOverEvent { Victory = true, FinalScore = _gameModel.Score.Value });
            }
            else
            {
                _levelModel.SetWaveIndex(nextWave);
                StartNextWave();
            }
        }

        private void StartNextWave()
        {
            var currentLevel = _levelModel.CurrentLevel.Value;
            if (currentLevel == null) return;

            int waveIndex = _levelModel.CurrentWaveIndex.Value;
            if (waveIndex >= currentLevel.Waves.Count) return;

            var waveData = currentLevel.Waves[waveIndex];
            _spawnService.StartWave(waveData);

            Publish(new WaveStartedEvent
            {
                WaveIndex = waveIndex,
                TotalWaves = currentLevel.Waves.Count
            });
        }
    }
}
