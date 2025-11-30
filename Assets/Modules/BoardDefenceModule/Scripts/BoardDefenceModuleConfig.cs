using UnityEngine;
using Strada.Core.DI;
using Strada.Core.Modules;
using Strada.Core.Sync;
using Strada.Core.ECS.World;
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
                .Register<EntityHandleRegistry>()
                .RegisterModel<IBoardModel, BoardModel>()
                .RegisterModel<IGameModel, GameModel>()
                .RegisterModel<ILevelModel, LevelModel>()
                .RegisterService<ISpawnService, SpawnService>()
                .RegisterController<GameController>()
                .RegisterController<BoardController>()
                .RegisterController<DragDropController>()
                .Register<DefenceSpawnSystem>()
                .Register<EnemySpawnSystem>()
                .Register<ProjectileSpawnSystem>()
                .Register<EntityViewSyncSystem>();
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

            var gameController = services.Get<GameController>();
            InjectionProcessor.Inject(gameController, container);
            gameController?.Initialize();

            var boardController = services.Get<BoardController>();
            InjectionProcessor.Inject(boardController, container);
            boardController?.Initialize();

            var dragDropController = services.Get<DragDropController>();
            InjectionProcessor.Inject(dragDropController, container);
            dragDropController?.Initialize();

            var defenceSpawnSystem = services.Get<DefenceSpawnSystem>();
            InjectionProcessor.Inject(defenceSpawnSystem, container);
            defenceSpawnSystem?.Initialize();

            var enemySpawnSystem = services.Get<EnemySpawnSystem>();
            InjectionProcessor.Inject(enemySpawnSystem, container);
            enemySpawnSystem?.Initialize();

            var projectileSpawnSystem = services.Get<ProjectileSpawnSystem>();
            InjectionProcessor.Inject(projectileSpawnSystem, container);
            projectileSpawnSystem?.Initialize();

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

            var viewSyncSystem = services.Get<EntityViewSyncSystem>();
            InjectionProcessor.Inject(viewSyncSystem, container);
            viewSyncSystem?.Initialize(_poolManager);
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
