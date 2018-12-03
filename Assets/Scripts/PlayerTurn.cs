using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The turn controller for the player
/// </summary>
public class PlayerTurn :  Turn
{
    // Set the side name for each piece
    protected override void Start()
    {
        turnSideName = SideName.PlayerSide;
        int i = 0;
        foreach (Piece piece in allPieces)
        {
            piece.SideName = turnSideName;
            piece.SetAssociatedTurnObject(this);

            // store start location and index of each piece
            pieceStartLocations.Add(piece.transform.position);
            piece.SetStartIndex(i);
            i++;
        }
    }

    /// <summary>
    /// Begins the player phase
    /// </summary>
    internal override void ActivatePhase()
    {
        rolledNumberText.SetActive(false);
        //Debug.Log("Player phase activated");
        if (!AreAllPiecesFrozen() && PreRollOpenSpacesAvailable())
        {
            OpenRollUI();
        }
        else
        {
            EndTurn();
        }
    }

    internal override void SetTurnRepeat()
    {
        OpenRollUI();
    }
}
