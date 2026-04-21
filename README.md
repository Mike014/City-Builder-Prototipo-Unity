# City Builder — Unity Prototype

## Project Description

City Builder is a management game prototype in active development, built with Unity and C#. The project draws inspiration from the strategic city builder genre — titles like *Anno*, *Frostpunk*, or *Age of Empires* — in which the player builds and manages a city on a grid, balancing economic resources, population, labor, and food supply.

The project is currently in an **early but functional stage**: the core pipeline for building placement, resource management, and camera control is operational. The long-term goal is to expand the system toward more complex simulations, with the possible introduction of autonomous agents (NPC/AI) that populate and animate the city procedurally.

---

## Current State

Implemented and working features include:

- Building placement and demolition on a grid
- Turn-based resource system (money, population, labor, food)
- Camera control with pan, zoom, and rotation
- Tile detection via raycast on the game plane
- UI with real-time statistics updated via Event Bus — `UIManager` reads from `ResourceAmount`, decoupling complete
- Four building types: House, Factory, Farm, Road
- Building placement Undo via Command Pattern (`Z`)
- Building placement Redo (`Ctrl+Y`)
- Bulldoze Undo/Redo via dedicated `BulldozeCommand`
- Operational Event Bus architecture — `City`, `EconomySystem`, `PopulationSystem`, `UIManager` communicate via events without direct coupling
- Building grid based on `Dictionary<Vector3Int, Building>` — O(1) lookup for bulldoze and placement
- Interface-based architecture — `IResourceSource` and `IResourceSink` for producer and consumer buildings
- `FarmBuilding` implements `IResourceSource` — produces food with an internal slot
- `HouseBuilding` implements `IResourceSink` — consumes food, preparatory for the agent system
- Configuration/runtime state separation — `CityConfig` (ScriptableObject) holds initial values, `CityRuntimeState` (POCO) manages mutable runtime state

> **Note on the Road asset:** The Road building is implemented as a 3D plane scaled to `(0.1, 0.1, 0.1)` to fit exactly one grid tile. It does not contribute to population, labor, or food, but is part of the urban expansion logic and has an associated `costPerTurn` like any other structure.

---

## Code Architecture

The project explicitly applies software design patterns, with a strong focus on separating **data** from **behavior**.

### ScriptableObjects as Configuration Containers

A key design choice — distinct from the course's base implementation — is the introduction of ScriptableObjects dedicated exclusively to **immutable configuration**:

**`CameraSettings`** — Contains all camera configuration parameters (movement speed, rotation limits, zoom range, rotation and zoom speed). Separating this data from the MonoBehaviour allows modifying values from the Inspector without touching code, and makes it easy to create multiple camera profiles.

**`CityConfig`** — Contains the city's initial values (`startingMoney`, `startingDay`, `startingPopulation`, `incomePerJobs`). These values are never modified at runtime — they serve only as a template for initializing state.

### Runtime State as POCO

**`CityRuntimeState`** — A plain C# class (not a MonoBehaviour, not a ScriptableObject) that holds the mutable state of the simulation (`money`, `day`, `curPopulation`, `curJobs`, `curFood`, `maxPopulation`, `maxJobs`). Instantiated in `City.Awake()` by copying initial values from `CityConfig`. Being a heap object, it is garbage collected on Play Mode exit — guaranteeing automatic state reset between sessions.

---

### Main Scripts

**`Building.cs`**
Base component attached to every building prefab instantiated in the scene. Holds a reference to its own `BuildingPreset`. Subclasses `FarmBuilding` and `HouseBuilding` extend this class by implementing the `IResourceSource` and `IResourceSink` interfaces.

**`FarmBuilding.cs`**
Subclass of `Building` implementing `IResourceSource`. Maintains an internal `_storedFood` slot initialized from `preset.food` in `Awake()`. `TryProvideResource()` returns available food and reloads the slot after each retrieval.

**`HouseBuilding.cs`**
Subclass of `Building` implementing `IResourceSink`. Receives food via `ReceiveResource()`. The individual per-resident consumption logic is preparatory for the future agent system — currently without direct gameplay impact.

**`IResourceSource.cs`**
Interface defining the contract for producer buildings: `TryProvideResource(out int amount)`. The `TryGet` pattern with `out` handles the case where the resource is unavailable without exceptions or sentinel values.

**`IResourceSink.cs`**
Interface defining the contract for consumer buildings: `ReceiveResource(int amount)`.

**`BuildingPreset.cs`**
ScriptableObject defining the properties of each building type. After refactoring, preset values have been corrected: House `food: 0`, Factory `food: 0`, Road `population: 0`.

**`BuildingPlacement.cs`**
Manages the entire placement and demolition pipeline. Maintains `_undoStack` and `_redoStack`. `PlaceBuilding()` uses a data-driven `PlaceBuildingCommand`. `Bulldoze()` uses a dedicated `BulldozeCommand` with correct semantics — `Execute()` demolishes, `Undo()` reconstructs. Fix applied: `EventSystem.current.IsPointerOverGameObject()` prevents placement in the same frame as a UI click.

**`BulldozeCommand.cs`**
Dedicated Command Pattern implementation for demolition. `Execute()` removes the building from the grid, `Undo()` re-instantiates it — inverse semantics relative to `PlaceBuildingCommand`.

**`CameraController.cs`**
Manages three distinct behaviors separated into private methods: `Zooming()` for mouse wheel scroll with clamping, `Rotating()` for rotation while holding the right mouse button, `Moving()` for WASD movement relative to camera orientation. All parameters are read from `CameraSettings`.

**`City.cs`**
Singleton coordinator of the simulation. In `Awake()` instantiates `CityRuntimeState` by copying initial values from `CityConfig`. Maintains the building grid as a `Dictionary<Vector3Int, Building>` for O(1) lookup. Delegates calculations to `EconomySystem` and `PopulationSystem`, passing `_state` and `_cityConfig` explicitly as parameters (method-level dependency injection), and publishes the updated state to the `EventBus` via `PublishCityState()` — the single publication point to eliminate multiple publishes per turn.

**`CityConfig.cs`**
ScriptableObject defining the city's initial values: `startingMoney`, `startingDay`, `startingPopulation`, `incomePerJobs`. Never modified at runtime.

**`CityRuntimeState.cs`**
POCO class holding the mutable simulation state. Instantiated in memory on each Play session, guaranteeing automatic state reset between sessions without manual intervention.

**`EconomySystem.cs`**
System dedicated to money and labor calculations. The `Calculate()` method receives `IEnumerable<Building>`, `CityRuntimeState`, and `CityConfig` as explicit parameters — no internal state, fully transparent dependencies.

**`PopulationSystem.cs`**
System dedicated to population and food calculations. `CalculateFood()` uses `GetComponent<IResourceSource>()` to collect food from producer buildings. Receives `CityRuntimeState` and `CityConfig` as parameters. Fix applied: gradual famine — population decreases by 1 per turn instead of instantly collapsing to 0.

**`EventBus.cs`**
Static messaging channel implementing the Observer Pattern. Exposes the `OnResourceUpdated` event that systems subscribe to.

**`ResourceAmount.cs`**
C# struct updated with all required fields: `food`, `money`, `jobs`, `population`, `day`, `maxPopulation`, `maxJobs`. Constructed locally and passed to `EventBus.Publish()`.

**`UIManager.cs`**
Subscribes to `EventBus.OnResourceUpdated` in `OnEnable()` and unsubscribes in `OnDisable()`. Reads directly from the `ResourceAmount` parameter — complete decoupling.

**`PlaceBuildingCommand.cs`**
Data-driven Command Pattern implementation. Stores only `BuildingPreset` and `Vector3Int`. `Execute()` instantiates the prefab from scratch, `Undo()` looks up the Dictionary and removes it.

**`Selector.cs`**
Singleton responsible for detecting the tile under the cursor via raycast on a horizontal mathematical plane.

---

## Applied Design Patterns

| Pattern | Where Applied |
|---|---|
| **Singleton** | `City`, `Selector` — global access to unique instances |
| **ScriptableObject (Config)** | `CameraSettings`, `CityConfig`, `BuildingPreset` — immutable configuration separated from behavior |
| **POCO (Runtime State)** | `CityRuntimeState` — mutable state in memory, automatic reset between sessions |
| **Single Responsibility** | `CameraController`, `EconomySystem`, `PopulationSystem`, `UIManager` — isolated responsibility per system |
| **Observer / Event Bus** | `EventBus`, `UIManager` — decoupled inter-system communication via C# events |
| **Command** | `PlaceBuildingCommand`, `BulldozeCommand`, `ICommand`, `BuildingPlacement` — undo/redo with stack |
| **Dependency Injection (via parameter)** | `EconomySystem.Calculate()`, `PopulationSystem.Calculate()` — receive state and config explicitly, no hidden dependencies |
| **Polymorphism / Interfaces** | `IResourceSource`, `IResourceSink` — buildings as active network nodes, preparatory for the agent system |

---

## References and Studies

- [Game Programming Patterns](https://docs.google.com/document/d/1Ou3lJYsV_q99P-ejsP6zAVnZnWSGQ4Sa6VxBHPQYfYY/edit?usp=sharing)
- [SimCity One Page Documents](https://docs.google.com/document/d/1E2Y2-9Mp13S2S3KDdb2Ax4E8VpKy4Lu5e1K5XM3rtX4/edit?usp=sharing)
- [Citystate II Postmortem]()
- [Game Mechanics — Internal Economy]()

---

## Known Bugs

### High Priority

**No occupied tile check:** `BuildingPlacement.PlaceBuilding()` does not verify whether `City.instance.grid` already contains a building at the selected position. It is possible to overlap multiple buildings on the same tile without errors.

**Building snapping not aligned to grid:** assets are placed halfway between two tiles instead of at the center of the cell. The issue lies in the position calculation in `Selector.cs` or in how `_curIndicatorPos` is applied to `Instantiate` in `BuildingPlacement.cs`. Must be fixed before adding new placement features.

**Asset scale offset after refactoring:** following architectural changes, building assets have undergone a proportion shift in the scene. The issue has been isolated on a separate branch and must be fixed before merging.

---

### Medium Priority

**No money check before placement:** `BeginNewBuildingPlacement()` contains the comment `// check money` but no associated logic. Buildings can be placed even with a negative balance, with no player feedback.

**`FarmBuilding._storeFood` has non-functional logic:** in `TryProvideResource()`, the `_storeFood` field is immediately reset to `preset.food` after every retrieval, making it semantically equivalent to returning `preset.food` directly each turn. The slot never actually accumulates or depletes. If the intention is to simulate a food reserve that empties over time, the logic needs to be rethought.

---

### Low Priority / WIP

**`HouseBuilding._storeFood` has no gameplay impact:** food received via `ReceiveResource()` accumulates in `_storeFood` but does not affect population. Preparatory logic for the agent system — each resident will draw food from the house's individual slot.

**`DistributeFood` has no real impact:** food distribution to `IResourceSink` instances is implemented in `PopulationSystem` but commented out — it does not yet alter gameplay. To be connected to the agent system.

---

## Future Development

The project is in active refactoring with the goal of progressively applying established programming patterns. Planned developments for future steps:

- **Agent system (NPC/AI):** connect `HouseBuilding._storeFood` and `DistributeFood` to the agent system — each resident will draw food from their own house via `IResourceSink`, simulating labor and residential flows with NavMesh
- **Save system:** implementation of a robust persistence system based on JSON files and/or `PlayerPrefs`, with full city state serialization between sessions
- **Building rotation during placement:** ability to rotate an asset before confirming its position on a tile — implementable via dedicated input that modifies `Quaternion` during the preview phase
- **Dynamic menu with responsive UI:** building toolbar redesigned with an adaptive layout, informative tooltips for each building, and visual feedback on available resources
- **Dynamic day/night cycle:** procedural lighting system simulating the passage of time by modifying the `Directional Light` and `Skybox` in sync with game turns
- **Audio design:** integration of sound feedback for placement, demolition, and day transitions
- **BulldozeCommand Redo:** dedicated redo stack for bulldoze, separate from the placement stack

---

## Technical Requirements

- Unity 6.x (Built-In Render Pipeline)
- TextMeshPro
- Legacy Input System enabled (`Edit > Project Settings > Player > Active Input Handling: Both`)
