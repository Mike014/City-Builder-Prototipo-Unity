using System.Collections.Generic;
using UnityEngine;

// Gestisce lo stato globale della città, aggiorna l'interfaccia utente 
// e calcola l'economia a ogni fine turno.
// Attualmente implementato con il pattern Singleton.
public class City : MonoBehaviour
{
    [Header("City State")]
    // Lista dinamica che traccia tutti gli edifici attualmente presenti in scena.
    // public List<Building> buildings = new List<Building>();
    public Dictionary<Vector3Int, Building> grid = new Dictionary<Vector3Int, Building>();

    // Riferimento allo ScriptableObject che funge da database dei dati correnti.
    [SerializeField] private CityConfig _cityConfig;
    private CityRuntimeState _state;
    [SerializeField] private EconomySystem _economySystem;
    [SerializeField] private PopulationSystem _populationSystem;

    // Istanza globale per l'accesso facilitato (Singleton pattern).
    public static City instance;

    void Awake()
    {
        // Inizializzazione del Singleton
        instance = this;

        _state = new CityRuntimeState
        {
            money = _cityConfig.startingMoney,
            day = _cityConfig.startingDay,
            curPopulation = _cityConfig.startingPopulation,
            curJobs = 0,
            curFood = 0,
            maxPopulation = 0,
            maxJobs = 0
        };

        // Forza l'aggiornamento iniziale della UI
        // PublishCityState();
    }

    void Start()
    {
        PublishCityState();
    }

    // Registra un nuovo edificio piazzato, aggiornando le statistiche massime e il denaro.
    // Parametro building: L'edificio appena istanziato sulla griglia.
    // Sezione 3 : La firma ora risulta sbagliata?? 
    public void OnPlaceBuilding(Building building)
    {
        _state.money -= building.preset.cost;
        _state.maxPopulation += building.preset.population;
        _state.maxJobs += building.preset.jobs;

        // grid[posizione] = building;
        // buildings.Add(building);
        grid[Vector3Int.RoundToInt(building.transform.position)] = building;

        PublishCityState();
    }

    // Rimuove un edificio dalla simulazione, sottraendo i suoi contributi e distruggendo il GameObject.
    // Parametro building: L'edificio da demolire o rimuovere tramite Undo.
    // Sezione 3 : La firma ora risulta sbagliata??
    public void OnRemoveBuilding(Building building)
    {
        _state.money += building.preset.cost;
        _state.maxPopulation -= building.preset.population;
        _state.maxJobs -= building.preset.jobs;

        grid.Remove(Vector3Int.RoundToInt(building.transform.position));

        PublishCityState();

        // Il problema se creo un sistema Redo() in PlaceBuildingCommand
        // Non ho più il riferimento all'oggetto perchè è stato distrutto
        Destroy(building.gameObject);
    }

    // Avanza di un giorno e ricalcola tutte le metriche della simulazione.
    public void EndTurn()
    {
        _state.day++;
        _economySystem.Calculate(grid.Values, _state, _cityConfig);
        _populationSystem.Calculate(grid.Values, _state, _cityConfig);

        PublishCityState();
    }

    private void PublishCityState()
    {
        EventBus.Publish(new ResourceAmount
        {
            day = _state.day,
            money = _state.money,
            jobs = _state.curJobs,
            maxJobs = _state.maxJobs,
            food = _state.curFood,
            population = _state.curPopulation,
            maxPopulation = _state.maxPopulation
        });
    }
}