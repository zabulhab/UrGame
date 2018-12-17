using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A tile that gives another turn to the player who landed on it
/// </summary>
public class RepeatTile : Tile
{
    // Intialize piece list and, for now, assign unique color
    protected override void Start()
    {
        PiecesOnTop = new List<Piece>();
        maxNumberSamePiece = 1;
        setSummary("This piece grants you another roll. Holds one piece.");
        TypeOfTile = TileType.Repeat;
        TileID = gameObject.name;
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
