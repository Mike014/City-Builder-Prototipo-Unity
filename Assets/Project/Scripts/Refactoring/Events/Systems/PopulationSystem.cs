
using UnityEngine;
using System.Collections.Generic;

public class PopulationSystem : MonoBehaviour
{
    // Aggiorna la popolazione in base alla disponibilità di cibo, simulando crescita o decrescita.
    void CalculatePopulation(CityRuntimeState state)
    {
        // Se c'è abbastanza cibo per sfamare tutti e c'è spazio abitativo
        if (state.curFood >= state.curPopulation && state.curPopulation < state.maxPopulation)
        {
            state.curFood -= state.curPopulation / 4;
            state.curPopulation = Mathf.Min(state.curPopulation + (state.curFood / 4), state.maxPopulation);
        }
        // Se il cibo non basta, la popolazione cala di 1 per turno (carestia graduale)
        else if (state.curFood < state.curPopulation)
        {
            state.curPopulation = Mathf.Max(0, state.curPopulation - 1);
        }
    }

    // Ricalcola da zero la disponibilità totale di cibo sommando l'output di tutti gli edifici.
    void CalculateFood(IEnumerable<Building> buildings, CityRuntimeState state)
    {
        state.curFood = 0;

        foreach (Building building in buildings)
        {
            IResourceSource source = building.GetComponent<IResourceSource>();
            if (source != null && source.TryProvideResource(out int amount))
            {
                state.curFood += amount;
            }
        }
    }

    public void Calculate(IEnumerable<Building> buildings, CityRuntimeState state, CityConfig config)
    {
        CalculateFood(buildings, state);
        CalculatePopulation(state);
    }
}
    