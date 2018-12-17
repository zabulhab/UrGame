using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moves the piece that landed on it back to its 
/// starting position, with a 50-50 chance. 
/// TODO: Add rolling visualization to this chance.
/// </summary>
public class RestartTile : Tile 
{

    /// <summary>
    /// Used to make a time delay before kicking a piece back to start.
    /// Ends the turn here, as well, instead of outside of the coroutine.
    /// </summary>
    /// <returns>The and restart piece.</returns>
    private void RestartPieceAndEndTurn()
    {
        //yield return new WaitForSeconds(.5f);
        topMostPiece.KickBackToStart();
        this.RemovePiece(topMostPiece);
        stateController.GetActiveTurn().EndTurn();
    }

    protected override void Start()
    {
        PiecesOnTop = new List<Piece>();
        maxNumberSamePiece = 1;
        setSummary("You might go back to start with a 50% chance. Holds one piece.");
        TypeOfTile = TileType.Restart;
        TileID = gameObject.name;
    }

    internal override void ActivateTileFunction()
    {
        System.Random rand = new System.Random();
        // TODO: Wait for some lag time before teleporting back to start
        if (rand.Next(0, 2) != 0) // With 1/2 chance, move this piece to start
        {
            RestartPieceAndEndTurn();
        }
        else // if we land successfully, try kicking any enemies on top out
        {
            TryKickEnemyOut();
            stateController.GetActiveTurn().EndTurn();
        }
    }
}
