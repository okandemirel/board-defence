using UnityEngine;
using Strada.Core.DI.Attributes;
using Strada.Core.Communication;
using Strada.Core.Patterns.Interfaces;
using BoardDefence.Events;
using BoardDefence.Models;
using BoardDefence.Views;

namespace BoardDefence.Controllers
{
    public class DragDropController : IInitializable
    {
        [Inject] private IBoardModel _boardModel;
        [Inject] private EventBus _eventBus;

        private PlacementPreviewView _previewView;
        private Camera _mainCamera;
        private string _currentDefenceKey;
        private bool _isDragging;
        private int _lastValidRow = -1;
        private int _lastValidColumn = -1;

        public void SetPreviewView(PlacementPreviewView previewView)
        {
            _previewView = previewView;
        }

        public void Initialize()
        {
            _mainCamera = Camera.main;

            _eventBus.Subscribe<DragStartedEvent>(OnDragStarted);
            _eventBus.Subscribe<DragUpdatedEvent>(OnDragUpdated);
            _eventBus.Subscribe<DragEndedEvent>(OnDragEnded);
        }

        private void OnDragStarted(DragStartedEvent evt)
        {
            _currentDefenceKey = evt.DefenceKey;
            _isDragging = true;
            _lastValidRow = -1;
            _lastValidColumn = -1;

            _previewView?.Show();
            _previewView?.SetDefenceType(_currentDefenceKey);

            _eventBus.Publish(new PlacementValidEvent { IsValid = false });

            UpdatePreviewPosition(evt.ScreenPosition);
        }

        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            if (!_isDragging) return;
            UpdatePreviewPosition(evt.ScreenPosition);
        }

        private void OnDragEnded(DragEndedEvent evt)
        {
            if (!_isDragging) return;

            _isDragging = false;
            _previewView?.Hide();

            if (_lastValidRow >= 0 && _lastValidColumn >= 0)
            {
                _eventBus.Send(new PlaceDefenceSignal
                {
                    DefenceKey = _currentDefenceKey,
                    Row = _lastValidRow,
                    Column = _lastValidColumn
                });
            }

            _currentDefenceKey = null;
            _lastValidRow = -1;
            _lastValidColumn = -1;

            _eventBus.Publish(new PlacementValidEvent { IsValid = false, Row = -1, Column = -1 });
        }

        private void UpdatePreviewPosition(Vector2 screenPosition)
        {
            var worldPos = ScreenToWorldPosition(screenPosition);
            if (!worldPos.HasValue)
            {
                _previewView?.SetValid(false);
                _lastValidRow = -1;
                _lastValidColumn = -1;
                return;
            }

            var gridPos = WorldToGridPosition(worldPos.Value);
            int row = gridPos.row;
            int column = gridPos.column;

            bool isValid = IsValidPlacement(row, column);

            _previewView?.SetPosition(GridToWorldPosition(row, column));
            _previewView?.SetValid(isValid);

            if (isValid)
            {
                _lastValidRow = row;
                _lastValidColumn = column;
            }
            else
            {
                _lastValidRow = -1;
                _lastValidColumn = -1;
            }

            _eventBus.Publish(new PlacementValidEvent
            {
                Row = row,
                Column = column,
                IsValid = isValid
            });
        }

        private Vector3? ScreenToWorldPosition(Vector2 screenPosition)
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;

            var ray = _mainCamera.ScreenPointToRay(screenPosition);
            var plane = new Plane(Vector3.up, Vector3.zero);

            if (plane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return null;
        }

        private (int row, int column) WorldToGridPosition(Vector3 worldPosition)
        {
            float cellSize = _boardModel.CellSize;
            int column = Mathf.FloorToInt(worldPosition.x / cellSize + 0.5f);
            int row = Mathf.FloorToInt(worldPosition.z / cellSize + 0.5f);
            return (row, column);
        }

        private Vector3 GridToWorldPosition(int row, int column)
        {
            float cellSize = _boardModel.CellSize;
            return new Vector3(column * cellSize, 0.1f, row * cellSize);
        }

        private bool IsValidPlacement(int row, int column)
        {
            if (row < 0 || row >= _boardModel.PlaceableRowCount)
                return false;

            if (column < 0 || column >= _boardModel.Columns)
                return false;

            return !_boardModel.HasDefence(column, row);
        }
    }
}
