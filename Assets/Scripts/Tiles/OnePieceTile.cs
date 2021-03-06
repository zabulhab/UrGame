﻿/// <summary>
/// This tile can only hold one piece at a time
/// </summary>
public class OnePieceTile : Tile 
{

    protected void Start()
    {
        base.Start("Only one of your pieces can occupy this tile at a time.", TileType.OnePiece, gameObject.name);
    }

    internal override void ActivateTileFunction()
    {
        TryKickEnemyOut();
        stateController.EndActiveTurn();
    }
}
