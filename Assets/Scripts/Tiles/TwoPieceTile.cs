using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoPieceTile : Tile {

    protected override void Start()
    {
        PiecesOnTop = new List<Piece>();
        maxNumberSamePiece = 2;
        setSummary("Two of your pieces can occupy this tile at a time.");
        TypeOfTile = TileType.TwoPiece;
        TileID = gameObject.name;
    }

    internal override void ActivateTileFunction()
    {
        TryKickEnemyOut();
        stateController.GetActiveTurn().EndTurn();
    }
}
