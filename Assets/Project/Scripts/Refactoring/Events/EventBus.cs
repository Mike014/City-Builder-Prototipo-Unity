using System;

public class EventBus 
{
    public static event Action<ResourceAmount> OnResourceUpdated;

    public static void Publish(ResourceAmount resourceAmount)
    {
        OnResourceUpdated?.Invoke(resourceAmount);
    }
}


