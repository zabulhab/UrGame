using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The turn controller for the opponent
/// </summary>
public class EnemyTurn : Turn
{
    /// <summary>
    /// Set side name and other info if we are using this side
    /// </summary>
    internal override void TurnSetup()
    {
        turnSideName = SideName.EnemySide;
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
    /// Begins the enemy phase
    /// </summary>
    internal override void ActivatePhase()
    {
        if (isFrozen)
        {
            SetFreezePanelVisible(true);
        }

        rolledNumberText.SetActive(false); // TODO: make a real solution for this

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
        ActivatePhase();
    }
}
