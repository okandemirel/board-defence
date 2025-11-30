using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Strada.Modules.Screen;
using Strada.Core.Communication;
using Strada.Core.DI.Attributes;
using BoardDefence.Data;
using BoardDefence.Models;
using BoardDefence.Events;

namespace BoardDefence.UI.Screens
{
    public class GameHUDScreen : ScreenView
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private TextMeshProUGUI _waveText;
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private Transform _defenceCardsContainer;
        [SerializeField] private GameObject _defenceCardPrefab;

        [Inject] private IGameModel _gameModel;
        [Inject] private ILevelModel _levelModel;
        [Inject] private EventBus _eventBus;
        [Inject] private GameConfigData _gameConfig;

        private DefenceCardView[] _defenceCards;

        public override void BeforeSetup()
        {
            base.BeforeSetup();

            if (_gameModel != null)
            {
                _gameModel.Score.Subscribe(OnScoreChanged);
                _gameModel.BaseHealth.Subscribe(OnHealthChanged);
            }

            if (_eventBus != null)
            {
                _eventBus.Subscribe<WaveStartedEvent>(OnWaveStarted);
                _eventBus.Subscribe<DefencePlacedEvent>(OnDefencePlaced);
            }

            UpdateScore(0);
            if (_gameModel != null)
                UpdateHealth(_gameModel.BaseHealth.Value);
        }

        protected override void OnScreenShown()
        {
            CreateDefenceCards();
        }

        protected override void OnScreenDeactivated()
        {
            ClearDefenceCards();
        }

        private void CreateDefenceCards()
        {
            ClearDefenceCards();

            if (_levelModel == null) return;
            var level = _levelModel.CurrentLevel.Value;
            if (level == null) return;

            _defenceCards = new DefenceCardView[level.AvailableDefences.Count];

            for (int i = 0; i < level.AvailableDefences.Count; i++)
            {
                if (_defenceCardPrefab == null || _defenceCardsContainer == null) continue;

                var allocation = level.AvailableDefences[i];
                if (!TryGetDefenceData(allocation.DefenceKey, out var defenceData)) continue;

                var cardGO = Instantiate(_defenceCardPrefab, _defenceCardsContainer);
                var cardView = cardGO.GetComponent<DefenceCardView>();

                cardView.Initialize(allocation.DefenceKey, defenceData, allocation.Count, _eventBus);
                _defenceCards[i] = cardView;
            }
        }

        private bool TryGetDefenceData(string key, out DefenceItemData data)
        {
            data = null;
            if (_gameConfig == null || string.IsNullOrEmpty(key)) return false;
            return _gameConfig.DefenceItems.TryGetValue(key, out data);
        }

        private void ClearDefenceCards()
        {
            if (_defenceCards != null)
            {
                foreach (var card in _defenceCards)
                {
                    if (card != null)
                        Destroy(card.gameObject);
                }
            }
            _defenceCards = null;
        }

        private void OnScoreChanged(int score) => UpdateScore(score);
        private void OnHealthChanged(int health) => UpdateHealth(health);

        private void OnWaveStarted(WaveStartedEvent evt)
        {
            if (_waveText != null)
                _waveText.text = $"Wave {evt.WaveIndex + 1}/{evt.TotalWaves}";
        }

        private void OnDefencePlaced(DefencePlacedEvent evt) => RefreshDefenceCards();

        private void UpdateScore(int score)
        {
            if (_scoreText != null)
                _scoreText.text = $"Score: {score}";
        }

        private void UpdateHealth(int health)
        {
            if (_healthText != null)
                _healthText.text = $"HP: {health}";
            if (_healthSlider != null && _gameModel != null)
                _healthSlider.value = (float)health / _gameModel.MaxBaseHealth;
        }

        public void RefreshDefenceCards()
        {
            if (_defenceCards == null || _levelModel == null) return;
            for (int i = 0; i < _defenceCards.Length; i++)
                _defenceCards[i]?.SetCount(_levelModel.GetRemainingCount(i));
        }
    }
}
