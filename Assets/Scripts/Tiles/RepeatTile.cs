/// <summary>
/// A tile that gives another turn to the player who landed on it
/// </summary>
public class RepeatTile : Tile
{
    // Intialize piece list and, for now, assign unique color
    protected void Start()
    {
        base.Start("This piece grants you another roll. Holds one piece.", 
                        TileType.Repeat, gameObject.name);
    }

    /// <summary>
    /// Activates the tile function; in this case, repeating a turn
    /// </summary>
    internal override void ActivateTileFunction()
    {
        TryKickEnemyOut();
        topMostPiece.GetAssociatedTurnObject().SetTurnRepeat();
    }

}
