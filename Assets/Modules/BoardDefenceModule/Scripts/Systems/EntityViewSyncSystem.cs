using System;
using System.Collections.Generic;
using Strada.Core.Bootstrap;
using Strada.Core.ECS;
using Strada.Core.ECS.Systems;
using Strada.Core.ECS.World;
using Strada.Core.Modules;
using Strada.Core.Sync;
using BoardDefence.Components;
using BoardDefence.Events;
using BoardDefence.Services;
using BoardDefence.Views;
using UnityEngine;

namespace BoardDefence.Systems
{
    public enum ViewCategory : byte
    {
        None = 0,
        Defence = 1,
        Enemy = 2,
        Projectile = 3
    }

    [StradaSystem(
        Module = "BoardDefence",
        Category = "View",
        Description = "Synchronizes ECS entities with Unity views",
        Phase = UpdatePhase.LateUpdate,
        Order = 900)]
    public class EntityViewSyncSystem : SystemBase
    {
        private ILevelContainerService _levelContainer;
        private PoolManager _poolManager;
        private readonly Dictionary<long, ViewCategory> _entityToViewType = new(64);

        private const int DEFENCE_POOL_SIZE = 20;
        private const int ENEMY_POOL_SIZE = 30;
        private const int PROJECTILE_POOL_SIZE = 50;

        public void SetPoolManager(PoolManager poolManager)
        {
            _poolManager = poolManager;
        }

        protected override void OnInitialize()
        {
            _levelContainer = GameBootstrapper.Services.Get<ILevelContainerService>();

            if (_poolManager != null)
            {
                RegisterPools();
            }

            SubscribeToEvents();
        }

        protected override void OnUpdate(float deltaTime) { }

        private void RegisterPools()
        {
            var defencePrefab = Resources.Load<GameObject>("Prefabs/DefenceItem");
            var enemyPrefab = Resources.Load<GameObject>("Prefabs/Enemy");
            var projectilePrefab = Resources.Load<GameObject>("Prefabs/Projectile");

            if (defencePrefab != null)
                _poolManager.RegisterPool<DefenceItemView>(defencePrefab, DEFENCE_POOL_SIZE);

            if (enemyPrefab != null)
                _poolManager.RegisterPool<EnemyView>(enemyPrefab, ENEMY_POOL_SIZE);

            if (projectilePrefab != null)
                _poolManager.RegisterPool<ProjectileView>(projectilePrefab, PROJECTILE_POOL_SIZE);
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<DefencePlacedEvent>(OnDefencePlaced);
            EventBus.Subscribe<EnemySpawnedEvent>(OnEnemySpawned);
            EventBus.Subscribe<ProjectileSpawnedEvent>(OnProjectileSpawned);
            EventBus.Subscribe<EntityDestroyedEvent>(OnEntityDestroyed);
            RegisterSignalHandler<CleanupLevelSignal>(OnCleanupLevel);
        }

        private void OnDefencePlaced(DefencePlacedEvent evt)
        {
            var entity = HandleRegistry.Resolve(evt.Handle);
            if (entity.IsNull) return;

            var view = _poolManager.Spawn<DefenceItemView>(entity, _levelContainer.Defences);
            if (view != null)
            {
                _entityToViewType[GetEntityKey(entity)] = ViewCategory.Defence;
                view.ForceSyncBindings();
            }
        }

        private void OnEnemySpawned(EnemySpawnedEvent evt)
        {
            var entity = HandleRegistry.Resolve(evt.Handle);
            if (entity.IsNull) return;

            var view = _poolManager.Spawn<EnemyView>(entity, _levelContainer.Enemies);
            if (view != null)
            {
                _entityToViewType[GetEntityKey(entity)] = ViewCategory.Enemy;
                view.ForceSyncBindings();
            }
        }

        private void OnProjectileSpawned(ProjectileSpawnedEvent evt)
        {
            var entity = HandleRegistry.Resolve(evt.Handle);
            if (entity.IsNull) return;

            var view = _poolManager.Spawn<ProjectileView>(entity, _levelContainer.Projectiles);
            if (view != null)
            {
                _entityToViewType[GetEntityKey(entity)] = ViewCategory.Projectile;
                view.ForceSyncBindings();
            }
        }

        private void OnEntityDestroyed(EntityDestroyedEvent evt)
        {
            var entity = HandleRegistry.Resolve(evt.Handle);
            if (entity.IsNull) return;

            var key = GetEntityKey(entity);
            if (!_entityToViewType.TryGetValue(key, out var viewCategory))
                return;

            switch (viewCategory)
            {
                case ViewCategory.Defence:
                    if (EntityManager.HasComponent<GridPositionComponent>(entity))
                    {
                        var pos = EntityManager.GetComponent<GridPositionComponent>(entity);
                        Publish(new DefenceDestroyedEvent
                        {
                            Handle = evt.Handle,
                            Column = pos.Column,
                            Row = pos.Row
                        });
                    }
                    _poolManager.DespawnByEntity<DefenceItemView>(entity);
                    break;

                case ViewCategory.Enemy:
                    _poolManager.DespawnByEntity<EnemyView>(entity);
                    break;

                case ViewCategory.Projectile:
                    _poolManager.DespawnByEntity<ProjectileView>(entity);
                    break;
            }

            _entityToViewType.Remove(key);
            HandleRegistry.Unregister(evt.Handle);
        }

        private void OnCleanupLevel(CleanupLevelSignal signal)
        {
            _poolManager.DespawnAllViews();
            _entityToViewType.Clear();
            HandleRegistry.Clear();
        }

        protected override void OnDispose()
        {
            EventBus.Unsubscribe<DefencePlacedEvent>(OnDefencePlaced);
            EventBus.Unsubscribe<EnemySpawnedEvent>(OnEnemySpawned);
            EventBus.Unsubscribe<ProjectileSpawnedEvent>(OnProjectileSpawned);
            EventBus.Unsubscribe<EntityDestroyedEvent>(OnEntityDestroyed);
            _entityToViewType.Clear();
        }

        private static long GetEntityKey(Entity entity) => ((long)entity.Index << 32) | (uint)entity.Version;
    }
}
