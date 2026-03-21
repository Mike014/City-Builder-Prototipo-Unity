using UnityEngine;

public class HouseBuilding : Building, IResourceSink
{
    // TODO: _storeFood non è ancora collegato alla logica di popolazione.
    // Sarà utilizzato dal sistema di agenti per simulare il consumo
    // individuale di cibo per residente.
    private int _storeFood;

    void Awake()
    {
        _storeFood = preset.food; // valore negativo da preset (-2)
    }

    public void ReceiveResource(int amount)
    {
        _storeFood += amount;
    }
}


// una casa non accumula cibo, accumula popolazione o consuma cibo per sfamare i residenti.
// TODO: _storeFood non è ancora collegato alla logica di popolazione.
// Sarà utilizzato dal sistema di agenti per simulare il consumo
// individuale di cibo per residente.