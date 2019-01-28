
/// <summary>
/// The parent class for the online and offline statecontrollers
/// </summary>
public abstract class StateController
{
    // Force the children to implement this
    protected abstract void SwitchTurn();

    internal abstract Turn GetActiveTurn();

    internal abstract Turn GetInactiveTurn();

}
