using UnityEngine;
using System.Collections.Generic;

public class EconomySystem : MonoBehaviour
{
    [SerializeField] private CitySettings _citySettings;

    void CalculateMoney(IEnumerable<Building> buildings)
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

    public void Calculate(IEnumerable<Building> buildings)
    {
        CalculateMoney(buildings);
        CalculateJobs();
    }
}