using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // for sorting
using System;

public class AIController : Turn
{
    /// <summary>
    /// The coroutine for the time to wait before moving a piece, 
    /// so that this turn looks more "natural" to the naked eye
    /// </summary>
    private IEnumerator movePieceCoroutine;

    /// <summary>
    /// Set to true when the turn is allowed to repeat.
    /// Overrides the auto-ending done when a piece has landed on a repeat tile
    /// </summary>
    private bool turnEndDisabled = false;

    /// <summary>
    /// Reference to the player turn for AI decision-making purposes
    /// </summary>
    private Turn playerSide; 
    
    /// <summary>
    /// Set side name and other info if we are using this side
    /// </summary>
    internal override void TurnSetup()
    {
        Debug.Log("AI TURN STARTED");
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
    /// Specific to the AI Turn setup. Gives a reference to the player side,
    /// so that we can analyze its pieces for our decision-making logic.
    /// </summary>
    /// <param name="playerTurn">Player turn.</param>
    internal void AssignPlayerRef(PlayerTurn playerTurn)
    {
        playerSide = playerTurn;
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
        Dictionary<Piece, Tile> possibleTiles = GetPossibleTileDestinations(rolledNumber);
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
        return ChoosePieceUsingPriorities(pieceToTileMap);
        //return GetPieceFarthestOnBoard(pieceToTileMap);
    }

    /// <summary>
    /// Contains the main logic for choosing a piece to move while
    /// considering different factors related to the state of the game.
    /// </summary>
    /// <returns>The piece to move.</returns>
    /// <param name="pieceToTileMap">Piece to tile dictionary.</param>
    private Piece ChoosePieceUsingPriorities(Dictionary<Piece, Tile> pieceToTileMap)
    {
        Dictionary<Piece, Tile.TileType> pieceToTileTypeMap =
                                         new Dictionary<Piece, Tile.TileType>();
        foreach (Piece piece in pieceToTileMap.Keys)
        {
            Tile.TileType tileName = pieceToTileMap[piece].TypeOfTile;
            pieceToTileTypeMap.Add(piece, tileName);
        }
        // Get the pieces and their tile indexes, ordered highest to lowest
        IOrderedEnumerable < KeyValuePair<Piece, int> > piecesDescendingDistList = 
                                        GetPiecesAndDistDescList(pieceToTileMap);
        Piece farthestPiece = piecesDescendingDistList.ElementAt(0).Key; // last piece

        Dictionary<Piece, List<Piece>> killablePlayerPcs = GetPieceAndKillablePiecesDict();
        Dictionary<Piece, List<Piece>> killableAIPcs = playerSide.GetPieceAndKillablePiecesDict();

        int CPUBoardVal = GetSideValue();
        int playerBoardVal = playerSide.GetSideValue();

        // make <tileType, piece[]> dictionary that has null for pieces, 
        // unless a piece will be able to land on that tile type
        // if multiple pieces will be able to land on that tile type, 
        // then check how close player piece is

        Piece repeatPiece = null; //return from this loop
        foreach (KeyValuePair<Piece, int> pair in piecesDescendingDistList)
        {
            Piece curPiece = pair.Key;
            Tile.TileType tileType = pieceToTileTypeMap[curPiece];

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
                    repeatPiece = curPiece; // ideal piece is this
                    break;
                case Tile.TileType.Restart:
                    // Least likely
                    break;
            }
        }

        if (repeatPiece != null)
        {
            return repeatPiece;
        }
        else
        {
            //if (farthestPiece.GetTargetTile(rolledNumber).TypeOfTile==Tile.TileType.Restart)
            //{
            //    return second-farthest number in list
            //}
            return farthestPiece;
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
                if (piece.GetPieceStatus() == Piece.PieceStatus.Undeployed)
                {
                    tileDict.Add(piece, piece.GetTargetTile(rolledNum));
                }
            }
        }
        return tileDict;

    }

    /// <summary>
    /// Returns the list of pieces sorted from farthest to least farthest on board
    /// </summary>
    /// <returns>The piece farthest on board.</returns>
    /// <param name="pieceToTileMap">Piece to tile destinations map.</param>
    private IOrderedEnumerable<KeyValuePair<Piece,int>> GetPiecesAndDistDescList
                                        (Dictionary<Piece, Tile> pieceToTileMap)
    {
        List<KeyValuePair<Piece, int>> pieceDistanceList =
            new List<KeyValuePair<Piece, int>>();
        foreach (KeyValuePair<Piece, Tile> entry in pieceToTileMap)
        {
            int pieceTileIdx = entry.Key.CurrentTileIdx;
            KeyValuePair<Piece, int> pair =
                            new KeyValuePair<Piece, int>(entry.Key, pieceTileIdx);
            pieceDistanceList.Add(pair);
        }

        var orderedDistList = pieceDistanceList.OrderByDescending(x => x.Value); // descending order

        // TODO: check here if best piece lands on restart tile
        return orderedDistList;
    }

    /// <summary>
    /// Disables the turn ending for this turn
    /// </summary>
    internal override void SetTurnRepeat()
    {
        Debug.Log("JHH");
        turnEndDisabled = true;
    }
}
