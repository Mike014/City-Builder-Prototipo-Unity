### Core

| Script | Tipo | Funzione |
| :--- | :--- | :--- |
| **Building.cs** | `MonoBehaviour` | Componente dati di un edificio istanziato. Contiene solo il riferimento al suo `BuildingPreset`. |
| **BuildingPlacement.cs** | `MonoBehaviour` | Gestisce piazzamento e demolizione edifici. Aggiorna l'indicatore di anteprima, esegue i comandi via stack (`PlaceBuildingCommand`) e implementa l'undo. |
| **CameraController.cs** | `MonoBehaviour` | Gestisce l'input della telecamera: zoom (rotella), rotazione (tasto destro), movimento (WASD). Usa i limiti definiti in `CameraSettings`. |
| **City.cs** | `MonoBehaviour` *(Singleton)* | Gestore globale della città. Mantiene la lista degli edifici, gestisce piazzamento/rimozione, coordina `EconomySystem` e `PopulationSystem` a fine turno, pubblica lo stato via EventBus. |
| **Selector.cs** | `MonoBehaviour` *(Singleton)* | Esegue il Raycast sul piano per ottenere la posizione del tile sotto il mouse. Ignora i click sopra elementi della UI. |

---

### ScriptableObjects (Data Layer)

| Script | Tipo | Funzione |
| :--- | :--- | :--- |
| **BuildingPreset.cs** | `ScriptableObject` | Dati di configurazione di un tipo di edificio: costo iniziale, costo/turno, prefab, contributi a popolazione/lavoro/cibo. |
| **CameraSettings.cs** | `ScriptableObject` | Parametri della telecamera: velocità, limiti di rotazione e zoom. |
| **CitySettings.cs** | `ScriptableObject` | Stato globale della città: denaro, giorno, popolazione, lavoro, cibo, massimi e moltiplicatore reddito. |

---

### Refactoring (Sistemi Disaccoppiati)

| Script | Tipo | Funzione |
| :--- | :--- | :--- |
| **EconomySystem.cs** | `MonoBehaviour` | Calcola il reddito (posti lavoro × moltiplicatore), i costi di mantenimento e i posti disponibili. Pubblica le variazioni via EventBus. |
| **PopulationSystem.cs** | `MonoBehaviour` | Calcola la crescita/decrescita della popolazione in base al cibo disponibile. Pubblica le variazioni via EventBus. |
| **UIManager.cs** | `MonoBehaviour` | Ascolta `EventBus.OnResourceUpdated` e aggiorna il testo UI (TextMeshPro) con giorno, denaro, popolazione, lavoro e cibo. |
| **PlaceBuildingCommand.cs**| `Classe` *(ICommand)* | Implementazione del Command Pattern: `Execute()` chiama `City.OnPlaceBuilding()`, `Undo()` chiama `City.OnRemoveBuilding()`. |
| **EventBus.cs** | `Classe statica` | Bus centrale degli eventi. Espone `OnResourceUpdated (Action<ResourceAmount>)` e il metodo `Publish()` per disaccoppiare i sistemi dalla UI. |

---

### Interfaces & Data

| Script | Tipo | Funzione |
| :--- | :--- | :--- |
| **ICommand.cs** | `Interface` | Contratto per il Command Pattern. Richiede l'implementazione dei metodi `Execute()` e `Undo()`. |
| **ResourceAmount.cs** | `Struct` | Contenitore dati leggero (allocato sullo stack) per food, money, jobs, population, trasmesso tramite EventBus. |

---

### Architettura in Sintesi (Flusso Dati)

1. **Input:** L'utente usa il mouse `->` `Selector` calcola le coordinate.
2. **Azione:** `BuildingPlacement` crea un `PlaceBuildingCommand` `->` Esegue il piazzamento.
3. **Logica Centrale:** Il comando notifica `City` (aggiunta/rimozione).
4. **Ciclo di Turno:** `City` chiama `EndTurn` `->` `EconomySystem` e `PopulationSystem` elaborano i dati matematici.
5. **Reazione UI:** I sistemi elaborano i dati `->` `EventBus.Publish()` invia il nuovo stato `->` `UIManager` aggiorna i testi a schermo.

**Pattern di Design Utilizzati:**
* **Singleton:** `City`, `Selector` (Punti di accesso globali per sistemi unici).
* **Command Pattern:** `PlaceBuildingCommand` + Stack in `BuildingPlacement` (Permette l'Undo/Redo e incapsula le azioni).
* **Observer Pattern:** `EventBus` (Disaccoppia totalmente i calcoli logici dall'interfaccia utente).
* **Data-Driven Design:** `ScriptableObject` (Separa i parametri di bilanciamento dal codice sorgente).



