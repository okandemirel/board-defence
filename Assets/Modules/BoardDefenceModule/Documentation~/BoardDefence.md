# Board Defence Game Documentation

A tower defense game built on the Strada ECS framework. Players place defence units on a grid board to stop waves of enemies from reaching the base.

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Game Flow](#game-flow)
- [ECS Systems](#ecs-systems)
- [Components](#components)
- [Events & Signals](#events--signals)
- [Models](#models)
- [Controllers](#controllers)
- [Services](#services)
- [Views](#views)
- [Data Configuration](#data-configuration)

---

## Architecture Overview

The game follows a layered architecture using the Strada framework:

```
┌─────────────────────────────────────────────────────────────┐
│                        Unity Views                          │
│   BoardView, EnemyView, DefenceItemView, ProjectileView     │
├─────────────────────────────────────────────────────────────┤
│                     View Sync Layer                         │
│              EntityViewSyncSystem + PoolManager             │
├─────────────────────────────────────────────────────────────┤
│                      ECS Systems                            │
│  Spawning → Targeting → Combat → Movement → Lifecycle       │
├─────────────────────────────────────────────────────────────┤
│                    ECS Components                           │
│   Tags, Position, Health, Attack, Movement, Projectile      │
├─────────────────────────────────────────────────────────────┤
│                Controllers & Services                       │
│   GameController, BoardController, SpawnService             │
├─────────────────────────────────────────────────────────────┤
│                       Models                                │
│        GameModel, BoardModel, LevelModel                    │
├─────────────────────────────────────────────────────────────┤
│                   Configuration                             │
│    GameConfigData, BoardData, LevelData, EnemyData          │
└─────────────────────────────────────────────────────────────┘
```

### Key Patterns

1. **SystemBase Pattern**: All ECS systems extend `SystemBase` and access dependencies via:
   - `EntityManager` - Entity and component operations
   - `EventBus` - Event publishing and signal handling
   - `HandleRegistry` - Entity-to-handle mapping for view synchronization
   - `GameBootstrapper.Services` - Access to game configuration data

2. **Entity Handle System**: Entities use `EntityHandle` for stable references between ECS and views. The `EntityHandleRegistry` maps handles to entities using a 64-bit key (Index << 32 | Version).

3. **View Pooling**: Unity GameObjects are pooled via `PoolManager` and `ViewPool<T>` for performance.

---

## Game Flow

### Initialization Sequence

```
GameBootstrapper.Awake()
    │
    ├── Phase 1: Validation
    │
    ├── Phase 2: BuildContainer()
    │       └── Create DI container, EventBus, HandleRegistry
    │       └── Set GameBootstrapper.Services (available for systems)
    │
    ├── Phase 3: CreateWorld()
    │       └── Create ECS World and SystemRunner
    │       └── Add systems from ModuleConfig
    │
    ├── Phase 4: Module Initialization
    │       └── BoardDefenceModuleConfig.Initialize()
    │           ├── Initialize Models (BoardModel, GameModel, LevelModel)
    │           ├── Initialize Services (SpawnService, LevelContainerService)
    │           ├── Initialize Controllers (GameController, BoardController)
    │           ├── Create BoardConfigComponent entity
    │           └── Setup PoolManager for EntityViewSyncSystem
    │
    ├── Phase 5: System Initialization
    │       └── SystemRunner.Initialize()
    │           └── Each system's OnInitialize() called
    │               └── Register signal handlers
    │
    └── CompleteInitialization()
            └── Trigger OnInitializationComplete
                └── BoardDefenceBootstrap.OnBootstrapComplete()
                    └── Send StartGameSignal → GameState.Menu
```

### Gameplay Loop

```
MainMenuScreen
    │
    └── Player clicks level button
            │
            └── Send StartLevelSignal
                    │
                    └── GameController.OnStartLevelSignal()
                            ├── Load level data
                            ├── Reset models
                            ├── Set GameState.Playing
                            └── SpawnService.StartWave()
                                    │
                                    └── [Wave Loop]
                                            │
                                            ├── SpawnService.Tick() (called from BoardDefenceBootstrap.Update)
                                            │       └── Send SpawnEnemySignal periodically
                                            │
                                            └── ECS Update Loop (per frame):
                                                    ├── EnemySpawnSystem → Create enemy entities
                                                    ├── DefenceSpawnSystem → Create defence entities
                                                    ├── AttackCooldownSystem → Update cooldowns
                                                    ├── TargetAcquisitionSystem → Find targets, spawn projectiles
                                                    ├── ProjectileSpawnSystem → Create projectile entities
                                                    ├── ProjectileMovementSystem → Move projectiles, detect hits
                                                    ├── EnemyMovementSystem → Move enemies toward base
                                                    ├── DeathDetectionSystem → Mark dead entities
                                                    ├── DestroySystem → Remove marked entities
                                                    └── EntityViewSyncSystem → Sync ECS ↔ Unity views
```

### Defence Placement Flow

```
Player drags defence card
    │
    └── DragDropController
            ├── OnBeginDrag → Show placement preview
            ├── OnDrag → Update preview position, highlight valid cells
            └── OnEndDrag
                    │
                    └── Valid placement?
                            │
                            ├── Yes → Send PlaceDefenceSignal
                            │           └── BoardController handles placement
                            │                   └── Send SpawnDefenceSignal
                            │                           └── DefenceSpawnSystem creates entity
                            │
                            └── No → Cancel placement
```

---

## ECS Systems

Systems are executed in order by `UpdatePhase` and `Order`:

| System | Phase | Order | Description |
|--------|-------|-------|-------------|
| DefenceSpawnSystem | Update | 50 | Spawns defence entities from signals |
| EnemySpawnSystem | Update | 60 | Spawns enemy entities from signals |
| ProjectileSpawnSystem | Update | 100 | Spawns projectile entities from signals |
| EnemyMovementSystem | Update | 100 | Moves enemies toward the base |
| AttackCooldownSystem | Update | 200 | Updates attack cooldown timers |
| TargetAcquisitionSystem | Update | 300 | Acquires targets and fires projectiles |
| ProjectileMovementSystem | Update | 400 | Moves projectiles and detects hits |
| DeathDetectionSystem | Update | 500 | Marks entities with health <= 0 for destruction |
| DestroySystem | Update | 1000 | Removes entities marked with DestroyTag |
| EntityViewSyncSystem | LateUpdate | 900 | Synchronizes ECS entities with Unity views |

### System Details

#### DefenceSpawnSystem
Listens for `SpawnDefenceSignal` and creates defence entities with:
- `DefenceItemTag`
- `DefenceTypeComponent`
- `GridPositionComponent`
- `AttackStatsComponent`
- `AttackCooldownComponent`

#### EnemySpawnSystem
Listens for `SpawnEnemySignal` and creates enemy entities with:
- `EnemyTag`
- `EnemyTypeComponent`
- `GridPositionComponent`
- `MoveSpeedComponent`
- `HealthComponent`

#### TargetAcquisitionSystem
Each frame for defence entities:
1. Check if cooldown is ready
2. Find nearest enemy in range (based on `AttackStatsComponent.Direction`)
3. Send `SpawnProjectileSignal` with target info
4. Reset cooldown

#### ProjectileMovementSystem
Each frame for projectile entities:
1. Calculate direction to target
2. Move projectile toward target position
3. On hit (distance < threshold):
   - Apply damage to target's `HealthComponent`
   - Publish `EnemyDamagedEvent` and `ProjectileHitEvent`
   - Mark projectile for destruction

#### EntityViewSyncSystem
Subscribes to spawn events and manages view lifecycle:
- `EnemySpawnedEvent` → Spawn `EnemyView`
- `DefencePlacedEvent` → Spawn `DefenceItemView`
- `ProjectileSpawnedEvent` → Spawn `ProjectileView`
- `EntityDestroyedEvent` → Despawn corresponding view

---

## Components

All components implement `IComponent` and are unmanaged structs:

### Tag Components
```csharp
EnemyTag        // Marks entity as an enemy
DefenceItemTag  // Marks entity as a defence unit
ProjectileTag   // Marks entity as a projectile
DestroyTag      // Marks entity for destruction
```

### Data Components
```csharp
GridPositionComponent {
    int Column, Row;
    float WorldX, WorldY, WorldZ;
}

HealthComponent {
    int Current, Max;
}

MoveSpeedComponent {
    float BlocksPerSecond;
}

AttackStatsComponent {
    int Damage;
    float Range;
    AttackDirection Direction;
    float ProjectileSpeed;
}

AttackCooldownComponent {
    float CurrentTime;
    float Interval;
}

ProjectileComponent {
    Entity TargetEntity;
    int Damage;
    float Speed;
    float TargetX, TargetY, TargetZ;
}

EnemyTypeComponent {
    int TypeIndex;
    int Damage;
    int ScoreValue;
}

DefenceTypeComponent {
    int TypeIndex;
}

BoardConfigComponent {
    int Columns, Rows;
    int PlaceableRowsFromBottom;
    float CellSize, CellSpacing;
}
```

---

## Events & Signals

### Events (Broadcast, multiple subscribers)

| Event | Published By | Data |
|-------|-------------|------|
| `EnemySpawnedEvent` | EnemySpawnSystem | Handle, Column, EnemyTypeIndex |
| `EnemyDamagedEvent` | ProjectileMovementSystem | Handle, Damage, RemainingHealth |
| `EnemyKilledEvent` | DeathDetectionSystem | Handle, ScoreValue |
| `EnemyReachedBaseEvent` | EnemyMovementSystem | Handle, Damage |
| `DefencePlacedEvent` | DefenceSpawnSystem | Handle, Column, Row, DefenceTypeIndex |
| `ProjectileSpawnedEvent` | ProjectileSpawnSystem | Handle, TargetHandle |
| `ProjectileHitEvent` | ProjectileMovementSystem | ProjectileHandle, TargetHandle, Damage |
| `EntityDestroyedEvent` | DestroySystem | Handle |
| `GameStateChangedEvent` | GameModel | OldState, NewState |
| `LevelStartedEvent` | GameController | LevelIndex |
| `WaveStartedEvent` | GameController | WaveIndex, TotalWaves |
| `WaveCompletedEvent` | GameController | WaveIndex |
| `GameOverEvent` | GameController | Victory, FinalScore |

### Signals (Direct, single handler)

| Signal | Handler | Purpose |
|--------|---------|---------|
| `SpawnEnemySignal` | EnemySpawnSystem | Spawn enemy entity |
| `SpawnDefenceSignal` | DefenceSpawnSystem | Spawn defence entity |
| `SpawnProjectileSignal` | ProjectileSpawnSystem | Spawn projectile entity |
| `StartGameSignal` | GameController | Initialize game to menu |
| `StartLevelSignal` | GameController | Start specific level |
| `RestartLevelSignal` | GameController | Restart current level |
| `NextLevelSignal` | GameController | Advance to next level |
| `ReturnToMenuSignal` | GameController | Return to main menu |
| `PlaceDefenceSignal` | BoardController | Place defence unit |
| `CleanupLevelSignal` | EntityViewSyncSystem | Clear all entities/views |

---

## Models

Models hold reactive game state using `ReactiveProperty<T>`:

### GameModel
```csharp
ReactiveProperty<GameState> State      // Menu, Playing, Paused, Victory, GameOver
ReactiveProperty<int> Score
ReactiveProperty<int> BaseHealth
ReactiveProperty<int> Currency
```

### BoardModel
```csharp
int Columns, Rows, PlaceableRowCount
float CellSize, CellSpacing
Dictionary<(int, int), int> _defencePositions  // Grid occupancy

bool IsValidPlacement(int col, int row)
bool HasDefence(int col, int row)
void PlaceDefence(int col, int row, int typeIndex)
void RemoveDefence(int col, int row)
```

### LevelModel
```csharp
ReactiveProperty<LevelData> CurrentLevel
ReactiveProperty<int> CurrentWaveIndex
```

---

## Controllers

### GameController
Orchestrates game flow:
- Handles level start/restart/complete
- Manages wave progression
- Tracks enemy counts
- Determines victory/defeat conditions

### BoardController
Handles defence placement:
- Validates placement positions
- Sends spawn signals
- Updates board model

### DragDropController
Manages drag-and-drop for defence placement:
- Tracks drag state
- Shows placement preview
- Validates drop targets

---

## Services

### SpawnService
Manages enemy wave spawning:
- Maintains spawn queue from `WaveData`
- Spawns enemies at configured intervals
- Tracks spawning progress

### LevelContainerService
Provides the parent transform for spawned entities:
- Returns container for enemy/defence/projectile instantiation

---

## Views

All entity views extend `EntityView` base class:

### EntityView Base
```csharp
Entity Entity { get; }
void Bind(Entity entity)
void Unbind()
void OnSpawn()
void OnDespawn()
void UpdatePosition(Vector3 position)
```

### View Types
- **EnemyView**: Visual representation of enemies with health bars
- **DefenceItemView**: Visual representation of defence units
- **ProjectileView**: Visual representation of projectiles
- **BoardView**: Grid visualization with cell highlighting
- **CellView**: Individual grid cell with selection state

---

## Data Configuration

Configuration is defined via ScriptableObjects:

### CD_GameConfig
Contains all game data:
- `DefenceItems`: Dictionary of DefenceItemData by key
- `Enemies`: Dictionary of EnemyData by key
- `Levels`: List of LevelData

### DefenceItemData
```csharp
int Id
string Key
string DisplayName
int Damage
float Range
AttackDirection Direction
float AttackInterval
float ProjectileSpeed
int Cost
GameObject Prefab
GameObject ProjectilePrefab
```

### EnemyData
```csharp
int Id
string Key
string DisplayName
int MaxHealth
float MoveSpeed
int Damage
int ScoreValue
GameObject Prefab
```

### LevelData
```csharp
string LevelName
List<DefenceAllocation> AvailableDefences
List<WaveData> Waves
```

### WaveData
```csharp
float SpawnInterval
List<EnemySpawnEntry> Enemies  // EnemyKey, Count, SpawnColumn
```

---

## Module Configuration

The game module is configured via `BoardDefenceModuleConfig`:

```csharp
[CreateAssetMenu(fileName = "BoardDefenceModuleConfig", menuName = "BoardDefence/Module Config")]
public class BoardDefenceModuleConfig : ModuleConfig
{
    // Serialized configuration
    CD_GameConfig _gameConfig;
    CD_BoardConfig _boardConfig;

    // Registers services, models, controllers
    protected override void Configure(IModuleBuilder builder);

    // Initializes all components after DI container is built
    public override void Initialize(IServiceLocator services);
}
```

### Adding New Systems

1. Create system class extending `SystemBase`
2. Add `[StradaSystem]` attribute with Module, Category, Phase, Order
3. Add system to ModuleConfig via Inspector (use "Discover Systems" button)

```csharp
[StradaSystem(
    Module = "BoardDefence",
    Category = "Combat",
    Description = "Description here",
    Phase = UpdatePhase.Update,
    Order = 100)]
public class MyNewSystem : SystemBase
{
    protected override void OnInitialize()
    {
        // Access config via GameBootstrapper.Services.Get<T>()
        // Register signal handlers
    }

    protected override void OnUpdate(float deltaTime)
    {
        // Query and update components
        ForEach<ComponentA, ComponentB>((int idx, ref ComponentA a, ref ComponentB b) =>
        {
            // Update logic
        });
    }
}
```
