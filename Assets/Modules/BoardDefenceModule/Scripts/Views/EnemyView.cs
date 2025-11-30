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
            _healthBinding = BindComponent<HealthComponent>();
            _healthBinding.OnChanged += OnHealthChanged;

            var pos = GetComponent<GridPositionComponent>();
            transform.position = new Vector3(pos.WorldX, pos.WorldY, pos.WorldZ);
        }

        protected override void OnUnbind()
        {
            base.OnUnbind();
            if (_healthBinding != null)
            {
                _healthBinding.OnChanged -= OnHealthChanged;
            }

            if (_healthBarFill != null)
            {
                _healthBarFill.localScale = Vector3.one;
            }
        }

        protected override void OnComponentChanged(GridPositionComponent pos)
        {
            transform.position = new Vector3(pos.WorldX, pos.WorldY, pos.WorldZ);
        }

        private void OnHealthChanged(HealthComponent health)
        {
            UpdateHealthBar(health.Current, health.Max);
        }

        private void UpdateHealthBar(int current, int max)
        {
            if (_healthBarFill == null) return;

            float ratio = max > 0 ? (float)current / max : 0f;
            _healthBarFill.localScale = new Vector3(ratio, 1f, 1f);
        }
    }
}
