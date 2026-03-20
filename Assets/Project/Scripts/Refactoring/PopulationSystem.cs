
using UnityEngine;
using System.Collections.Generic;

public class PopulationSystem : MonoBehaviour
{
    // Riferimento allo ScriptableObject che funge da database dei dati correnti.
    [SerializeField] private CitySettings _citySettings;

    // Aggiorna la popolazione in base alla disponibilità di cibo, simulando crescita o decrescita.
    void CalculatePopulation()
    {
        // Se c'è abbastanza cibo per sfamare tutti e c'è spazio abitativo
        if (_citySettings.curFood >= _citySettings.curPopulation && _citySettings.curPopulation < _citySettings.maxPopulation)
        {
            _citySettings.curFood -= _citySettings.curPopulation / 4;
            _citySettings.curPopulation = Mathf.Min(_citySettings.curPopulation + (_citySettings.curFood / 4), _citySettings.maxPopulation);
        }
        // Se il cibo non basta, la popolazione crolla istantaneamente al livello del cibo disponibile
        else if (_citySettings.curFood < _citySettings.curPopulation)
        {
            _citySettings.curPopulation = _citySettings.curFood;
        }
    }

    // Ricalcola da zero la disponibilità totale di cibo sommando l'output di tutti gli edifici.
    void CalculateFood(List<Building> buildings)
    {
        _citySettings.curFood = 0;

        foreach (Building building in buildings)
        {
            _citySettings.curFood += building.preset.food;
        }
    }

    public void Calculate(List<Building> buildings)
    {
        CalculateFood(buildings);
        CalculatePopulation();
        ResourceAmount resourceAmount = new ResourceAmount
        {
            food = _citySettings.curFood,
            population = _citySettings.curPopulation,
        };
        EventBus.Publish(resourceAmount);
    }
}