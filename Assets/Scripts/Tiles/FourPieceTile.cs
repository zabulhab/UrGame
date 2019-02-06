/// <summary>
/// This tile can hold four pieces of the same side
/// </summary>
public class FourPieceTile : Tile 
{

    protected void Start()
    {
        base.Start("Four of your pieces can occupy this tile at a time.", 
                    TileType.FourPiece, gameObject.name, 4);
    }

    internal override void ActivateTileFunction()
    {
        TryKickEnemyOut();
        stateController.EndActiveTurn();
    }
}
