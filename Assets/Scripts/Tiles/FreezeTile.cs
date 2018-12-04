using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Prevents the other player from moving any pieces already on the board.
/// They may still move out any new pieces onto the board, if they can.
/// </summary>
public class FreezeTile : Tile {

    protected override void Start()
    {
        PiecesOnTop = new List<Piece>();
        maxNumberSamePiece = 1;
        setSummary("Enemy is unable to take action with on-field pieces for their next roll.");
        TypeOfTile = TileType.Freeze;
        TileID = gameObject.name;
    }

    internal override void ActivateTileFunction()
    {
        TryKickEnemyOut();
        //Debug.Log("FREEZING " + stateController.GetInactiveTurn().getSideName()+ "!");
        stateController.GetInactiveTurn().FreezeBoardPieces();
        stateController.GetActiveTurn().EndTurn();
    }
}
