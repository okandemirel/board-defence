using Strada.Core.Patterns;
using Strada.Core.DI.Attributes;
using Strada.Modules.Screen;
using BoardDefence.Events;
using BoardDefence.UI.Screens;

namespace BoardDefence.UI.Controllers
{
    public class UIController : Controller
    {
        [Inject] private IScreenService _screenService;

        private const int LAYER_MAIN = 0;
        private const int LAYER_POPUP = 1;

        protected override void OnInitialize()
        {
            Subscribe<GameStateChangedEvent>(OnGameStateChanged);
            Subscribe<GameOverEvent>(OnGameOver);
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            switch (evt.NewState)
            {
                case GameState.Menu:
                    ShowMainMenuAsync();
                    break;
                case GameState.Playing:
                    ShowGameplayAsync();
                    break;
            }
        }

        private async void ShowMainMenuAsync()
        {
            await _screenService.HideLayerAsync(LAYER_MAIN);
            await _screenService.HideLayerAsync(LAYER_POPUP);
            await _screenService.Open<MainMenuScreen>()
                .SetLayer(LAYER_MAIN)
                .ShowAsync();
        }

        private async void ShowGameplayAsync()
        {
            await _screenService.HideLayerAsync(LAYER_MAIN);
            await _screenService.HideLayerAsync(LAYER_POPUP);
            await _screenService.Open<GameHUDScreen>()
                .SetLayer(LAYER_MAIN)
                .ShowAsync();
        }

        private async void OnGameOver(GameOverEvent evt)
        {
            await _screenService.Open<GameOverScreen>()
                .SetLayer(LAYER_POPUP)
                .SetParameters(evt.Victory, evt.FinalScore)
                .ShowAsync();
        }
    }
}
