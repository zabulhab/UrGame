using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // for sorting
using System;

public class AIController : Turn 
{
    [SerializeField]
    private GridSystem grid;

    // The amount of time to wait before moving a piece, 
    // so this turn looks more "natural"
    private IEnumerator movePieceCoroutine;

    // Set to true when the turn is allowed to repeat.
    // Overrides the auto-ending done when a piece has landed on a repeat tile
    private bool turnEndDisabled = false;

    // Set the side name for each piece
    protected override void Start()
    {
        // TODO: Make a new method to avoid code duplication in 2 turn objects
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

        movePieceCoroutine = WaitAndTryMovePiece();
    }

    /// <summary>
    /// Used by the movePieceCouroutine. Waits before moving a piece.
    /// </summary>
    /// <returns>The to move piece.</returns>
    private IEnumerator WaitAndTryMovePiece()
    {
        if (!AreAllPiecesFrozen() && PreRollOpenSpacesAvailable())
        {
            RollDice();
            yield return new WaitForSeconds(2f);
            if (rolledNumber != 0)
            {
                MakeIdealMove();  
            }
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }
        if (!turnEndDisabled)
        {
            EndTurn(true);
        }
        else
        {
            ActivatePhase();
        }
    }

    internal override void ActivatePhase()
    {
        DisableTurnStartPanel();
        rolledNumberText.SetActive(false); // TODO: make an elegant solution for this
        // If turn end disabled last time because of repeat tile, enable ending again
        if (turnEndDisabled == true) 
        {
            turnEndDisabled = false;
        }
        StartCoroutine(WaitAndTryMovePiece());

    }

    /// <summary>
    /// Makes the ideal move, or no move, if none are possible.
    /// </summary>
    private void MakeIdealMove()
    {
        Piece piece = GetIdealPieceToMove();
        if (piece != null)
        {
            piece.SetNumSpacesToMove(rolledNumber);
            piece.MoveToTargetTile();
        }
    }

    /// <summary>
    /// Checks the state of the board and chooses the best move.
    /// For now, it doesn't take the opponent's actions into consideration.
    /// </summary>
    private Piece GetIdealPieceToMove()
    {
        Dictionary<Piece,Tile> possibleTiles = GetPossibleTileDestinations(rolledNumber);
        if (possibleTiles.Count == 0)
        {
            return null; // can't make any moves
        }
        else
        {
            Piece idealPiece = GetOptimalPiece(possibleTiles);
            return idealPiece;
        }
    }

    /// <summary>
    /// Helper method that returns a piece to move after considering the whole board
    /// </summary>
    /// <returns>The optimal piece.</returns>
    private Piece GetOptimalPiece(Dictionary<Piece, Tile> pieceToTileMap)
    {
        int CPUBoardVal = GetSideValue();
        grid.WriteBoardStatusToFile();
        // call ChoosePieceUsingPriorities(pieceToTileMap)
        return GetPieceFarthestOnBoard(pieceToTileMap);
    }

    private Piece ChoosePieceUsingPriorities(Dictionary<Piece, Tile> pieceToTileMap)
    {
        Dictionary<Piece, Tile.TileType> pieceToTileTypeMap = 
                                         new Dictionary<Piece, Tile.TileType>();
        foreach (Piece piece in pieceToTileMap.Keys)
        {
            Tile.TileType tileName = pieceToTileMap[piece].TypeOfTile;
            pieceToTileTypeMap.Add(piece, tileName);
        }

        Piece idealPiece; //return from this loop
        foreach (Piece piece in pieceToTileTypeMap.Keys)
        {
            Tile.TileType tileType = pieceToTileTypeMap[piece];

            switch (tileType)
            {
                // Kick out player piece #1 priority if player winning
                // check how likely piece if can be killed after
                // Order risky to least risky behavior?
                // Weigh risks when 
                // TODO: make & call GetBoardValuePlayerSide
                case Tile.TileType.OnePiece:
                    break;
                case Tile.TileType.TwoPiece:
                    break;
                case Tile.TileType.FourPiece:
                    break;
                case Tile.TileType.Freeze:
                    // 2nd Most likely
                    break;
                case Tile.TileType.Repeat:
                    // Most likely
                    break;
                case Tile.TileType.Restart:
                    // Least likely
                    break;
            }
        }

        // make a list of pieces to move
        // get potential board value for each, by adding to current one
        // order by highest to lowest move value
        // make category for each move?

        // if no pieces in shared middle section:
        // a move has the highest value if it lands on a repeat tile
        // if pieces in shared middle section:
        // if within 3 spaces of a player piece, prioritize moving it
        // pieces in the finish strip have a lower priority

        // if the CPUBoardVal is significantly higher, there is a lower chance of taking risks
        // less likely to move pieces to be near player piece, eg jumping 
        // on a piece to kill it then being in a vulnerable spot
        // player pieces on board being frozen next turn has higher value
        // the further the player pieces are on the board
        // prioritize freezing over repeat tile when the CPUBoardVal is lower
        return null;
    }
   

    /// <summary>
    /// Returns the key-value pair map of the pieces and 
    /// the tiles that they can move to
    /// </summary>
    /// <returns>The possible tile destinations.</returns>
    /// <param name="rolledNum">Rolled number.</param>
    private Dictionary<Piece, Tile> GetPossibleTileDestinations(int rolledNum)
    {
        Dictionary<Piece, Tile> tileDict = new Dictionary<Piece, Tile>();


        foreach (Piece piece in allPieces)
        {
            if (piece.GetPieceStatus() == Piece.PieceStatus.Finished)
            {
                continue;
            }
            if (!CheckPieceCanMoveToTile(piece))
            {
                //Debug.Log("Cannot move to tile");
                continue;
            }
            if (!isFrozen)
            {
                tileDict.Add(piece, piece.GetTargetTile(rolledNum));
            }
            else // it is frozen
            {
                if (piece.GetPieceStatus()==Piece.PieceStatus.Undeployed)
                {
                    tileDict.Add(piece, piece.GetTargetTile(rolledNum));
                }
            }
        }
        return tileDict;

    }

    /// <summary>
    /// Returns the piece farthest forward on the board, if applicable.
    /// </summary>
    /// <returns>The piece farthest on board.</returns>
    /// <param name="pieceToTileMap">Piece to tile destinations map.</param>
    private Piece GetPieceFarthestOnBoard(Dictionary<Piece, Tile> pieceToTileMap)
    {
        List<KeyValuePair<Piece, int>> pieceDistanceList = 
            new List<KeyValuePair<Piece, int>>();
        foreach (KeyValuePair<Piece, Tile> entry in pieceToTileMap)
        {
            int pieceTileIdx = entry.Key.CurrentTileIdx;
            KeyValuePair<Piece, int> pair = 
                            new KeyValuePair<Piece, int>(entry.Key,pieceTileIdx);
            pieceDistanceList.Add(pair);
        }

        var orderedDistList = pieceDistanceList.OrderByDescending(x => x.Value); // descending order
        foreach (KeyValuePair<Piece, int> entry in pieceDistanceList)
        {
            Debug.Log("ENTRY: " + entry);
        }
        Piece piece = orderedDistList.ElementAt(0).Key; // last piece
        // TODO: check here if best piece lands on restart tile
        return piece;
    }

    /// <summary>
    /// Disables the turn ending for this turn
    /// </summary>
    internal override void SetTurnRepeat()
    {
        turnEndDisabled = true;
    }
}
