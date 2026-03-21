using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacement : MonoBehaviour
{
    // --- Campi privati ---
    private bool _currentlyPlacing;
    private bool _currentlyBulldozing;
    private BuildingPreset _curBuildingPreset;
    private float _indicatorUpdateRate = .05f;
    private float _lastUpdateTime;
    private Vector3 _curIndicatorPos;
    private Stack<ICommand> _undoStack = new Stack<ICommand>();
    private Stack<ICommand> _redoStack = new Stack<ICommand>();

    // --- Campi pubblici ---
    public GameObject placementIndicator;
    public GameObject bulldozerIndicator;

    // --- Unity Lifecycle ---
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
        else if (Input.GetMouseButtonDown(0) && _currentlyBulldozing)
        {
            Bulldoze();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            UndoLastCommand();
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y))
        {
            RedoLastCommand();
        }
    }

    // --- Metodi pubblici ---
    public void BeginNewBuildingPlacement(BuildingPreset preset)
    {
        // check money
        _currentlyPlacing = true;
        _curBuildingPreset = preset;
        placementIndicator.SetActive(true);
        placementIndicator.transform.position = new Vector3(0, -99, 0);
    }

    public void CancelBuildingPlacement()
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

    public void PlaceBuilding()
    {
        // 1. Istanzia l'edificio
        // GameObject buildingObj = Instantiate(_curBuildingPreset.prefab, _curIndicatorPos, Quaternion.identity);
        // 2. Prendi il componente Building
        // Building building = buildingObj.GetComponent<Building>();
        // 3. Crea il command con i tre dati
        // Sezione 3 : La firma ora risulta sbagliata

        PlaceBuildingCommand placeBuildingCommand = new PlaceBuildingCommand(_curBuildingPreset, Vector3Int.RoundToInt(_curIndicatorPos));
        // Esegui
        placeBuildingCommand.Execute();
        // Push command to undo stack
        _undoStack.Push(placeBuildingCommand);
        _redoStack.Clear();
        Debug.Log($"Stack size: {_undoStack.Count}");

        CancelBuildingPlacement();
    }

    // --- Metodi privati ---
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

    private void Bulldoze()
    {
        Debug.Log("Bulldoze Method activated");
        Vector3Int targetPos = Vector3Int.RoundToInt(_curIndicatorPos);

        if (City.instance.grid.TryGetValue(targetPos, out Building building))
        {
            PlaceBuildingCommand command = new PlaceBuildingCommand(building.preset, targetPos);
            City.instance.OnRemoveBuilding(building);
            _redoStack.Push(command);
        }
    }

    private void UndoLastCommand()
    {
        if (_undoStack.Count > 0)
        {
            ICommand command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
            Debug.Log($"Stack size: {_undoStack.Count}");
        }
    }

    private void RedoLastCommand()
    {
        if (_redoStack.Count > 0)
        {
            ICommand command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
        }
    }
}

// NOTA ARCHITETTURALE: il bulldoze pusha direttamente nel _redoStack invece che nell'_undoStack.
// Questo rompe la semantica standard del Command Pattern (undo = annulla azione eseguita).
// Soluzione futura: separare lo stack del bulldoze da quello del piazzamento,
// o introdurre un BulldozeCommand dedicato con Execute() e Undo() invertiti.

