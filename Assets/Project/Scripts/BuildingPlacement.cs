using UnityEngine;

public class BuildingPlacement : MonoBehaviour
{
    private bool _currentlyPlacing;
    private bool _currentlyBulldozing;

    private BuildingPreset _curBuildingPreset;

    private float _indicatorUpdateRate = .05f;
    private float _lastUpdateTime;
    private Vector3 _curIndicatorPos;

    public GameObject placementIndicator;
    public GameObject bulldozerIndicator;

    void Update()
    {
        if (_currentlyPlacing)
        {
            PressCancelBuildingPlacement();
        }

        PlacementIndicator();

        if (Input.GetMouseButtonDown(0) && _currentlyPlacing)
        {
            PlaceBuilding();
        }
        else if ((Input.GetMouseButtonDown(0) && _currentlyBulldozing))
        {
            Bulldoze();
        }
    }

    public void BeginNewBuildingPlacement(BuildingPreset preset)
    {
        // check money
        _currentlyPlacing = true;
        _curBuildingPreset = preset;
        placementIndicator.SetActive(true);
        placementIndicator.transform.position = new Vector3(0, -99, 0);
    }

    void CancelBuildingPlacement()
    {
        _currentlyPlacing = false;
        placementIndicator.SetActive(false);
    }

    public void ToggleBulldoze()
    {
        _currentlyBulldozing = !_currentlyBulldozing;
        bulldozerIndicator.SetActive(_currentlyBulldozing);
        bulldozerIndicator.transform.position = new Vector3(0, -99, 0);
    }

    void PlaceBuilding()
    {
        GameObject buildingObj = Instantiate(_curBuildingPreset.prefab, _curIndicatorPos, Quaternion.identity);
        City.instance.OnPlaceBuilding(buildingObj.GetComponent<Building>());
        CancelBuildingPlacement();
    }

    private void PressCancelBuildingPlacement()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelBuildingPlacement();
        }
    }

    private void PlacementIndicator()
    {
        // called every 0.05 seconds
        if (Time.time - _lastUpdateTime > _indicatorUpdateRate)
        {
            _lastUpdateTime = Time.time;
            // get the currently selected tile position
            _curIndicatorPos = Selector.instance.GetCurTilePosition();
            // move the placement indicator or bulldoze indicator to the selected tile
            if (_currentlyPlacing)
                placementIndicator.transform.position = _curIndicatorPos;
            else if (_currentlyBulldozing)
                bulldozerIndicator.transform.position = _curIndicatorPos;
        }
    }

    void Bulldoze()
    {
        Building buildingToDestroy = City.instance.buildings.Find(x => x.transform.position == _curIndicatorPos);

        if (buildingToDestroy != null)
        {
            City.instance.OnRemoveBuilding(buildingToDestroy);
        }
    }
}

