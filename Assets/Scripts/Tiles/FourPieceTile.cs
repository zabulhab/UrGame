using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FourPieceTile : Tile {

    protected override void Start()
    {
        PiecesOnTop = new List<Piece>();
        maxNumberSamePiece = 4;
        setSummary("Four of your pieces can occupy this tile at a time.");
        TypeOfTile = TileType.FourPiece;
        TileID = gameObject.name;
    }

    internal override void ActivateTileFunction()
    {
        TryKickEnemyOut();
        stateController.GetActiveTurn().EndTurn();
    }
}
