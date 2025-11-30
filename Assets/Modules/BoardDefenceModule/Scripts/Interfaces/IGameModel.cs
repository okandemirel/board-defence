using Strada.Core.Patterns.Interfaces;
using Strada.Core.Sync;
using BoardDefence.Events;

namespace BoardDefence.Models
{
    public interface IGameModel : IModel
    {
        IReadOnlyReactiveProperty<GameState> State { get; }
        IReadOnlyReactiveProperty<int> Score { get; }
        IReadOnlyReactiveProperty<int> BaseHealth { get; }
        int MaxBaseHealth { get; }

        void SetState(GameState state);
        void AddScore(int amount);
        void TakeDamage(int amount);
        void Reset();
    }
}
