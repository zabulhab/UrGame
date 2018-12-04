using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalTile : Tile 
{
    protected override void Start()
    {
        PiecesOnTop = new List<Piece>();
    }

    internal override void ActivateTileFunction()
    {
        stateController.GetActiveTurn().EndTurn();
    }

}
