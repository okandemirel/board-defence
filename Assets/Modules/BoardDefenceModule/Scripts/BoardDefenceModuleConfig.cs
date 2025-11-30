using UnityEngine;
using Strada.Core.DI;
using Strada.Core.Modules;
using Strada.Core.ECS.World;
using Strada.Core.Bootstrap;
using Strada.Core.Sync;
using BoardDefence.Components;
using BoardDefence.Data;
using BoardDefence.Models;
using BoardDefence.Services;
using BoardDefence.Controllers;
using BoardDefence.Systems;

namespace BoardDefence
{
    [CreateAssetMenu(fileName = "BoardDefenceModuleConfig", menuName = "BoardDefence/Module Config")]
    public class BoardDefenceModuleConfig : ModuleConfig
    {
        [Header("Configuration Data")]
        [SerializeField] private CD_GameConfig _gameConfig;
        [SerializeField] private CD_BoardConfig _boardConfig;

        private PoolManager _poolManager;

        protected override void Configure(IModuleBuilder builder)
        {
            builder
                .RegisterInstance(_gameConfig.Data)
                .RegisterInstance(_boardConfig.Data)
                .RegisterModel<IBoardModel, BoardModel>()
                .RegisterModel<IGameModel, GameModel>()
                .RegisterModel<ILevelModel, LevelModel>()
                .RegisterService<ISpawnService, SpawnService>()
                .RegisterService<ILevelContainerService, LevelContainerService>()
                .RegisterController<GameController>()
                .RegisterController<BoardController>()
                .RegisterController<DragDropController>();
        }

        public override void Initialize(IServiceLocator services)
        {
            var container = services.Get<IContainer>();

            var boardModel = services.Get<IBoardModel>();
            InjectionProcessor.Inject(boardModel, container);
            (boardModel as BoardModel)?.Initialize();

            var gameModel = services.Get<IGameModel>();
            InjectionProcessor.Inject(gameModel, container);
            (gameModel as GameModel)?.Initialize();

            var levelModel = services.Get<ILevelModel>();
            InjectionProcessor.Inject(levelModel, container);
            (levelModel as LevelModel)?.Initialize();

            var spawnService = services.Get<ISpawnService>();
            InjectionProcessor.Inject(spawnService, container);
            (spawnService as SpawnService)?.Initialize();

            var levelContainerService = services.Get<ILevelContainerService>();
            InjectionProcessor.Inject(levelContainerService, container);

            var gameController = services.Get<GameController>();
            InjectionProcessor.Inject(gameController, container);
            gameController?.Initialize();

            var boardController = services.Get<BoardController>();
            InjectionProcessor.Inject(boardController, container);
            boardController?.Initialize();

            var dragDropController = services.Get<DragDropController>();
            InjectionProcessor.Inject(dragDropController, container);
            dragDropController?.Initialize();

            var entityManager = World.Current.EntityManager;

            var configEntity = entityManager.CreateEntity();
            entityManager.AddComponent(configEntity, new BoardConfigComponent
            {
                Columns = _boardConfig.Data.Columns,
                Rows = _boardConfig.Data.Rows,
                PlaceableRowsFromBottom = _boardConfig.Data.PlaceableRowsFromBottom,
                CellSize = _boardConfig.Data.CellSize,
                CellSpacing = _boardConfig.Data.CellSpacing
            });

            _poolManager = new PoolManager(entityManager, container);

            if (GameBootstrapper.Systems != null)
            {
                foreach (var system in GameBootstrapper.Systems.GetAllSystems())
                {
                    if (system is EntityViewSyncSystem viewSync)
                    {
                        viewSync.SetPoolManager(_poolManager);
                        break;
                    }
                }
            }

            (levelContainerService as LevelContainerService)?.Initialize();
        }

        public override void Shutdown()
        {
            _poolManager?.Dispose();
            _poolManager = null;
        }

        public CD_GameConfig GameConfig => _gameConfig;
        public CD_BoardConfig BoardConfig => _boardConfig;
    }
}
