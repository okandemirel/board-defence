using Strada.Core.Patterns;
using Strada.Core.DI.Attributes;
using BoardDefence.Signals;
using BoardDefence.Models;
using BoardDefence.Events;

namespace BoardDefence.Controllers
{
    public class BoardController : Controller
    {
        [Inject] private IBoardModel _boardModel;
        [Inject] private ILevelModel _levelModel;
        [Inject] private IGameModel _gameModel;

        private int _selectedDefenceIndex = -1;

        protected override void OnInitialize()
        {
            Subscribe<CellClickedEvent>(OnCellClicked);
            Subscribe<DefencePlacedEvent>(OnDefencePlaced);
            RegisterSignalHandler<SelectDefenceSignal>(OnSelectDefence);
            RegisterSignalHandler<PlaceDefenceSignal>(OnPlaceDefence);
        }

        private void OnSelectDefence(SelectDefenceSignal signal)
        {
            if (_gameModel.State.Value != GameState.Playing) return;

            int available = _levelModel.GetRemainingCount(signal.DefenceIndex);
            if (available > 0)
            {
                _selectedDefenceIndex = signal.DefenceIndex;
                Publish(new DefenceSelectedEvent { DefenceIndex = signal.DefenceIndex });
            }
        }

        private void OnPlaceDefence(PlaceDefenceSignal signal)
        {
            if (_gameModel.State.Value != GameState.Playing) return;
            if (!_boardModel.CanPlace(signal.Column, signal.Row)) return;

            int slotIndex = GetSlotIndexForDefenceKey(signal.DefenceKey);
            if (slotIndex < 0) return;

            int available = _levelModel.GetRemainingCount(slotIndex);
            if (available <= 0) return;

            _levelModel.ConsumeDefence(slotIndex);

            Send(new SpawnDefenceSignal
            {
                DefenceKey = signal.DefenceKey,
                Column = signal.Column,
                Row = signal.Row
            });
        }

        private void OnCellClicked(CellClickedEvent evt)
        {
            if (_gameModel.State.Value != GameState.Playing) return;
            if (_selectedDefenceIndex < 0) return;
            if (!_boardModel.CanPlace(evt.Column, evt.Row)) return;

            int available = _levelModel.GetRemainingCount(_selectedDefenceIndex);
            if (available <= 0)
            {
                _selectedDefenceIndex = -1;
                return;
            }

            var defenceKey = _levelModel.GetDefenceKey(_selectedDefenceIndex);
            if (string.IsNullOrEmpty(defenceKey)) return;

            _levelModel.ConsumeDefence(_selectedDefenceIndex);

            Send(new SpawnDefenceSignal
            {
                DefenceKey = defenceKey,
                Column = evt.Column,
                Row = evt.Row
            });

            if (_levelModel.GetRemainingCount(_selectedDefenceIndex) <= 0)
            {
                _selectedDefenceIndex = -1;
            }
        }

        private void OnDefencePlaced(DefencePlacedEvent evt)
        {
            _boardModel.Place(evt.Column, evt.Row, evt.Handle);
        }

        private int GetSlotIndexForDefenceKey(string defenceKey)
        {
            var level = _levelModel.CurrentLevel.Value;
            if (level == null) return -1;

            for (int i = 0; i < level.AvailableDefences.Count; i++)
            {
                if (level.AvailableDefences[i].DefenceKey == defenceKey)
                    return i;
            }
            return -1;
        }
    }
}
