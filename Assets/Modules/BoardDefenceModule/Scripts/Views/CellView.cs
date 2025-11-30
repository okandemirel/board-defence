using System;
using UnityEngine;
using Strada.Core.Patterns;

namespace BoardDefence.Views
{
    public class CellView : View
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Color _normalColor = Color.gray;
        [SerializeField] private Color _placeableColor = new Color(0.4f, 0.6f, 0.4f, 1f);
        [SerializeField] private Color _highlightColor = Color.yellow;
        [SerializeField] private Color _dragHighlightColor = new Color(0.3f, 0.8f, 0.3f, 0.8f);
        [SerializeField] private Color _hoverHighlightColor = new Color(0.2f, 1f, 0.2f, 1f);

        public event Action<int, int> OnCellClicked;

        private int _column;
        private int _row;
        private bool _isPlaceable;
        private bool _isHighlighted;
        private bool _isDragHighlighted;
        private bool _isHoverHighlighted;
        private MaterialPropertyBlock _propertyBlock;

        public int Column => _column;
        public int Row => _row;
        public bool IsPlaceable => _isPlaceable;

        public void Setup(int column, int row, bool isPlaceable)
        {
            _column = column;
            _row = row;
            _isPlaceable = isPlaceable;
            _propertyBlock = new MaterialPropertyBlock();

            UpdateVisual();
        }

        public void SetHighlight(bool highlight)
        {
            _isHighlighted = highlight;
            UpdateVisual();
        }

        public void SetDragHighlight(bool highlight)
        {
            _isDragHighlighted = highlight;
            UpdateVisual();
        }

        public void SetHoverHighlight(bool highlight)
        {
            _isHoverHighlighted = highlight;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (_renderer == null) return;

            Color color;
            if (_isHoverHighlighted)
            {
                color = _hoverHighlightColor;
            }
            else if (_isDragHighlighted)
            {
                color = _dragHighlightColor;
            }
            else if (_isHighlighted)
            {
                color = _highlightColor;
            }
            else if (_isPlaceable)
            {
                color = _placeableColor;
            }
            else
            {
                color = _normalColor;
            }

            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor("_Color", color);
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        private void OnMouseDown()
        {
            if (_isPlaceable)
            {
                OnCellClicked?.Invoke(_column, _row);
            }
        }
    }
}
