/// <summary>
/// Prevents the other player from moving any pieces already on the board.
/// They may still move out any new pieces onto the board, if they can.
/// </summary>
public class FreezeTile : Tile 
{

    protected void Start()
    {
        base.Start("Enemy may only deploy new pieces on their next roll, " +
            "shown with a light blue screen. Holds one piece.",
            TileType.Freeze, gameObject.name);
    }

    internal override void ActivateTileFunction()
    {
        TryKickEnemyOut();
        stateController.FreezeInactiveTurn();
        stateController.EndActiveTurn();
    }
}
