using System.Collections;
using UnityEngine;
using Strada.Core.Sync;
using BoardDefence.Components;

namespace BoardDefence.Views
{
    public class EnemyView : EntityView<GridPositionComponent>
    {
        [SerializeField] private Transform _model;
        [SerializeField] private Transform _healthBarPivot;
        [SerializeField] private Transform _healthBarFill;

        private ComponentBinding<HealthComponent> _healthBinding;
        private Renderer _modelRenderer;
        private Renderer _healthFillRenderer;
        private MaterialPropertyBlock _healthBarProps;
        private Color _originalColor;
        private int _lastHealth;
        private Coroutine _flashCoroutine;

        private static readonly Color DamageFlashColor = new Color(1f, 0.3f, 0.3f);
        private static readonly Color HealthBarColor = new Color(0.2f, 0.8f, 0.2f);
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
        private const float FlashDuration = 0.1f;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_model == null && transform.childCount > 0)
                _model = transform.GetChild(0);
        }
#endif

        protected override void OnBind()
        {
            base.OnBind();

            if (_model != null)
            {
                _modelRenderer = _model.GetComponent<Renderer>();
                if (_modelRenderer != null)
                    _originalColor = _modelRenderer.material.color;
            }

            if (_healthBarFill != null)
            {
                _healthFillRenderer = _healthBarFill.GetComponent<Renderer>();
                if (_healthFillRenderer != null && _healthFillRenderer != _modelRenderer)
                {
                    _healthBarProps = new MaterialPropertyBlock();
                    _healthBarProps.SetColor(ColorPropertyId, HealthBarColor);
                    _healthFillRenderer.SetPropertyBlock(_healthBarProps);
                }
            }

            _healthBinding = BindComponent<HealthComponent>();
            _healthBinding.OnChanged += OnHealthChanged;

            var health = GetComponent<HealthComponent>();
            _lastHealth = health.Current;

            var pos = GetComponent<GridPositionComponent>();
            transform.position = new Vector3(pos.WorldX, pos.WorldY, pos.WorldZ);
        }

        protected override void OnUnbind()
        {
            base.OnUnbind();

            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
                _flashCoroutine = null;
            }

            if (_modelRenderer != null)
                _modelRenderer.material.color = _originalColor;

            if (_healthBinding != null)
                _healthBinding.OnChanged -= OnHealthChanged;

            if (_healthBarFill != null)
                _healthBarFill.localScale = Vector3.one;
        }

        protected override void OnComponentChanged(GridPositionComponent pos)
        {
            transform.position = new Vector3(pos.WorldX, pos.WorldY, pos.WorldZ);
        }

        private void OnHealthChanged(HealthComponent health)
        {
            UpdateHealthBar(health.Current, health.Max);

            if (health.Current < _lastHealth)
                TriggerDamageFlash();

            _lastHealth = health.Current;
        }

        private void UpdateHealthBar(int current, int max)
        {
            if (_healthBarFill == null) return;

            float ratio = max > 0 ? (float)current / max : 0f;
            _healthBarFill.localScale = new Vector3(ratio, 1f, 1f);
        }

        private void TriggerDamageFlash()
        {
            if (_modelRenderer == null) return;

            if (_flashCoroutine != null)
                StopCoroutine(_flashCoroutine);

            _flashCoroutine = StartCoroutine(FlashCoroutine());
        }

        private IEnumerator FlashCoroutine()
        {
            _modelRenderer.material.color = DamageFlashColor;
            yield return new WaitForSeconds(FlashDuration);
            _modelRenderer.material.color = _originalColor;
            _flashCoroutine = null;
        }
    }
}
