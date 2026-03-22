using UnityEngine;

public class FSM : MonoBehaviour
{
    protected virtual void Initialize() {}
    protected virtual void FSMUpdate() {}
    protected virtual void FSMFixedUpdate() {}

    // Use this for initialization
    void Start()
    {
        Initialize();
    }

    // Update is called once for frame
    void Update()
    {
        FSMUpdate();
    }

    void FixedUpdate()
    {
        FSMFixedUpdate();
    }
}

// Come utilizzare questa classe
/*
public class SimpleFSM : FSM
{
    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void FSMUpdate()
    {
        base.FSMUpdate();
    }

    protected override void FSMFixedUpdate()
    {
        base.FSMFixedUpdate();
    }
}
*/

/*
protected = visibile solo alla classe stessa e alle sue figlie.
public     → tutti possono accedere
private    → solo la classe stessa
protected  → la classe + chi eredita da essa
*/

