/// <summary>
/// This tile can hold two pieces of the same side
/// </summary>
public class TwoPieceTile : Tile 
{

    protected void Start()
    {
        base.Start("Two of your pieces can occupy this tile at a time.", 
                    TileType.TwoPiece, gameObject.name, 2);
    }

    internal override void ActivateTileFunction()
    {
        TryKickEnemyOut();
        stateController.GetActiveTurn().EndTurn();
    }
}
