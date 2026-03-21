using UnityEngine;

public class BulldozeCommand : ICommand
{
    private BuildingPreset _preset;
    private Vector3Int _posTile;

    public BulldozeCommand(BuildingPreset preset, Vector3Int posTile)
    {
        _preset = preset;
        _posTile = posTile;
    }

    // Esegui = demolisci l'edificio
    public void Execute()
    {
        if (City.instance.grid.TryGetValue(_posTile, out Building building))
        {
            City.instance.OnRemoveBuilding(building);
        }
    }

    // Undo = ricostruisci l'edificio demolito
    public void Undo()
    {
        GameObject buildingObj = Object.Instantiate(_preset.prefab, (Vector3)_posTile, Quaternion.identity);
        Building building = buildingObj.GetComponent<Building>();
        City.instance.OnPlaceBuilding(building);
    }
}
