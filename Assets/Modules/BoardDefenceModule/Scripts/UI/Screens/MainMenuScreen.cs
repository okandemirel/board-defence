using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Strada.Modules.Screen;
using Strada.Core.Communication;
using Strada.Core.DI.Attributes;
using BoardDefence.Data;
using BoardDefence.Events;

namespace BoardDefence.UI.Screens
{
    public class MainMenuScreen : ScreenView
    {
        [SerializeField] private Transform _levelButtonsContainer;
        [SerializeField] private GameObject _levelButtonPrefab;
        [SerializeField] private TextMeshProUGUI _titleText;

        [Inject] private EventBus _eventBus;
        [Inject] private GameConfigData _gameConfig;

        private bool _buttonsCreated;

        protected override void OnScreenShown()
        {
            if (!_buttonsCreated)
            {
                CreateLevelButtons();
                _buttonsCreated = true;
            }
        }

        private void CreateLevelButtons()
        {
            if (_levelButtonsContainer == null || _levelButtonPrefab == null) return;
            if (_gameConfig == null) return;

            foreach (Transform child in _levelButtonsContainer)
                Destroy(child.gameObject);

            for (int i = 0; i < _gameConfig.Levels.Count; i++)
            {
                var buttonGO = Instantiate(_levelButtonPrefab, _levelButtonsContainer);
                var button = buttonGO.GetComponent<Button>();
                var text = buttonGO.GetComponentInChildren<TextMeshProUGUI>();

                if (text != null)
                    text.text = $"Level {i + 1}";

                int levelIndex = i;
                button?.onClick.AddListener(() => _eventBus?.Send(new StartLevelSignal { LevelIndex = levelIndex }));
            }
        }
    }
}
