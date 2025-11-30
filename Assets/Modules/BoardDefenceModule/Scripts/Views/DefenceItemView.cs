using UnityEngine;
using Strada.Core.Sync;
using BoardDefence.Components;

namespace BoardDefence.Views
{
    public class DefenceItemView : EntityView<GridPositionComponent>
    {
        [SerializeField] private Transform _model;
        [SerializeField] private Transform _attackPoint;

        private ComponentBinding<AttackStatsComponent> _statsBinding;

        public Transform AttackPoint => _attackPoint != null ? _attackPoint : transform;

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
            _statsBinding = BindComponent<AttackStatsComponent>();

            var pos = GetComponent<GridPositionComponent>();
            transform.position = new Vector3(pos.WorldX, pos.WorldY, pos.WorldZ);

            if (_model != null)
            {
                _model.localRotation = Quaternion.identity;
            }
        }

        protected override void OnUnbind()
        {
            base.OnUnbind();

            if (_model != null)
            {
                _model.localRotation = Quaternion.identity;
            }
        }

        protected override void OnComponentChanged(GridPositionComponent pos)
        {
            transform.position = new Vector3(pos.WorldX, pos.WorldY, pos.WorldZ);
        }

        public void LookAtTarget(Vector3 targetPosition)
        {
            if (_model == null) return;

            var direction = targetPosition - transform.position;
            direction.y = 0;

            if (direction.sqrMagnitude > 0.01f)
            {
                _model.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}
