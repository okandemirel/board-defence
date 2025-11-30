using UnityEngine;
using Strada.Core.Patterns;
using Strada.Core.Communication;
using BoardDefence.Models;
using BoardDefence.Events;
using BoardDefence.Services;

namespace BoardDefence.Views
{
    public class BoardView : View
    {
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private Color _dragHighlightColor = new Color(0.3f, 0.8f, 0.3f, 0.5f);

        private IBoardModel _boardModel;
        private EventBus _eventBus;
        private ILevelContainerService _levelContainer;
        private CellView[,] _cells;
        private bool _isDragActive;

        public void Inject(IBoardModel boardModel, EventBus eventBus, ILevelContainerService levelContainer)
        {
            _boardModel = boardModel;
            _eventBus = eventBus;
            _levelContainer = levelContainer;
        }

        public override void Initialize()
        {
            base.Initialize();
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            if (_eventBus == null) return;

            _eventBus.Subscribe<DragStartedEvent>(OnDragStarted);
            _eventBus.Subscribe<DragEndedEvent>(OnDragEnded);
            _eventBus.Subscribe<PlacementValidEvent>(OnPlacementValid);
            _eventBus.Subscribe<LevelStartedEvent>(OnLevelStarted);
            _eventBus.RegisterSignalHandler<CleanupLevelSignal>(OnCleanupLevel);
        }

        private void OnLevelStarted(LevelStartedEvent evt)
        {
            DestroyGrid();
            CreateGrid();
        }

        private void OnCleanupLevel(CleanupLevelSignal signal)
        {
            DestroyGrid();
        }

        private void OnDragStarted(DragStartedEvent evt)
        {
            _isDragActive = true;
            HighlightAllPlaceableCells(true);
        }

        private void OnDragEnded(DragEndedEvent evt)
        {
            _isDragActive = false;
            HighlightAllPlaceableCells(false);
            ClearHoverHighlight();
        }

        private void OnPlacementValid(PlacementValidEvent evt)
        {
            if (!_isDragActive) return;

            ClearHoverHighlight();

            if (evt.IsValid && evt.Row >= 0 && evt.Column >= 0)
            {
                var cell = GetCell(evt.Column, evt.Row);
                cell?.SetHoverHighlight(true);
            }
        }

        private void HighlightAllPlaceableCells(bool highlight)
        {
            if (_cells == null || _boardModel == null) return;

            for (int row = 0; row < _boardModel.PlaceableRowCount; row++)
            {
                for (int col = 0; col < _boardModel.Columns; col++)
                {
                    bool canPlace = highlight && !_boardModel.HasDefence(col, row);
                    _cells[col, row]?.SetDragHighlight(canPlace);
                }
            }
        }

        private void ClearHoverHighlight()
        {
            if (_cells == null || _boardModel == null) return;

            for (int row = 0; row < _boardModel.PlaceableRowCount; row++)
            {
                for (int col = 0; col < _boardModel.Columns; col++)
                {
                    _cells[col, row]?.SetHoverHighlight(false);
                }
            }
        }

        protected override void OnShow()
        {
        }

        private void CreateGrid()
        {
            if (_boardModel == null || _cellPrefab == null || _levelContainer?.Board == null) return;

            _cells = new CellView[_boardModel.Columns, _boardModel.Rows];
            float cellSize = _boardModel.CellSize;

            for (int row = 0; row < _boardModel.Rows; row++)
            {
                for (int col = 0; col < _boardModel.Columns; col++)
                {
                    var cellGO = Instantiate(_cellPrefab, _levelContainer.Board);
                    cellGO.transform.localPosition = new Vector3(col * cellSize, 0f, row * cellSize);

                    var cellView = cellGO.GetComponent<CellView>();
                    if (cellView != null)
                    {
                        cellView.Setup(col, row, row < _boardModel.PlaceableRowCount);
                        cellView.OnCellClicked += HandleCellClicked;
                    }

                    _cells[col, row] = cellView;
                }
            }
        }

        private void DestroyGrid()
        {
            if (_cells == null || _boardModel == null) return;

            for (int row = 0; row < _boardModel.Rows; row++)
            {
                for (int col = 0; col < _boardModel.Columns; col++)
                {
                    if (_cells[col, row] != null)
                    {
                        _cells[col, row].OnCellClicked -= HandleCellClicked;
                        Destroy(_cells[col, row].gameObject);
                    }
                }
            }
            _cells = null;
        }

        private void HandleCellClicked(int column, int row)
        {
            _eventBus?.Publish(new CellClickedEvent { Column = column, Row = row });
        }

        public void HighlightPlaceableCells(bool highlight)
        {
            if (_cells == null || _boardModel == null) return;

            for (int row = 0; row < _boardModel.PlaceableRowCount; row++)
            {
                for (int col = 0; col < _boardModel.Columns; col++)
                {
                    _cells[col, row]?.SetHighlight(highlight && !_boardModel.HasDefence(col, row));
                }
            }
        }

        public CellView GetCell(int column, int row)
        {
            if (_boardModel == null) return null;
            if (column < 0 || column >= _boardModel.Columns) return null;
            if (row < 0 || row >= _boardModel.Rows) return null;
            return _cells?[column, row];
        }

        protected override void OnHide()
        {
            DestroyGrid();
        }
    }
}
