using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnePieceTile : Tile {

    protected override void Start()
    {
        PiecesOnTop = new List<Piece>();
        maxNumberSamePiece = 1;
        setSummary("Only one of your pieces can occupy this tile at a time.");
        TypeOfTile = TileType.OnePiece;
        TileID = gameObject.name;
    }

    internal override void ActivateTileFunction()
    {
        TryKickEnemyOut();
        stateController.GetActiveTurn().EndTurn();
    }
}
