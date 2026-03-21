
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
        // Se il cibo non basta, la popolazione cala di 1 per turno (carestia graduale)
        else if (_citySettings.curFood < _citySettings.curPopulation)
        {
            _citySettings.curPopulation = Mathf.Max(0, _citySettings.curPopulation - 1);
        }
    }

    // Ricalcola da zero la disponibilità totale di cibo sommando l'output di tutti gli edifici.
    void CalculateFood(IEnumerable<Building> buildings)
    {
        _citySettings.curFood = 0;

        foreach (Building building in buildings)
        {
            IResourceSource source = building.GetComponent<IResourceSource>();
            if (source != null && source.TryProvideResource(out int amount))
            {
                _citySettings.curFood += amount;
            }
        }
    }

    public void Calculate(IEnumerable<Building> buildings)
    {
        CalculateFood(buildings);
        CalculatePopulation();
    }
    
    // TODO: distribuzione attualmente senza impatto sul gameplay.
    // Preparatoria per il sistema di agenti — gli NPC preleveranno
    // cibo dalle IResourceSink invece di leggere da CitySettings.
    // private void DistributeFood(IEnumerable<Building> buildings)
    // {
    //     foreach (Building building in buildings)
    //     {
    //         IResourceSink sink = building.GetComponent<IResourceSink>();
    //         if (sink != null && _citySettings.curFood > 0)
    //         {
    //             sink.ReceiveResource(_citySettings.curFood);
    //         }
    //     }
    // }
}