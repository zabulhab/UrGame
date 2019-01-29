using UnityEngine;

/// <summary>
/// The parent class for the online and offline statecontrollers
/// </summary>
public abstract class StateController : MonoBehaviour
{

    // Force the children to implement these

    public abstract void SwitchTurn();

    internal abstract Turn GetActiveTurn();

    internal abstract Turn GetInactiveTurn();

    protected abstract void SetReferenceInTiles();


}
