using UnityEngine;
using Strada.Core.Sync;
using BoardDefence.Components;

namespace BoardDefence.Views
{
    public class ProjectileView : EntityView<GridPositionComponent>
    {
        [SerializeField] private TrailRenderer _trail;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_trail == null)
                _trail = GetComponentInChildren<TrailRenderer>();
        }
#endif

        protected override void OnBind()
        {
            base.OnBind();

            var pos = GetComponent<GridPositionComponent>();
            transform.position = new Vector3(pos.WorldX, pos.WorldY, pos.WorldZ);

            if (_trail != null)
            {
                _trail.Clear();
            }
        }

        protected override void OnUnbind()
        {
            base.OnUnbind();

            if (_trail != null)
            {
                _trail.Clear();
            }
        }

        protected override void OnComponentChanged(GridPositionComponent pos)
        {
            transform.position = new Vector3(pos.WorldX, pos.WorldY, pos.WorldZ);
        }
    }
}
