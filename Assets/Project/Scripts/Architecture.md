## Struttura delle Cartelle

```
Scripts/
├── Core/               # MonoBehaviour principali della scena
├── Interfaces/         # Contratti (ICommand, IResourceSource, IResourceSink)
├── Resources/          # Strutture dati leggere (ResourceAmount)
├── SO/                 # ScriptableObject e relativi script
│   ├── Camera/
│   └── City/
└── Refactoring/        # Sistemi disaccoppiati
    ├── Building/       # Comandi e classi edificio specializzate
    ├── Events/         # EventBus e sistemi ascoltatori
    │   └── Systems/
    └── Managers/       # UIManager
```

---

### Core

| Script | Tipo | Funzione |
| :--- | :--- | :--- |
| **Building.cs** | `MonoBehaviour` | Componente base di un edificio istanziato. Espone il riferimento al suo `BuildingPreset`. Superclasse di `FarmBuilding` e `HouseBuilding`. |
| **BuildingPlacement.cs** | `MonoBehaviour` | Gestisce piazzamento e demolizione edifici. Aggiorna gli indicatori di anteprima, crea ed esegue i comandi (`PlaceBuildingCommand`, `BulldozeCommand`) e mantiene i due stack Undo/Redo. |
| **CameraController.cs** | `MonoBehaviour` | Gestisce l'input della telecamera: zoom (rotella), rotazione (tasto destro), movimento (WASD). Usa i limiti definiti in `CameraSettings`. |
| **City.cs** | `MonoBehaviour` *(Singleton)* | Gestore globale della città. Mantiene la griglia `Dictionary<Vector3Int, Building>` per lookup O(1), gestisce piazzamento/rimozione edifici, coordina `EconomySystem` e `PopulationSystem` a ogni `EndTurn()`, pubblica lo stato via `EventBus`. |
| **Selector.cs** | `MonoBehaviour` *(Singleton)* | Esegue il Raycast sul piano per ottenere la posizione del tile sotto il mouse. Ignora i click sopra elementi della UI. |

---

### ScriptableObjects (Data Layer)

| Script | Tipo | Funzione |
| :--- | :--- | :--- |
| **BuildingPreset.cs** | `ScriptableObject` | Dati di configurazione di un tipo di edificio: `cost`, `costPerTurn`, `prefab`, contributi a `population`, `jobs`, `food`. |
| **CameraSettings.cs** | `ScriptableObject` | Parametri della telecamera: velocità, limiti di rotazione e zoom. |
| **CitySettings.cs** | `ScriptableObject` | Stato globale mutabile della città: `money`, `day`, `curPopulation`, `maxPopulation`, `curJobs`, `maxJobs`, `curFood`, `incomePerJobs`. |

---

### Refactoring / Building

| Script | Tipo | Funzione |
| :--- | :--- | :--- |
| **PlaceBuildingCommand.cs** | `Classe` *(ICommand)* | Memorizza `BuildingPreset` + `Vector3Int`. `Execute()` istanzia il prefab e chiama `City.OnPlaceBuilding()`. `Undo()` recupera l'edificio dalla griglia e chiama `City.OnRemoveBuilding()`. |
| **BulldozeCommand.cs** | `Classe` *(ICommand)* | Memorizza `BuildingPreset` + `Vector3Int`. `Execute()` demolisce l'edificio. `Undo()` lo reistanzia dalla posizione salvata — speculare a `PlaceBuildingCommand`. |
| **FarmBuilding.cs** | `MonoBehaviour` *(Building, IResourceSource)* | Estende `Building`. Implementa `TryProvideResource()`: restituisce il cibo prodotto dal preset e resetta lo stock per il turno successivo. |
| **HouseBuilding.cs** | `MonoBehaviour` *(Building, IResourceSink)* | Estende `Building`. Implementa `ReceiveResource()`: accumula cibo ricevuto in `_storeFood` (preparatorio per il sistema agenti, non ancora attivo in gameplay). |

---

### Refactoring / Events

| Script | Tipo | Funzione |
| :--- | :--- | :--- |
| **EventBus.cs** | `Classe statica` | Bus centrale degli eventi. Espone `OnResourceUpdated (Action<ResourceAmount>)` e il metodo statico `Publish()` per disaccoppiare i sistemi dalla UI. |
| **EconomySystem.cs** | `MonoBehaviour` | Chiamato da `City.EndTurn()`. Calcola il reddito (`curJobs × incomePerJobs`), sottrae i costi di mantenimento di ogni edificio e aggiorna `curJobs = Min(curPopulation, maxJobs)`. |
| **PopulationSystem.cs** | `MonoBehaviour` | Chiamato da `City.EndTurn()`. Ricalcola `curFood` interrogando `IResourceSource` sugli edifici, poi aggiorna `curPopulation` in base alla disponibilità di cibo e allo spazio abitativo (`maxPopulation`). |

---

### Refactoring / Managers

| Script | Tipo | Funzione |
| :--- | :--- | :--- |
| **UIManager.cs** | `MonoBehaviour` | Si iscrive a `EventBus.OnResourceUpdated` in `OnEnable` e si disiscrrive in `OnDisable`. Aggiorna un unico `TextMeshProUGUI` con giorno, denaro, popolazione, lavoro e cibo. |

---

### Interfaces & Data

| Script | Tipo | Funzione |
| :--- | :--- | :--- |
| **ICommand.cs** | `Interface` | Contratto per il Command Pattern: `Execute()` e `Undo()`. |
| **IResourceSource.cs** | `Interface` | Contratto per produttori di risorse: `TryProvideResource(out int amount)`. Implementato da `FarmBuilding`. |
| **IResourceSink.cs** | `Interface` | Contratto per consumatori di risorse: `ReceiveResource(int amount)`. Implementato da `HouseBuilding`. |
| **ResourceAmount.cs** | `Struct` | Contenitore dati leggero (stack-allocated) trasmesso via EventBus: `day`, `money`, `food`, `jobs`, `maxJobs`, `population`, `maxPopulation`. |

---

### Architettura in Sintesi (Flusso Dati)

1. **Input:** Il mouse aggiorna `Selector` → `BuildingPlacement` legge la posizione del tile.
2. **Azione:** `BuildingPlacement` crea un `PlaceBuildingCommand` o `BulldozeCommand` → chiama `Execute()` → lo spinge sull'undo stack e svuota il redo stack.
3. **Undo/Redo:** `Z` fa `Undo()` e sposta il comando sul redo stack. `Ctrl+Y` fa `Execute()` e lo riporta sull'undo stack.
4. **Stato griglia:** I comandi notificano `City.OnPlaceBuilding()` / `City.OnRemoveBuilding()`, che aggiornano `Dictionary<Vector3Int, Building>` e `CitySettings`.
5. **Ciclo di turno:** `City.EndTurn()` incrementa il giorno → `EconomySystem.Calculate()` aggiorna denaro e lavoro → `PopulationSystem.Calculate()` interroga `IResourceSource`, aggiorna cibo e popolazione.
6. **Reazione UI:** Ogni modifica allo stato chiama `City.PublishCityState()` → `EventBus.Publish()` → `UIManager.UpdateStatText()` aggiorna il testo a schermo.

---

### Pattern di Design Utilizzati

| Pattern | Dove | Scopo |
| :--- | :--- | :--- |
| **Singleton** | `City`, `Selector` | Punto di accesso globale unico per sistemi condivisi. |
| **Command** | `PlaceBuildingCommand`, `BulldozeCommand` + stack in `BuildingPlacement` | Incapsula le azioni; abilita Undo/Redo doppio stack. |
| **Observer** | `EventBus` (`Action<ResourceAmount>`) | Disaccoppia la logica di simulazione dall'interfaccia utente. |
| **Data-Driven** | `BuildingPreset`, `CitySettings`, `CameraSettings` (ScriptableObject) | Separa i parametri di bilanciamento dal codice sorgente. |
| **Strategy / Polimorfismo** | `IResourceSource`, `IResourceSink` su `FarmBuilding` / `HouseBuilding` | Permette a `PopulationSystem` di interrogare gli edifici senza conoscerne il tipo concreto. |
