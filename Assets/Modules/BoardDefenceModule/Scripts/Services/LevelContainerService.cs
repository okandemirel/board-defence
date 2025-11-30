using System;
using Strada.Core.Patterns;
using Strada.Core.Communication;
using Strada.Core.DI.Attributes;
using BoardDefence.Events;
using UnityEngine;

namespace BoardDefence.Services
{
    public sealed class LevelContainerService : Service, ILevelContainerService, IDisposable
    {
        [Inject] private EventBus _eventBus;

        private LevelContainer _container;

        public Transform Board => _container?.Board;
        public Transform Enemies => _container?.Enemies;
        public Transform Defences => _container?.Defences;
        public Transform Projectiles => _container?.Projectiles;
        public bool IsActive => _container?.IsActive ?? false;

        public new void Initialize()
        {
            base.Initialize();
            _container = new LevelContainer();
            _eventBus.Subscribe<LevelStartedEvent>(OnLevelStarted);
            _eventBus.RegisterSignalHandler<CleanupLevelSignal>(OnCleanupLevel);
        }

        private void OnLevelStarted(LevelStartedEvent evt)
        {
            _container.Create(evt.LevelIndex);
        }

        private void OnCleanupLevel(CleanupLevelSignal signal)
        {
            _container.Dispose();
        }

        public void Dispose()
        {
            _eventBus?.Unsubscribe<LevelStartedEvent>(OnLevelStarted);
            _container?.Dispose();
        }
    }
}
