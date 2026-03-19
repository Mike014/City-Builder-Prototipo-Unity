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
- UI con statistiche aggiornate in tempo reale
- Quattro tipologie di edifici: Casa, Fabbrica, Fattoria, Strada

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
Gestisce l'intera pipeline di piazzamento e demolizione. Internamente separa le responsabilità in metodi dedicati: `PlacementIndicator()` aggiorna la posizione del cursore visivo ogni 0.05 secondi (throttling intenzionale per ottimizzare le performance), `PlaceBuilding()` istanzia il prefab e notifica il sistema città, `Bulldoze()` cerca l'edificio nella lista tramite lambda e lo rimuove. L'annullamento del piazzamento è isolato in `PressCancelBuildingPlacement()` e viene invocato solo quando `_currentlyPlacing` è attivo.

**`CameraController.cs`**
Gestisce tre comportamenti distinti separati in metodi privati: `Zooming()` per lo scroll della rotella del mouse con clamping, `Rotating()` per la rotazione tenendo premuto il tasto destro del mouse, `Moving()` per il movimento WASD relativo all'orientamento della camera (la componente Y del vettore forward viene azzerata e normalizzata per garantire movimento esclusivamente sul piano orizzontale, indipendentemente dall'inclinazione della camera). Tutti i parametri sono letti da `CameraSettings`.

**`City.cs`**
Singleton che gestisce la logica economica e demografica della città. Al termine di ogni turno esegue in sequenza: `CalculateMoney()` (reddito da lavoro meno costi di mantenimento degli edifici), `CalculatePopulation()` (crescita vincolata dalla disponibilità di cibo), `CalculateJobs()` (i posti occupati non possono superare né la popolazione né i posti disponibili), `CalculateFood()` (somma dei contributi alimentari di tutti gli edifici attivi). Tutti i dati di stato sono delegati a `CitySettings`.

**`Selector.cs`**
Singleton responsabile del rilevamento del tile sotto il cursore. Proietta un raggio dalla camera verso un piano matematico orizzontale all'altezza zero della scena. Il punto di intersezione viene traslato di `-0.5` sull'asse X e arrotondato con `Mathf.CeilToInt` per ottenere coordinate intere allineate alla griglia. Se il cursore è sopra un elemento UI, restituisce un vettore sentinella `(0, -99, 0)` che i sistemi a valle usano per ignorare l'input.

---

## Pattern di design applicati

| Pattern | Dove applicato |
|---|---|
| **Singleton** | `City`, `Selector` — accesso globale a istanze uniche |
| **ScriptableObject (Data Container)** | `CameraSettings`, `CitySettings`, `BuildingPreset` — separazione dati/comportamento |
| **Single Responsibility** | `CameraController` — zoom, rotazione e movimento isolati in metodi privati |
| **Observer (implicito)** | `City.OnPlaceBuilding` / `City.OnRemoveBuilding` — eventi di sistema notificati al gestore centrale |

---

## Limitazioni note

**Reset manuale dei dati tra una sessione e l'altra:** `CitySettings` è uno ScriptableObject. In fase di prototipazione i valori dinamici (denaro, popolazione, cibo, giorno) vengono modificati a runtime ma non vengono persistiti al riavvio. È necessario resettarli manualmente dall'Inspector prima di ogni nuova sessione di test. Un sistema di salvataggio dedicato è pianificato tra gli sviluppi futuri.

---

## Sviluppi futuri

Il progetto è in refactoring attivo con l'obiettivo di applicare progressivamente pattern di programmazione consolidati. Gli sviluppi pianificati per step successivi sono:

- **Refactoring e pattern:** applicazione sistematica di game programming pattern (Command, State Machine, Object Pooling, Event Bus) sull'intera codebase
- **Sistema di salvataggio:** implementazione di un sistema di persistenza robusto basato su file JSON e/o `PlayerPrefs`, con serializzazione dello stato completo della città tra una sessione e l'altra
- **Agenti autonomi (NPC/IA):** popolamento della città con personaggi che si muovono tra edifici tramite NavMesh, simulando flussi lavorativi e residenziali
- **Menu Dinamico**

---

## Requisiti tecnici

- Unity 6.x (Built-In Render Pipeline)
- TextMeshPro
- Legacy Input System abilitato (`Edit > Project Settings > Player > Active Input Handling: Both`)
