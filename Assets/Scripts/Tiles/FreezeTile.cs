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
        setSummary("Enemy may only deploy new pieces on their next roll, shown with a light blue screen. Holds one piece.");
        TypeOfTile = TileType.Freeze;
        TileID = gameObject.name;
    }

    internal override void ActivateTileFunction()
    {
        TryKickEnemyOut();
        stateController.GetInactiveTurn().FreezeBoardPieces();
        stateController.GetActiveTurn().EndTurn();
    }
}
