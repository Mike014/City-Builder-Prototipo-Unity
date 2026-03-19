using UnityEngine;

[CreateAssetMenu(fileName = "Building Preset", menuName = "New Building Preset")]
public class BuildingPreset : ScriptableObject
{
    // Costo una tantum per piazzare l'edificio
    public int cost;

    // Costo ricorrente per mantenere l'edificio attivo (ogni turno)
    public int costPerTurn;

    // Il prefab 3D da istanziare nella scena quando l'edificio viene piazzato
    public GameObject prefab;

    // Contributo alla popolazione totale della città (positivo = residenti)
    public int population;

    // Contributo ai posti di lavoro totali della città (positivo = lavoro disponibile)
    public int jobs;

    // Contributo al cibo totale della città (positivo = produce, negativo = consuma)
    public int food;
}