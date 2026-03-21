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
- UI con statistiche aggiornate in tempo reale tramite Event Bus — `UIManager` legge da `ResourceAmount`, disaccoppiamento completato
- Quattro tipologie di edifici: Casa, Fabbrica, Fattoria, Strada
- Undo del piazzamento edifici tramite Command Pattern (`Z`)
- Redo del piazzamento edifici (`Ctrl+Y`)
- Undo/Redo del bulldoze tramite `BulldozeCommand` dedicato
- Architettura Event Bus operativa — `City`, `EconomySystem`, `PopulationSystem`, `UIManager` comunicano tramite eventi senza accoppiamento diretto
- Griglia edifici basata su `Dictionary<Vector3Int, Building>` — lookup O(1) per bulldoze e piazzamento
- Architettura a interfacce — `IResourceSource` e `IResourceSink` per edifici produttori e consumatori
- `FarmBuilding` implementa `IResourceSource` — produce cibo con slot interno
- `HouseBuilding` implementa `IResourceSink` — consuma cibo, preparatoria per sistema agenti

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
Componente base attaccato a ogni prefab edificio istanziato in scena. Contiene un riferimento al proprio `BuildingPreset`. Le sottoclassi `FarmBuilding` e `HouseBuilding` estendono questa classe implementando le interfacce `IResourceSource` e `IResourceSink`.

**`FarmBuilding.cs`**
Sottoclasse di `Building` che implementa `IResourceSource`. Mantiene uno slot interno `_storedFood` inizializzato da `preset.food` in `Awake()`. `TryProvideResource()` restituisce il cibo disponibile e ricarica lo slot ad ogni prelievo.

**`HouseBuilding.cs`**
Sottoclasse di `Building` che implementa `IResourceSink`. Riceve cibo tramite `ReceiveResource()`. La logica di consumo individuale per residente è preparatoria per il sistema di agenti futuri — attualmente senza impatto diretto sul gameplay.

**`IResourceSource.cs`**
Interfaccia che definisce il contratto per edifici produttori: `TryProvideResource(out int amount)`. Il pattern `TryGet` con `out` gestisce il caso in cui la risorsa non è disponibile senza eccezioni o valori sentinella.

**`IResourceSink.cs`**
Interfaccia che definisce il contratto per edifici consumatori: `ReceiveResource(int amount)`.

**`BuildingPreset.cs`**
ScriptableObject che definisce le proprietà di ogni tipo di edificio. Dopo il refactoring i valori dei preset sono stati corretti: House `food: 0`, Factory `food: 0`, Road `population: 0`.

**`BuildingPlacement.cs`**
Gestisce l'intera pipeline di piazzamento e demolizione. Mantiene `_undoStack` e `_redoStack`. `PlaceBuilding()` usa `PlaceBuildingCommand` data-driven. `Bulldoze()` usa `BulldozeCommand` dedicato con semantica corretta — `Execute()` demolisce, `Undo()` ricostruisce. Fix applicato: `EventSystem.current.IsPointerOverGameObject()` impedisce il piazzamento nello stesso frame del click UI.

**`BulldozeCommand.cs`**
Implementazione dedicata del Command Pattern per la demolizione. `Execute()` rimuove l'edificio dalla griglia, `Undo()` lo reinstanzia — semantica inversa rispetto a `PlaceBuildingCommand`.

**`CameraController.cs`**
Gestisce tre comportamenti distinti separati in metodi privati: `Zooming()` per lo scroll della rotella del mouse con clamping, `Rotating()` per la rotazione tenendo premuto il tasto destro del mouse, `Moving()` per il movimento WASD relativo all'orientamento della camera. Tutti i parametri sono letti da `CameraSettings`.

**`City.cs`**
Singleton coordinatore della simulazione. Mantiene la griglia degli edifici come `Dictionary<Vector3Int, Building>` per lookup O(1). Delega i calcoli a `EconomySystem` e `PopulationSystem` tramite dependency injection, e pubblica lo stato aggiornato sull'`EventBus` tramite `PublishCityState()` — unico punto di pubblicazione per eliminare i publish multipli per turno.

**`EconomySystem.cs`**
Sistema dedicato al calcolo di denaro e lavoro. Accetta `IEnumerable<Building>`. Fix applicato: rimosso `EventBus.Publish` interno che causava flash errato nella UI con `food: 0` hardcoded.

**`PopulationSystem.cs`**
Sistema dedicato al calcolo di popolazione e cibo. `CalculateFood()` usa `GetComponent<IResourceSource>()` per raccogliere cibo dagli edifici produttori. `DistributeFood()` distribuisce alle `IResourceSink`. Fix applicato: carestia graduale — popolazione decresce di 1 per turno invece di crollare istantaneamente a 0.

**`EventBus.cs`**
Canale di messaggistica statico che implementa l'Observer Pattern. Espone l'evento `OnResourceUpdated` a cui i sistemi si iscrivono.

**`ResourceAmount`**
Struct C# aggiornata con tutti i campi necessari: `food`, `money`, `jobs`, `population`, `day`, `maxPopulation`, `maxJobs`. Viene costruita localmente e passata a `EventBus.Publish()`.

**`UIManager.cs`**
Si iscrive a `EventBus.OnResourceUpdated` in `OnEnable()` e si deregistra in `OnDisable()`. Legge ora direttamente dal parametro `ResourceAmount` — disaccoppiamento da `_citySettings` completato.

**`PlaceBuildingCommand.cs`**
Implementazione data-driven del Command Pattern. Salva solo `BuildingPreset` e `Vector3Int`. `Execute()` istanzia il prefab da zero, `Undo()` cerca nel Dictionary e rimuove.

**`Selector.cs`**
Singleton responsabile del rilevamento del tile sotto il cursore tramite raycast su piano matematico orizzontale.

---

## Pattern di design applicati

| Pattern | Dove applicato |
|---|---|
| **Singleton** | `City`, `Selector` — accesso globale a istanze uniche |
| **ScriptableObject (Data Container)** | `CameraSettings`, `CitySettings`, `BuildingPreset` — separazione dati/comportamento |
| **Single Responsibility** | `CameraController`, `EconomySystem`, `PopulationSystem`, `UIManager` — responsabilità isolate per sistema |
| **Observer / Event Bus** | `EventBus`, `UIManager` — comunicazione disaccoppiata tra sistemi tramite eventi C# |
| **Command** | `PlaceBuildingCommand`, `BulldozeCommand`, `ICommand`, `BuildingPlacement` — undo/redo con stack |
| **Dependency Injection** | `City` riceve `EconomySystem` e `PopulationSystem` dall'Inspector |
| **Polimorfismo / Interfacce** | `IResourceSource`, `IResourceSink` — edifici come nodi attivi della rete, preparatori per sistema agenti |

---

## Riferimenti e Studi

- [Game Programming Patterns](https://docs.google.com/document/d/1Ou3lJYsV_q99P-ejsP6zAVnZnWSGQ4Sa6VxBHPQYfYY/edit?usp=sharing)
- [SimCity One Page Documents](https://docs.google.com/document/d/1E2Y2-9Mp13S2S3KDdb2Ax4E8VpKy4Lu5e1K5XM3rtX4/edit?usp=sharing)
- [Citystate II Postmortem]()
- [Game Mechanics — Internal Economy]()

---

## Bug noti

### Alta priorità

**`CitySettings` mantiene lo stato tra sessioni di Play (Editor):** `CitySettings` è uno `ScriptableObject` con campi scritti a runtime (`money`, `curPopulation`, `curFood`, ecc.). In Unity Editor le modifiche agli asset SO persistono su disco — al secondo avvio il gioco parte con i valori della sessione precedente invece che con quelli iniziali. Fix: aggiungere un reset esplicito dei campi dinamici in `City.Awake()`, oppure separare lo stato runtime in una classe POCO non persistita.

**Nessun controllo tile occupata:** `BuildingPlacement.PlaceBuilding()` non verifica se `City.instance.grid` contiene già un edificio nella posizione selezionata. È possibile sovrapporre più edifici sullo stesso tile senza errori.

**Snapping edifici non allineato alla griglia:** gli asset vengono posizionati a metà tra due tile invece che al centro della casella. Il problema è nel calcolo della posizione in `Selector.cs` o nel modo in cui `_curIndicatorPos` viene applicato all'`Instantiate` in `BuildingPlacement.cs`. Da correggere prima di aggiungere nuove funzionalità di piazzamento.

**Sfasamento dello scale degli asset dopo il refactoring:** a seguito delle modifiche architetturali, gli asset degli edifici hanno subito uno sfasamento delle proporzioni in scena. Il problema è stato isolato su un branch separato e deve essere corretto prima del merge.

---

### Media priorità

**Nessun controllo sul denaro prima del piazzamento:** `BeginNewBuildingPlacement()` contiene il commento `// check money` ma nessuna logica associata. È possibile piazzare edifici anche con saldo negativo, senza alcun feedback al giocatore.

**`FarmBuilding._storeFood` ha logica non funzionante:** in `TryProvideResource()` il campo `_storeFood` viene immediatamente resettato a `preset.food` dopo ogni prelievo, il che lo rende semanticamente equivalente a restituire `preset.food` direttamente ogni turno. Lo slot non accumula né si esaurisce mai realmente. Se l'intenzione è simulare una riserva di cibo che si svuota nel tempo, la logica va ripensata.

---

### Bassa priorità / WIP

**`HouseBuilding._storeFood` senza impatto sul gameplay:** il cibo ricevuto da `ReceiveResource()` viene accumulato in `_storeFood` ma non influenza `_citySettings.curPopulation`. Logica preparatoria per il sistema di agenti — ogni residente preleverà cibo dallo slot individuale della casa invece di leggere da `CitySettings`.

**`DistributeFood` senza impatto reale:** la distribuzione del cibo alle `IResourceSink` è implementata in `PopulationSystem` ma commentata — non altera ancora il gameplay. Da collegare al sistema di agenti.

---

## Limitazioni note

**Reset manuale dei dati tra una sessione e l'altra:** `CitySettings` è uno ScriptableObject. In fase di prototipazione i valori dinamici (denaro, popolazione, cibo, giorno) vengono modificati a runtime ma non vengono persistiti al riavvio. È necessario resettarli manualmente dall'Inspector prima di ogni nuova sessione di test. Un sistema di salvataggio dedicato è pianificato tra gli sviluppi futuri.

---

## Sviluppi futuri

Il progetto è in refactoring attivo con l'obiettivo di applicare progressivamente pattern di programmazione consolidati. Gli sviluppi pianificati per step successivi sono:

- **Sistema di agenti (NPC/IA):** collegare `HouseBuilding._storeFood` e `DistributeFood` al sistema di agenti — ogni residente preleverà cibo dalla propria casa tramite `IResourceSink`, simulando flussi lavorativi e residenziali con NavMesh
- **Sistema di salvataggio:** implementazione di un sistema di persistenza robusto basato su file JSON e/o `PlayerPrefs`, con serializzazione dello stato completo della città tra una sessione e l'altra
- **Rotazione degli edifici durante il piazzamento:** possibilità di ruotare l'asset su se stesso prima di confermarne la posizione sul tile — implementabile tramite input dedicato che modifica `Quaternion` durante la fase di preview
- **Menu dinamico con UI responsiva:** toolbar degli edifici riprogettata con layout adattivo, tooltip informativi per ogni edificio e feedback visivo sulle risorse disponibili
- **Ciclo giorno/notte dinamico:** sistema di illuminazione procedurale che simula il passaggio del tempo modificando la `Directional Light` e il `Skybox` in sincronia con i turni di gioco
- **Audio design:** integrazione di feedback sonori per piazzamento, demolizione e transizioni di giorno
- **BulldozeCommand Redo:** stack dedicato per il bulldoze separato da quello del piazzamento

---

## Requisiti tecnici

- Unity 6.x (Built-In Render Pipeline)
- TextMeshPro
- Legacy Input System abilitato (`Edit > Project Settings > Player > Active Input Handling: Both`)
