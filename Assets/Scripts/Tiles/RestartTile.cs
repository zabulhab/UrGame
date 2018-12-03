using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moves the piece that landed on it back to its 
/// starting position, with a 50-50 chance. 
/// TODO: Add rolling visualization to this chance.
/// </summary>
public class RestartTile : Tile {

    protected override void Start()
    {
        piecesOnTop = new List<Piece>();
        maxNumberSamePiece = 1;
        setSummary("You might go back to start with a 50% chance. Holds one piece.");
        TypeOfTile = TileType.Restart;
    }

    internal override void ActivateTileFunction()
    {
        // With 1/4 chance (roll dice?)
        // TODO: Wait an amount of time to make the transition between
        // landing on and leaving the space smooth
        topMostPiece.KickBackToStart();
        stateController.GetActiveTurn().EndTurn();
    }
}
