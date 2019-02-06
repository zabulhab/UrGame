using UnityEngine;

/// <summary>
/// The parent class for the online and offline statecontrollers.
/// Manages the turn flow and can return info about a given side.
/// </summary>
public abstract class StateController : MonoBehaviour
{

    // Force the children to implement these

    public abstract void SwitchTurn();

    internal abstract void EndActiveTurn();

    internal abstract Turn.SideName GetActiveTurnSideName();

    internal abstract bool IsActiveTurnInPieceSelectionPhase();

    internal abstract void FreezeInactiveTurn();

    protected abstract void SetReferenceInTiles();

   
}
