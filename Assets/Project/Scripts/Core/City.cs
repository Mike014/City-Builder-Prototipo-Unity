using System.Collections.Generic;
using UnityEngine;
using TMPro;

// TODO: Undo non ripristina il denaro — OnRemoveBuilding non restituisce il costo dell'edificio

public class City : MonoBehaviour
{
    public TextMeshProUGUI statsText;

    public List<Building> buildings = new List<Building>();

    [SerializeField] private CitySettings _citySettings;

    // Singleton
    public static City instance;

    void Awake()
    {
        instance = this;
        UpdateStatText();
    }

    // void Start()
    // {
    //     UpdateStatText();
    // }

    public void OnPlaceBuilding(Building building)
    {
        _citySettings.money -= building.preset.cost;
        _citySettings.maxPopulation += building.preset.population;
        _citySettings.maxJobs += building.preset.jobs;
        buildings.Add(building);
        UpdateStatText();
    }

    public void OnRemoveBuilding(Building building)
    {
        _citySettings.maxPopulation -= building.preset.population;
        _citySettings.maxJobs -= building.preset.jobs;
        buildings.Remove(building);

        Destroy(building.gameObject);
        UpdateStatText();
    }

    void UpdateStatText()
    {
        statsText.text = string.Format("Day: {0}   Money: ${1}   Pop: {2} / {3}   Jobs : {4} / {5}   Food: {6}", new object[7] { _citySettings.day, _citySettings.money, _citySettings.curPopulation, _citySettings.maxPopulation, _citySettings.curJobs, _citySettings.maxJobs, _citySettings.curFood});
    }

    public void EndTurn()
    {
        _citySettings.day++;
        CalculateMoney();
        CalculatePopulation();
        CalculateJobs();
        CalculateFood();

        UpdateStatText();
    }

    void CalculateMoney()
    {
        _citySettings.money += _citySettings.curJobs * _citySettings.incomePerJobs;
        foreach(Building building in buildings)
            _citySettings.money -= building.preset.costPerTurn;
    }

    void CalculatePopulation()
    {
        if (_citySettings.curFood >= _citySettings.curPopulation && _citySettings.curPopulation < _citySettings.maxPopulation)
        {
            _citySettings.curFood -= _citySettings.curPopulation / 4;
            _citySettings.curPopulation = Mathf.Min(_citySettings.curPopulation + (_citySettings.curFood / 4 ), _citySettings.maxPopulation);
        }
        else if(_citySettings.curFood < _citySettings.curPopulation)
        {
            _citySettings.curPopulation = _citySettings.curFood;
        }
    }

    void CalculateJobs()
    {
        _citySettings.curJobs = Mathf.Min(_citySettings.curPopulation, _citySettings.maxJobs);
    }

    void CalculateFood()
    {
        _citySettings.curFood = 0;
        
        foreach(Building building in buildings)
        {
            _citySettings.curFood += building.preset.food;
        }
    }

/*
    void ResetToDefaults()
    {
        // 
    }
*/
}
