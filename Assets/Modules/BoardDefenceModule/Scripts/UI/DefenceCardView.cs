using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using BoardDefence.Data;
using BoardDefence.Events;
using Strada.Core.Communication;

namespace BoardDefence.UI
{
    public class DefenceCardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _cardBackground;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _countText;
        [SerializeField] private CanvasGroup _canvasGroup;

        private RectTransform _rectTransform;
        private Canvas _canvas;
        private Vector3 _originalPosition;
        private Transform _originalParent;
        private string _defenceKey;
        private int _remainingCount;
        private EventBus _eventBus;
        private bool _isDragging;
        private Color _cardColor;

        public string DefenceKey => _defenceKey;
        public bool HasRemaining => _remainingCount > 0;

        public void Initialize(string key, DefenceItemData data, int count, EventBus eventBus)
        {
            _defenceKey = key;
            _remainingCount = count;
            _eventBus = eventBus;
            _cardColor = data.CardColor;

            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();

            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (_nameText != null)
                _nameText.text = key;

            UpdateCountDisplay();
        }

        public void SetCount(int count)
        {
            _remainingCount = count;
            UpdateCountDisplay();
        }

        public void DecrementCount()
        {
            _remainingCount = Mathf.Max(0, _remainingCount - 1);
            UpdateCountDisplay();
        }

        private void UpdateCountDisplay()
        {
            if (_countText != null)
                _countText.text = _remainingCount.ToString();

            if (_cardBackground != null)
            {
                if (_remainingCount > 0)
                    _cardBackground.color = _cardColor;
                else
                    _cardBackground.color = new Color(_cardColor.r * 0.5f, _cardColor.g * 0.5f, _cardColor.b * 0.5f, 0.7f);
            }

            if (_canvasGroup != null)
                _canvasGroup.alpha = _remainingCount > 0 ? 1f : 0.5f;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_remainingCount <= 0) return;
            _originalPosition = _rectTransform.position;
            _originalParent = transform.parent;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_remainingCount <= 0)
            {
                eventData.pointerDrag = null;
                return;
            }

            _isDragging = true;
            _canvasGroup.blocksRaycasts = false;

            transform.SetParent(_canvas.transform);
            transform.SetAsLastSibling();

            _eventBus?.Publish(new DragStartedEvent
            {
                DefenceKey = _defenceKey,
                ScreenPosition = eventData.position
            });
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;

            _rectTransform.position = eventData.position;

            _eventBus?.Publish(new DragUpdatedEvent
            {
                DefenceKey = _defenceKey,
                ScreenPosition = eventData.position
            });
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;

            _isDragging = false;
            _canvasGroup.blocksRaycasts = true;

            transform.SetParent(_originalParent);
            _rectTransform.position = _originalPosition;

            _eventBus?.Publish(new DragEndedEvent
            {
                DefenceKey = _defenceKey,
                ScreenPosition = eventData.position
            });
        }

        public void SetIcon(Sprite sprite)
        {
            if (_icon != null && sprite != null)
                _icon.sprite = sprite;
        }
    }
}
