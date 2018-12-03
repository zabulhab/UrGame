using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The turn controller for the opponent
/// </summary>
public class EnemyTurn : Turn
{
    // Set the side name for each piece
    protected override void Start()
    {
        // TODO: Make a new method to avoid code duplication in 2 turn objects
        turnSideName = SideName.EnemySide;
        int i = 0;
        foreach (Piece piece in allPieces)
        {
            piece.SetSideName(turnSideName);
            piece.SetAssociatedTurnObject(this);

            // store start location and index of each piece
            pieceStartLocations.Add(piece.transform.position);
            piece.SetStartIndex(i);
            i++;
        }
    }

    /// <summary>
    /// Begins the enemy phase
    /// </summary>
    internal override void ActivatePhase()
    {
        rolledNumberText.SetActive(false); // TODO: make a real solution for this
        //Debug.Log("Enemy phase activated");

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
