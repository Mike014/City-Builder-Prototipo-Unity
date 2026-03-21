# City Builder — Prototipo Unity

## Descrizione del progetto

City Builder è un prototipo di gioco gestionale in fase di sviluppo attivo, costruito con Unity e C#. Il progetto si ispira al genere dei city builder strategici — titoli come *Anno*, *Frostpunk* o *Age of Empires* — in cui il giocatore costruisce e gestisce una città su una griglia, bilanciando risorse economiche, popolazione, lavoro e approvvigionamento alimentare.

Il progetto è attualmente in uno **stadio prematuro ma funzionante**: la pipeline core di piazzamento edifici, gestione risorse e controllo camera è operativa. L'obiettivo a lungo termine è espandere il sistema verso simulazioni più complesse, con la possibile introduzione di agenti autonomi (NPC/IA) che popolino e animino la città in modo procedurale.

---

## Stato attuale

Le funzionalità implementate e funzionanti includono:

- Piazzamento e demolizione di edifici su griglia
- Sistema di risorse a turni (denaro, popolazione, lavoro, cibo)
- Controllo camera con pan, zoom e rotazione
- Rilevamento tile tramite raycast sul piano di gioco
- UI con statistiche aggiornate in tempo reale tramite Event Bus
- Quattro tipologie di edifici: Casa, Fabbrica, Fattoria, Strada
- Undo del piazzamento edifici tramite Command Pattern (`Z`)
- Redo del piazzamento edifici (`Ctrl+Y`)
- Redo del bulldoze (`Ctrl+Y`) — ripristina l'edificio demolito
- Architettura Event Bus operativa — `City`, `EconomySystem`, `PopulationSystem`, `UIManager` comunicano tramite eventi senza accoppiamento diretto
- Griglia edifici basata su `Dictionary<Vector3Int, Building>` — lookup O(1) per bulldoze e piazzamento

> **Nota sul Road asset:** L'edificio Strada è implementato come un piano 3D in scala `(0.1, 0.1, 0.1)` per adattarsi esattamente alla dimensione di un tile della griglia. Non contribuisce a popolazione, lavoro o cibo, ma fa parte della logica di espansione urbana e ha un `costPerTurn` associato come qualsiasi altra struttura.

---

## Architettura del codice

Il progetto applica in modo esplicito pattern di progettazione software, con particolare attenzione alla separazione tra **dati** e **comportamento**.

### ScriptableObject come contenitori di dati

Una scelta progettuale distintiva rispetto all'implementazione base del corso è l'introduzione di due ScriptableObject dedicati alla configurazione:

**`CameraSettings`** — Contiene tutti i parametri di configurazione della camera (velocità di movimento, limiti di rotazione, range di zoom, velocità di rotazione e zoom). Separare questi dati dal MonoBehaviour permette di modificare i valori dall'Inspector senza toccare il codice, e di creare profili camera multipli facilmente.

**`CitySettings`** — Contiene lo stato della città (denaro, giorno, popolazione corrente e massima, lavoro corrente e massimo, cibo, reddito per lavoro). Centralizzare questi dati in un asset separato garantisce che siano accessibili e modificabili indipendentemente dalla scena.

---

### Script principali

**`Building.cs`**
Componente attaccato a ogni prefab edificio istanziato in scena. Contiene un riferimento al proprio `BuildingPreset`, che funge da definizione dati dell'edificio. Questo collegamento è necessario per permettere al sistema di città di leggere i contributi statistici di ciascun edificio al momento del piazzamento e della demolizione.

**`BuildingPreset.cs`**
ScriptableObject che definisce le proprietà di ogni tipo di edificio: costo di acquisto, costo di mantenimento per turno, prefab associato e contributi alle risorse della città (popolazione, posti di lavoro, produzione di cibo). I valori possono essere positivi o negativi a seconda della natura dell'edificio — una fattoria produce cibo, una casa aumenta la capacità abitativa, una fabbrica genera posti di lavoro.

**`BuildingPlacement.cs`**
Gestisce l'intera pipeline di piazzamento e demolizione. Mantiene due stack separati — `_undoStack` per le azioni eseguite e `_redoStack` per le azioni annullate. `PlaceBuilding()` crea un `PlaceBuildingCommand` data-driven (preset + posizione, nessun riferimento al MonoBehaviour) e lo pusha nell'`_undoStack`. `Bulldoze()` esegue lookup O(1) sul Dictionary tramite `TryGetValue`, crea un comando e lo pusha nel `_redoStack` per permettere il ripristino tramite `Ctrl+Y`.

**`CameraController.cs`**
Gestisce tre comportamenti distinti separati in metodi privati: `Zooming()` per lo scroll della rotella del mouse con clamping, `Rotating()` per la rotazione tenendo premuto il tasto destro del mouse, `Moving()` per il movimento WASD relativo all'orientamento della camera (la componente Y del vettore forward viene azzerata e normalizzata per garantire movimento esclusivamente sul piano orizzontale, indipendentemente dall'inclinazione della camera). Tutti i parametri sono letti da `CameraSettings`.

**`City.cs`**
Singleton coordinatore della simulazione. Mantiene la griglia degli edifici come `Dictionary<Vector3Int, Building>` per lookup O(1). Delega i calcoli a `EconomySystem` e `PopulationSystem` tramite dependency injection, e pubblica lo stato aggiornato sull'`EventBus` tramite `PublishCityState()`.

**`EconomySystem.cs`**
Sistema dedicato al calcolo di denaro e lavoro. Accetta `IEnumerable<Building>` per disaccoppiarsi dalla struttura dati concreta. Pubblica i risultati sull'`EventBus`.

**`PopulationSystem.cs`**
Sistema dedicato al calcolo di popolazione e cibo. Stessa struttura di `EconomySystem` — accetta `IEnumerable<Building>`, calcola, pubblica sull'`EventBus`.

**`EventBus.cs`**
Canale di messaggistica statico che implementa l'Observer Pattern. Espone l'evento `OnResourceUpdated` a cui i sistemi si iscrivono. Publisher e Subscriber non si conoscono — comunicano solo tramite il Bus.

**`ResourceAmount`**
Struct C# che trasporta i dati delle risorse aggiornate negli eventi. Viene costruita localmente nei metodi e passata a `EventBus.Publish()` — nessuna allocazione heap, nessuno stato residuo.

**`UIManager.cs`**
Si iscrive a `EventBus.OnResourceUpdated` in `OnEnable()` e si deregistra in `OnDisable()`. Aggiorna il testo delle statistiche ogni volta che riceve un evento. Completamente disaccoppiato da `City.cs`.

**`PlaceBuildingCommand.cs`**
Implementazione data-driven del Command Pattern. Salva solo `BuildingPreset` e `Vector3Int` — nessun riferimento al MonoBehaviour istanziato. `Execute()` istanzia il prefab da zero tramite `Object.Instantiate`, permettendo un Redo corretto senza `MissingReferenceException`. `Undo()` cerca l'edificio nel Dictionary tramite `TryGetValue` e lo rimuove.

**`Selector.cs`**
Singleton responsabile del rilevamento del tile sotto il cursore. Proietta un raggio dalla camera verso un piano matematico orizzontale all'altezza zero della scena. Il punto di intersezione viene traslato di `-0.5` sull'asse X e arrotondato con `Mathf.CeilToInt` per ottenere coordinate intere allineate alla griglia. Se il cursore è sopra un elemento UI, restituisce un vettore sentinella `(0, -99, 0)` che i sistemi a valle usano per ignorare l'input.

---

## Pattern di design applicati

| Pattern | Dove applicato |
|---|---|
| **Singleton** | `City`, `Selector` — accesso globale a istanze uniche |
| **ScriptableObject (Data Container)** | `CameraSettings`, `CitySettings`, `BuildingPreset` — separazione dati/comportamento |
| **Single Responsibility** | `CameraController`, `EconomySystem`, `PopulationSystem`, `UIManager` — responsabilità isolate per sistema |
| **Observer / Event Bus** | `EventBus`, `UIManager` — comunicazione disaccoppiata tra sistemi tramite eventi C# |
| **Command** | `PlaceBuildingCommand`, `ICommand`, `BuildingPlacement` — undo/redo del piazzamento con stack |
| **Dependency Injection** | `City` riceve `EconomySystem` e `PopulationSystem` dall'Inspector invece di cercarli autonomamente |

---

## Riferimenti e Studi

- [Game Programming Patterns](https://docs.google.com/document/d/1Ou3lJYsV_q99P-ejsP6zAVnZnWSGQ4Sa6VxBHPQYfYY/edit?usp=sharing)
- [SimCity One Page Documents](https://docs.google.com/document/d/1E2Y2-9Mp13S2S3KDdb2Ax4E8VpKy4Lu5e1K5XM3rtX4/edit?usp=sharing)
- [Citystate II Postmortem]()
- [Game Mechanics — Internal Economy]()

---

## Bug noti

**Snapping edifici non allineato alla griglia:** gli asset vengono posizionati a metà tra due tile invece che al centro della casella. Il problema è nel calcolo della posizione in `Selector.cs` o nel modo in cui `_curIndicatorPos` viene applicato all'Instantiate in `BuildingPlacement.cs`. Da correggere prima di procedere con nuove funzionalità di piazzamento.

**Sfasamento dello scale degli asset dopo il refactoring:** a seguito delle modifiche architetturali, gli asset degli edifici hanno subito uno sfasamento delle proporzioni in scena. Il problema è stato isolato su un branch separato e deve essere corretto prima del merge.

**Semantica Undo/Redo del bulldoze non standard:** il bulldoze pusha direttamente nel `_redoStack` invece che nell'`_undoStack`, rompendo la semantica canonica del Command Pattern. Soluzione futura: introdurre un `BulldozeCommand` dedicato con `Execute()` e `Undo()` invertiti rispetto a `PlaceBuildingCommand`.

**`UIManager` legge ancora da `_citySettings`:** `UpdateStatText` riceve `ResourceAmount` ma non la usa — legge direttamente dallo ScriptableObject. Da completare per chiudere il disaccoppiamento dalla UI.

---

## Limitazioni note

**Reset manuale dei dati tra una sessione e l'altra:** `CitySettings` è uno ScriptableObject. In fase di prototipazione i valori dinamici (denaro, popolazione, cibo, giorno) vengono modificati a runtime ma non vengono persistiti al riavvio. È necessario resettarli manualmente dall'Inspector prima di ogni nuova sessione di test. Un sistema di salvataggio dedicato è pianificato tra gli sviluppi futuri.

---

## Sviluppi futuri

Il progetto è in refactoring attivo con l'obiettivo di applicare progressivamente pattern di programmazione consolidati. Gli sviluppi pianificati per step successivi sono:

- **Refactoring e pattern:** applicazione sistematica di game programming pattern (Command, State Machine, Object Pooling, Event Bus) sull'intera codebase — priorità assoluta prima di aggiungere nuove funzionalità
- **Sistema di salvataggio:** implementazione di un sistema di persistenza robusto basato su file JSON e/o `PlayerPrefs`, con serializzazione dello stato completo della città tra una sessione e l'altra
- **Rotazione degli edifici durante il piazzamento:** possibilità di ruotare l'asset (Casa, Fabbrica, Fattoria) su se stesso prima di confermarne la posizione sul tile, come avviene nei city builder moderni — implementabile tramite input dedicato che modifica `Quaternion` durante la fase di preview
- **Menu dinamico con UI responsiva:** toolbar degli edifici riprogettata con layout adattivo, tooltip informativi per ogni edificio e feedback visivo sulle risorse disponibili
- **Ciclo giorno/notte dinamico:** sistema di illuminazione procedurale che simula il passaggio del tempo modificando la `Directional Light` e il `Skybox` in sincronia con i turni di gioco
- **Audio design:** integrazione di feedback sonori per piazzamento, demolizione e transizioni di giorno — da costruire su un'architettura audio pulita prima dell'implementazione
- **Agenti autonomi (NPC/IA):** popolamento della città con personaggi che si muovono tra edifici tramite NavMesh, simulando flussi lavorativi e residenziali

---

## Requisiti tecnici

- Unity 6.x (Built-In Render Pipeline)
- TextMeshPro
- Legacy Input System abilitato (`Edit > Project Settings > Player > Active Input Handling: Both`)
