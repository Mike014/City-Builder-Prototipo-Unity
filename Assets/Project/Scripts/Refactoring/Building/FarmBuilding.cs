public class FarmBuilding : Building, IResourceSource
{

    private int _storeFood;

    void Awake()
    {
        _storeFood = preset.food;
    }

    public bool TryProvideResource(out int amount)
    {
        if (_storeFood > 0)
        {
            amount = _storeFood;
            _storeFood = preset.food;
            return true;
        }

        amount = 0;
        return false;
    }
}


// La Farm restituisce risorse, in questo caso cibo