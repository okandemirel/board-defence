using UnityEngine;
using Strada.Core.Bootstrap;
using Strada.Core.Communication;
using BoardDefence.Views;
using BoardDefence.Events;
using BoardDefence.Models;
using BoardDefence.Services;
using BoardDefence.Controllers;

namespace BoardDefence
{
    public class BoardDefenceBootstrap : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField] private BoardView _boardView;
        [SerializeField] private PlacementPreviewView _placementPreviewView;

        [Header("Prefabs")]
        [SerializeField] private GameObject _placementPreviewPrefab;

        private GameBootstrapper _bootstrapper;
        private SpawnService _spawnService;
        private DragDropController _dragDropController;

        private void Awake()
        {
            _bootstrapper = FindFirstObjectByType<GameBootstrapper>();
            if (_bootstrapper != null)
            {
                _bootstrapper.OnInitializationComplete += OnBootstrapComplete;
            }
        }

        private void OnBootstrapComplete()
        {
            SetupPlacementPreview();
            SetupBoardView();
            SetupDragDrop();
            SubscribeToEvents();
            CacheServices();

            var eventBus = GameBootstrapper.Services.Get<EventBus>();
            eventBus.Send(new StartGameSignal());
        }

        private void SetupPlacementPreview()
        {
            if (_placementPreviewView == null && _placementPreviewPrefab != null)
            {
                var previewGO = Instantiate(_placementPreviewPrefab);
                _placementPreviewView = previewGO.GetComponent<PlacementPreviewView>();
            }
        }

        private void SetupBoardView()
        {
            var services = GameBootstrapper.Services;
            var eventBus = services.Get<EventBus>();
            var boardModel = services.Get<IBoardModel>();

            _boardView?.Inject(boardModel, eventBus);
            _boardView?.Initialize();
        }

        private void SetupDragDrop()
        {
            _dragDropController = GameBootstrapper.Services.Get<DragDropController>();
            if (_dragDropController != null && _placementPreviewView != null)
            {
                _dragDropController.SetPreviewView(_placementPreviewView);
            }
        }

        private void SubscribeToEvents()
        {
            var eventBus = GameBootstrapper.Services.Get<EventBus>();
            eventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void CacheServices()
        {
            _spawnService = GameBootstrapper.Services.Get<ISpawnService>() as SpawnService;
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            switch (evt.NewState)
            {
                case GameState.Menu:
                    _boardView?.Hide();
                    break;
                case GameState.Playing:
                    _boardView?.Show();
                    break;
            }
        }

        private void Update()
        {
            _spawnService?.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            if (_bootstrapper != null)
            {
                _bootstrapper.OnInitializationComplete -= OnBootstrapComplete;
            }
        }
    }
}
