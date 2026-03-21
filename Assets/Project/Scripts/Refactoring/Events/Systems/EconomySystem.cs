using UnityEngine;
using System.Collections.Generic;

public class EconomySystem : MonoBehaviour
{
    void CalculateMoney(IEnumerable<Building> buildings, CityRuntimeState state, CityConfig config)
    {
        state.money += state.curJobs * config.incomePerJobs;

        foreach (Building building in buildings)
        {
            state.money -= building.preset.costPerTurn;
        }
    }

    void CalculateJobs(CityRuntimeState state)
    {
        state.curJobs = Mathf.Min(state.curPopulation, state.maxJobs);
    }

    public void Calculate(IEnumerable<Building> buildings, CityRuntimeState state, CityConfig config)
    {
        CalculateMoney(buildings, state, config);
        CalculateJobs(state);
    }
}