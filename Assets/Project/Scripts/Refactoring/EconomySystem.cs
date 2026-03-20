
using UnityEngine;
using System.Collections.Generic;


public class EconomySystem : MonoBehaviour
{
    // Riferimento allo ScriptableObject che funge da database dei dati correnti.
    [SerializeField] private CitySettings _citySettings;

    // Lista dinamica che traccia tutti gli edifici attualmente presenti in scena.
    // public List<Building> buildings = new List<Building>();
    // TODO: ricevere la lista buildings da City.cs invece di mantenerla autonomamente
    // per evitare duplicazione e rischio di desincronizzazione

    // Calcola le entrate basate sull'occupazione e sottrae i costi di mantenimento degli edifici.
    void CalculateMoney(List<Building> buildings)
    {
        _citySettings.money += _citySettings.curJobs * _citySettings.incomePerJobs;

        foreach (Building building in buildings)
        {
            _citySettings.money -= building.preset.costPerTurn;
        }
    }

    void CalculateJobs()
    {
        _citySettings.curJobs = Mathf.Min(_citySettings.curPopulation, _citySettings.maxJobs);
    }

    public void Calculate(List<Building> buildings)
    {
        CalculateMoney(buildings);
        CalculateJobs();
        ResourceAmount resourceAmount = new ResourceAmount
        {
            money = _citySettings.money,
            jobs = _citySettings.curJobs,
            food = 0
        }; 
        EventBus.Publish(resourceAmount);
    }
}
