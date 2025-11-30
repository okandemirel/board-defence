using System;
using System.Collections.Generic;
using Strada.Core.Communication;
using Strada.Core.DI.Attributes;
using Strada.Core.ECS.Core;
using Strada.Core.ECS.World;
using Strada.Core.Sync;
using BoardDefence.Events;
using BoardDefence.Views;
using UnityEngine;

namespace BoardDefence.Systems
{
    /// <summary>
    /// Handles view spawning/despawning in response to ECS entity events.
    /// Uses PoolManager for persistent, scene-independent view pooling.
    /// </summary>
    public class EntityViewSyncSystem : IDisposable
    {
        [Inject] private EventBus _eventBus;
        [Inject] private EntityHandleRegistry _handleRegistry;

        private PoolManager _poolManager;
        private readonly Dictionary<EntityHandle, Type> _handleToViewType = new(64);

        private const int DEFENCE_POOL_SIZE = 20;
        private const int ENEMY_POOL_SIZE = 30;
        private const int PROJECTILE_POOL_SIZE = 50;

        public void Initialize(PoolManager poolManager)
        {
            _poolManager = poolManager ?? throw new ArgumentNullException(nameof(poolManager));

            RegisterPools();
            SubscribeToEvents();
        }

        private void RegisterPools()
        {
            var defencePrefab = Resources.Load<GameObject>("Prefabs/DefenceItem");
            var enemyPrefab = Resources.Load<GameObject>("Prefabs/Enemy");
            var projectilePrefab = Resources.Load<GameObject>("Prefabs/Projectile");

            if (defencePrefab == null)
                Debug.LogError("[EntityViewSyncSystem] Failed to load DefenceItem prefab");
            if (enemyPrefab == null)
                Debug.LogError("[EntityViewSyncSystem] Failed to load Enemy prefab");
            if (projectilePrefab == null)
                Debug.LogError("[EntityViewSyncSystem] Failed to load Projectile prefab");

            if (defencePrefab != null)
                _poolManager.RegisterPool<DefenceItemView>(defencePrefab, DEFENCE_POOL_SIZE);

            if (enemyPrefab != null)
                _poolManager.RegisterPool<EnemyView>(enemyPrefab, ENEMY_POOL_SIZE);

            if (projectilePrefab != null)
                _poolManager.RegisterPool<ProjectileView>(projectilePrefab, PROJECTILE_POOL_SIZE);
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<DefencePlacedEvent>(OnDefencePlaced);
            _eventBus.Subscribe<EnemySpawnedEvent>(OnEnemySpawned);
            _eventBus.Subscribe<ProjectileSpawnedEvent>(OnProjectileSpawned);
            _eventBus.Subscribe<EntityDestroyedEvent>(OnEntityDestroyed);
            _eventBus.RegisterSignalHandler<CleanupLevelSignal>(OnCleanupLevel);
        }

        private void OnDefencePlaced(DefencePlacedEvent evt)
        {
            var entity = _handleRegistry.Resolve(evt.Handle);
            if (entity.IsNull) return;

            var view = _poolManager.Spawn<DefenceItemView>(entity);
            if (view != null)
            {
                _handleToViewType[evt.Handle] = typeof(DefenceItemView);
                view.ForceSyncBindings();
            }
        }

        private void OnEnemySpawned(EnemySpawnedEvent evt)
        {
            var entity = _handleRegistry.Resolve(evt.Handle);
            if (entity.IsNull) return;

            var view = _poolManager.Spawn<EnemyView>(entity);
            if (view != null)
            {
                _handleToViewType[evt.Handle] = typeof(EnemyView);
                view.ForceSyncBindings();
            }
        }

        private void OnProjectileSpawned(ProjectileSpawnedEvent evt)
        {
            var entity = _handleRegistry.Resolve(evt.Handle);
            if (entity.IsNull) return;

            var view = _poolManager.Spawn<ProjectileView>(entity);
            if (view != null)
            {
                _handleToViewType[evt.Handle] = typeof(ProjectileView);
                view.ForceSyncBindings();
            }
        }

        private void OnEntityDestroyed(EntityDestroyedEvent evt)
        {
            if (!_handleToViewType.TryGetValue(evt.Handle, out var viewType))
                return;

            var entity = _handleRegistry.Resolve(evt.Handle);
            _handleToViewType.Remove(evt.Handle);
            _handleRegistry.Unregister(evt.Handle);

            if (entity.IsNull) return;

            if (viewType == typeof(DefenceItemView))
                _poolManager.DespawnByEntity<DefenceItemView>(entity);
            else if (viewType == typeof(EnemyView))
                _poolManager.DespawnByEntity<EnemyView>(entity);
            else if (viewType == typeof(ProjectileView))
                _poolManager.DespawnByEntity<ProjectileView>(entity);
        }

        private void OnCleanupLevel(CleanupLevelSignal signal)
        {
            var entityManager = World.Current?.EntityManager;
            if (entityManager == null) return;

            var handlesToCleanup = new List<EntityHandle>(_handleToViewType.Keys);

            foreach (var handle in handlesToCleanup)
            {
                var entity = _handleRegistry.Resolve(handle);
                if (entity.IsNull) continue;

                if (_handleToViewType.TryGetValue(handle, out var viewType))
                {
                    if (viewType == typeof(DefenceItemView))
                        _poolManager.DespawnByEntity<DefenceItemView>(entity);
                    else if (viewType == typeof(EnemyView))
                        _poolManager.DespawnByEntity<EnemyView>(entity);
                    else if (viewType == typeof(ProjectileView))
                        _poolManager.DespawnByEntity<ProjectileView>(entity);
                }

                _handleRegistry.Unregister(handle);
                entityManager.DestroyEntity(entity);
            }

            _handleToViewType.Clear();
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<DefencePlacedEvent>(OnDefencePlaced);
            _eventBus.Unsubscribe<EnemySpawnedEvent>(OnEnemySpawned);
            _eventBus.Unsubscribe<ProjectileSpawnedEvent>(OnProjectileSpawned);
            _eventBus.Unsubscribe<EntityDestroyedEvent>(OnEntityDestroyed);

            _handleToViewType.Clear();
        }
    }
}
