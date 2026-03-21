using UnityEngine;
// using System.Collections.Generic;

public class PlaceBuildingCommand : ICommand
{
    private BuildingPreset _prefab;
    private Vector3Int _posTile;

    public PlaceBuildingCommand(BuildingPreset prefab, Vector3Int posTile)
    {
        _prefab = prefab;
        _posTile = posTile;
        // _posTile = posTile;
        // _buildingObj = buildingObj;
    }

    public void Execute()
    {
        // Sezione 3  
        GameObject buildingObj = Object.Instantiate(_prefab.prefab, (Vector3)_posTile, Quaternion.identity);
        Building building = buildingObj.GetComponent<Building>();
        City.instance.OnPlaceBuilding(building);
    }

    public void Undo()
    {
        // Sezione 3  
        if (City.instance.grid.TryGetValue(_posTile, out Building building))
        {
            City.instance.OnRemoveBuilding(building);
        }
    }
}
