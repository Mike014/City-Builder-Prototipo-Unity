using System.Collections.Generic;
using UnityEngine;

// TODO: Undo non ripristina il denaro — OnRemoveBuilding non restituisce il costo dell'edificio
// Fixare il sistema Observer leggi il documento pdf

// Gestisce lo stato globale della città, aggiorna l'interfaccia utente 
// e calcola l'economia a ogni fine turno.
// Attualmente implementato con il pattern Singleton.
public class City : MonoBehaviour
{
    [Header("City State")]
    // Lista dinamica che traccia tutti gli edifici attualmente presenti in scena.
    public List<Building> buildings = new List<Building>();

    // Riferimento allo ScriptableObject che funge da database dei dati correnti.
    [SerializeField] private CitySettings _citySettings;
    [SerializeField] private EconomySystem _economySystem;
    [SerializeField] private PopulationSystem _populationSystem;

    // Istanza globale per l'accesso facilitato (Singleton pattern).
    public static City instance;

    void Awake()
    {
        // Inizializzazione del Singleton
        instance = this;

        // Forza l'aggiornamento iniziale della UI
        // PublishCityState();
    }

    void Start()
    {
        PublishCityState();
    }

    // Registra un nuovo edificio piazzato, aggiornando le statistiche massime e il denaro.
    // Parametro building: L'edificio appena istanziato sulla griglia.
    public void OnPlaceBuilding(Building building)
    {
        _citySettings.money -= building.preset.cost;
        _citySettings.maxPopulation += building.preset.population;
        _citySettings.maxJobs += building.preset.jobs;

        buildings.Add(building);

        PublishCityState();
    }

    // Rimuove un edificio dalla simulazione, sottraendo i suoi contributi e distruggendo il GameObject.
    // Parametro building: L'edificio da demolire o rimuovere tramite Undo.
    public void OnRemoveBuilding(Building building)
    {
        _citySettings.maxPopulation -= building.preset.population;
        _citySettings.maxJobs -= building.preset.jobs;

        buildings.Remove(building);

        PublishCityState();

        Destroy(building.gameObject);
    }

    // Avanza di un giorno e ricalcola tutte le metriche della simulazione.
    public void EndTurn()
    {
        _citySettings.day++;
        _economySystem.Calculate(buildings);
        _populationSystem.Calculate(buildings);

        PublishCityState();
    }

    private void PublishCityState()
    {
        Debug.Log("PublishCityState chiamato");
        EventBus.Publish(new ResourceAmount
        {
            money = _citySettings.money,
            jobs = _citySettings.curJobs,
            food = _citySettings.curFood,
            population = _citySettings.curPopulation
        });
    }
}