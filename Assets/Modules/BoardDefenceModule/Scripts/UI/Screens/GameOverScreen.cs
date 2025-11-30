using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Strada.Modules.Screen;
using Strada.Core.Communication;
using Strada.Core.DI.Attributes;
using BoardDefence.Events;

namespace BoardDefence.UI.Screens
{
    public class GameOverScreen : ScreenView
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _menuButton;

        [Inject] private EventBus _eventBus;

        private bool _isVictory;

        public override void SetParameters(object[] parameters)
        {
            if (parameters?.Length >= 2)
            {
                bool victory = (bool)parameters[0];
                int score = (int)parameters[1];
                SetResult(victory, score);
            }
        }

        public override void BeforeSetup()
        {
            base.BeforeSetup();
            _restartButton?.onClick.AddListener(OnRestartClicked);
            _nextLevelButton?.onClick.AddListener(OnNextLevelClicked);
            _menuButton?.onClick.AddListener(OnMenuClicked);
        }

        public void SetResult(bool victory, int score)
        {
            _isVictory = victory;

            if (_titleText != null)
            {
                _titleText.text = victory ? "Victory!" : "Game Over";
                _titleText.color = victory ? new Color(0.4f, 1f, 0.4f) : new Color(1f, 0.4f, 0.4f);
            }

            if (_scoreText != null)
                _scoreText.text = $"Score: {score}";

            if (_restartButton != null)
                _restartButton.gameObject.SetActive(!victory);

            if (_nextLevelButton != null)
                _nextLevelButton.gameObject.SetActive(victory);
        }

        private void OnRestartClicked() => _eventBus?.Send(new RestartLevelSignal());
        private void OnNextLevelClicked() => _eventBus?.Send(new NextLevelSignal());
        private void OnMenuClicked() => _eventBus?.Send(new ReturnToMenuSignal());
    }
}
