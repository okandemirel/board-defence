using UnityEngine;
using Strada.Core.Patterns;
using Strada.Core.Communication;
using BoardDefence.Models;
using BoardDefence.Events;

namespace BoardDefence.Views
{
    public class BoardView : View
    {
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private Transform _cellsContainer;
        [SerializeField] private Color _dragHighlightColor = new Color(0.3f, 0.8f, 0.3f, 0.5f);

        private IBoardModel _boardModel;
        private EventBus _eventBus;
        private CellView[,] _cells;
        private bool _isDragActive;

        public void Inject(IBoardModel boardModel, EventBus eventBus)
        {
            _boardModel = boardModel;
            _eventBus = eventBus;
        }

        public override void Initialize()
        {
            base.Initialize();
            SubscribeToDragEvents();
        }

        private void SubscribeToDragEvents()
        {
            if (_eventBus == null) return;

            _eventBus.Subscribe<DragStartedEvent>(OnDragStarted);
            _eventBus.Subscribe<DragEndedEvent>(OnDragEnded);
            _eventBus.Subscribe<PlacementValidEvent>(OnPlacementValid);
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
            if (_cells == null)
            {
                CreateGrid();
            }
        }

        private void CreateGrid()
        {
            if (_boardModel == null || _cellPrefab == null) return;

            _cells = new CellView[_boardModel.Columns, _boardModel.Rows];
            float cellSize = _boardModel.CellSize;

            var container = _cellsContainer != null ? _cellsContainer : transform;

            for (int row = 0; row < _boardModel.Rows; row++)
            {
                for (int col = 0; col < _boardModel.Columns; col++)
                {
                    var cellGO = Instantiate(_cellPrefab, container);
                    cellGO.transform.localPosition = new Vector3(col * cellSize, 0f, row * cellSize);

                    var cellView = cellGO.GetComponent<CellView>();
                    if (cellView != null)
                    {
                        cellView.Setup(col, row, row < _boardModel.PlaceableRowCount); // Bottom rows are placeable
                        cellView.OnCellClicked += HandleCellClicked;
                    }

                    _cells[col, row] = cellView;
                }
            }
        }

        private void HandleCellClicked(int column, int row)
        {
            _eventBus?.Publish(new CellClickedEvent { Column = column, Row = row });
        }

        public void HighlightPlaceableCells(bool highlight)
        {
            if (_cells == null || _boardModel == null) return;

            for (int row = 0; row < _boardModel.PlaceableRowCount; row++) // Bottom rows are placeable
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
    }
}
