﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores a list and sublists of all tiles on the board
/// </summary>
public class GridSystem : MonoBehaviour 
{
    // The number of tiles that the piece can move on
    private static readonly int TILE_COUNT = 14;

    [SerializeField]
    private GameObject board;

    [SerializeField]
    private Tile[] allTiles = new Tile[20];

    [SerializeField]
    private Tile[] accessibleTilesPlayer = new Tile[TILE_COUNT];

    [SerializeField]
    private Tile[] accessibleTilesEnemy = new Tile[TILE_COUNT];

    private void Start()
    {
        for (int i = 0; i < TILE_COUNT; i++)
        {
            accessibleTilesEnemy[i].TileNumber = i;
        }
    }

   // private void OnValidate()
   // {
   //     if (accessibleTilesPlayer.Length != TILE_COUNT || 
   //         accessibleTilesEnemy.Length != TILE_COUNT)
   //     {
   //         Debug.LogError("The length of the Accessible Tile arrays must be 14!");
   //     }
   // }

   /// <summary>
   /// Takes in the desired index of a piece and returns the tile to move it to
   /// </summary>
   /// <returns>The tile object to land on.</returns>
   /// <param name="destTileIdx">Index of desired tile</param>
    internal Tile TileToLandOn(int destTileIdx, Turn.SideName playerName)
    {
        //Debug.Log(index + playerName);
        //Debug.Log(accessibleTilesEnemy);
        //Debug.Log(accessibleTilesPlayer);
        if (playerName == Turn.SideName.PlayerSide)
        {
            return accessibleTilesPlayer[destTileIdx];

        }
        else if (playerName == Turn.SideName.EnemySide)
        {
            return accessibleTilesEnemy[destTileIdx];
        }
        Debug.LogError("The phase names are wrong");
        return null;
    }

    ///// <summary>
    ///// Returns how many tiles one side can have access to.
    ///// Hard-coded to return 14 right now.
    ///// </summary>
    //internal int NumTilesAccessiblePerTurn()
    //{
    //    return 14;
    //}

    /// <summary>
    /// Prints a string that shows how many pieces 
    /// of each side a tile has on top of it, if any
    /// </summary>
    public void PrintBoardStatus()
    {
        int tileIdx = 0;
        Debug.Log("------Tile Statuses------");
        foreach (Tile tile in allTiles)
        {
            Dictionary<Turn.SideName, int> sidePieceCount = new Dictionary<Turn.SideName, int>();
            sidePieceCount.Add(Turn.SideName.EnemySide, 0);
            sidePieceCount.Add(Turn.SideName.PlayerSide, 0);
            Debug.Log("{T" + tileIdx + "}:");
            foreach (Piece piece in tile.PiecesOnTop)
            {
                Turn.SideName sidename = piece.SideName;
                if (sidename == Turn.SideName.PlayerSide)
                {
                    // add 1 to the current count of that side
                    sidePieceCount.Add(Turn.SideName.PlayerSide, 1);
                }
                else if (sidename == Turn.SideName.EnemySide)
                {
                    sidePieceCount.Add(Turn.SideName.EnemySide, 1);
                }
            }
            foreach (Turn.SideName turn in sidePieceCount.Keys)
            {
                if (sidePieceCount[turn] > 0)
                {
                    Debug.Log(sidePieceCount[turn]);
                }
            }


            tileIdx++;
        }
        Debug.Log("-----End Tile Statuses-----");
    }
    

    public void ReadBoardTest()
    {

    }
}
