using Strada.Core.Patterns;
using Strada.Core.Sync;
using Strada.Core.Communication;
using Strada.Core.DI.Attributes;
using BoardDefence.Events;

namespace BoardDefence.Models
{
    public class GameModel : Model, IGameModel
    {
        [Inject] private EventBus _eventBus;

        private ReactiveProperty<GameState> _state;
        private ReactiveProperty<int> _score;
        private ReactiveProperty<int> _baseHealth;
        private int _maxBaseHealth = 10;

        public IReadOnlyReactiveProperty<GameState> State => _state;
        public IReadOnlyReactiveProperty<int> Score => _score;
        public IReadOnlyReactiveProperty<int> BaseHealth => _baseHealth;
        public int MaxBaseHealth => _maxBaseHealth;

        protected override void OnInitialize()
        {
            _state = CreateProperty(GameState.Menu);
            _score = CreateProperty(0);
            _baseHealth = CreateProperty(_maxBaseHealth);
        }

        public void SetState(GameState state)
        {
            var previousState = _state.Value;
            _state.Value = state;

            _eventBus?.Publish(new GameStateChangedEvent
            {
                PreviousState = previousState,
                NewState = state
            });
        }

        public void AddScore(int amount)
        {
            _score.Value += amount;
        }

        public void TakeDamage(int amount)
        {
            _baseHealth.Value = System.Math.Max(0, _baseHealth.Value - amount);
        }

        public void Reset()
        {
            _score.Value = 0;
            _baseHealth.Value = _maxBaseHealth;
        }

        public void SetMaxBaseHealth(int maxHealth)
        {
            _maxBaseHealth = maxHealth;
            _baseHealth.Value = maxHealth;
        }
    }
}
