using UnityEngine;

public class PlaceBuildingCommand : ICommand
{
    private BuildingPreset _prefab;
    private Vector3 _posTile;
    private Building _buildingObj;

    public PlaceBuildingCommand(BuildingPreset prefab, Vector3 posTile, Building buildingObj)
    {
        _prefab = prefab;
        _posTile = posTile;
        _buildingObj = buildingObj;
    }

    public void Execute()
    {
        City.instance.OnPlaceBuilding(_buildingObj);
    }

    public void Undo()
    {
        City.instance.OnRemoveBuilding(_buildingObj);
    }
}


