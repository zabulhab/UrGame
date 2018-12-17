using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // for sorting
using System;

public class AIController : Turn
{
    /// <summary>
    /// Set to true when the turn is allowed to repeat.
    /// Overrides the auto-ending done when a piece has landed on a repeat tile
    /// </summary>
    private bool turnEndDisabled = false;

    /// <summary>
    /// Passed in to end turn call to invoke turn end method
    /// so we can bypass needing to click the button
    /// </summary>
    [SerializeField]
    private StateController phaseController;

    /// <summary>
    /// Reference to the player turn for AI decision-making purposes
    /// </summary>
    private PlayerTurn playerSide;
    
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
            yield return new WaitForSeconds(.5f);
        }
        // End turn normally unless AI landed on a repeat tile
        if (!turnEndDisabled)
        {
            SetFreezePanelVisible(false);
            EndTurn(true, phaseController);
        }
        else
        {
            ActivatePhase();
        }
    }

    internal override void ActivatePhase()
    {
        if (isFrozen)
        {
            SetFreezePanelVisible(true);
        }
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
            ClickSFX.Play(0); // play move sound
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
    }

    /// <summary>
    /// Contains the main logic for choosing a piece to move while
    /// considering different factors related to the state of the game.
    /// </summary>
    /// <returns>The piece to move.</returns>
    /// <param name="pieceToTileMap">Piece to tile dictionary.</param>
    private Piece ChoosePieceUsingPriorities(Dictionary<Piece, Tile> pieceToTileMap)
    {
#region local variables setup

        // Get the pieces and their tile indices, ordered highest to lowest
        IOrderedEnumerable<KeyValuePair<Piece, int>> piecesDescendingDistList =
                                       GetPiecesAndDistDescList(pieceToTileMap);
        // For given AI pieces, get the pieces we might be able to kill next turn 
        Dictionary<Piece, List<Piece>> killablePlayerPcsNextTurn = 
                            GetPieceAndNextTurnKillablePiecesDict(rolledNumber);
        Dictionary<Piece, List<Piece>> KillablePlayerPcsThisTurn =
                            GetPieceAndThisTurnKillablePiecesDict(rolledNumber);
        List<Piece> killableAIPcsNextTurn = 
            playerSide.GetKillableAIPiecesNextTurn(pieceToTileMap);

        int CPUBoardVal = GetSideValue();
        int playerBoardVal = playerSide.GetSideValue();

        Dictionary<Piece, Tile.TileType> pieceToTileTypeMap =
                                         new Dictionary<Piece, Tile.TileType>();
        foreach (Piece piece in pieceToTileMap.Keys)
        {
            Tile.TileType tileName = pieceToTileMap[piece].TypeOfTile;
            pieceToTileTypeMap.Add(piece, tileName);
        }
#endregion local variables setup
        // make <tileType, piece[]> dictionary that has null for pieces, 
        // unless a piece will be able to land on that tile type
        // if multiple pieces will be able to land on that tile type, 
        // then check how close player piece is

        // Default best piece is the one that is farthest ahead on the board
        Piece bestPiece = piecesDescendingDistList.ElementAt(0).Key;
        List<bool> killableStatsCurBestPiece = AIHelperChecks.GetPieceKillableStats(bestPiece,
                         KillablePlayerPcsThisTurn,
                         killablePlayerPcsNextTurn,
                         killableAIPcsNextTurn);
        // Finding a new best piece is similar to 1 pass of a bubble sort

        foreach (KeyValuePair<Piece, int> pair in piecesDescendingDistList)
        {

            Piece curPiece = pair.Key;
            // skip over best piece because nothing to compare
            if (curPiece.Equals(bestPiece))
            {
                continue;
            }
            Tile.TileType tileType = pieceToTileTypeMap[curPiece];
            List<bool> killableStatsThisPiece = AIHelperChecks.GetPieceKillableStats(curPiece,
                                                 KillablePlayerPcsThisTurn,
                                                 killablePlayerPcsNextTurn,
                                                 killableAIPcsNextTurn);


            // if the piece is a better piece, update current best piece
            if (ComparePieceKillableStats(bestPiece, 
                                          killableStatsThisPiece[0], 
                                          killableStatsThisPiece[1],
                                          killableStatsThisPiece[2],
                                          killableStatsCurBestPiece[0],
                                          killableStatsCurBestPiece[1],
                                          killableStatsCurBestPiece[2]))
            {
                bestPiece = curPiece;
                killableStatsCurBestPiece = killableStatsThisPiece;
            }
            //switch (tileType)
            //{
            //    // Kick out player piece #1 priority if player winning
            //    // check how likely piece if can be killed after
            //    // Order risky to least risky behavior?
            //    // Weigh risks when 
            //    // TODO: make & call GetBoardValuePlayerSide
            //    case Tile.TileType.OnePiece:
            //        break;
            //    case Tile.TileType.TwoPiece:
            //        break;
            //    case Tile.TileType.FourPiece:
            //        break;
            //    case Tile.TileType.Freeze:
            //        // 2nd Most likely
            //        break;
            //    case Tile.TileType.Repeat:
            //        // Most likely
            //        bestPiece = curPiece;
            //        break;
            //    case Tile.TileType.Restart:
            //        // Least likely
            //        break;
            //}
        }

        return bestPiece;

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
    /// Helper method to check which combination of killable stats the current
    /// piece has as compared to the current best piece, and see if the current 
    /// piece is better than the current best piece
    /// </summary>
    /// <returns><c>true</c>, if piece killable bools was checked, <c>false</c> otherwise.</returns>
    /// <param name="curPcCanKillThisTurn">If set to <c>true</c> can kill this turn.</param>
    /// <param name="curPcCanKillNextTurn">If set to <c>true</c> can kill next turn.</param>
    /// <param name="curPcCanBeKilledNextTurn">If set to <c>true</c> can be killed next turn.</param>
    private bool ComparePieceKillableStats(Piece bestPiece, 
                                           bool curPcCanKillThisTurn, 
                                           bool curPcCanKillNextTurn,
                                           bool curPcCanBeKilledNextTurn,
                                           bool bestPcCanKillThisTurn,
                                           bool bestPcCanKillNextTurn,
                                           bool bestPcCanBeKilledNextTurn)
    {
        bool isBetterPiece = false;
        int priorityNum = 0;
        // can kill this turn and next turn, and might get killed
        if (curPcCanKillThisTurn && curPcCanKillNextTurn && curPcCanBeKilledNextTurn)
        {
            priorityNum = 1;
        }
        // can kill this turn and next turn, and won't get killed
        else if (curPcCanKillThisTurn && curPcCanKillNextTurn && !curPcCanBeKilledNextTurn) 
        {
            priorityNum = 3;
        }
        // can kill this turn, but not next turn, and might get killed
        else if (curPcCanKillThisTurn && !curPcCanKillNextTurn && curPcCanBeKilledNextTurn) 
        {
            priorityNum = 2;
        }
        // can kill this turn, but not next turn, and won't get killed
        else if (curPcCanKillThisTurn && !curPcCanKillNextTurn && !curPcCanBeKilledNextTurn) 
        {
            priorityNum = 4;
        }
        // can't kill this turn, but can kill next turn, and might get killed
        else if (!curPcCanKillThisTurn && curPcCanKillNextTurn && curPcCanBeKilledNextTurn) 
        {
            priorityNum = 6;
        }
        // can't kill this turn, but can kill next turn, and won't get killed
        else if (!curPcCanKillThisTurn && curPcCanKillNextTurn && !curPcCanBeKilledNextTurn) 
        {
            priorityNum = 7;
        }
        // can't kill anyone this or next turn but might get killed
        else if (!curPcCanKillThisTurn && !curPcCanKillNextTurn && curPcCanBeKilledNextTurn) 
        {
            priorityNum = 5;
        }
        // can't kill anyone this or next turn, and won't get killed either
        else if (!curPcCanKillThisTurn && !curPcCanKillNextTurn && !curPcCanBeKilledNextTurn) 
        {
            priorityNum = 8;
        }
        else
        {
            Debug.LogError("AI priority check failed!");
        }
        isBetterPiece = ComparePieceStatHelper(bestPcCanKillThisTurn,
           bestPcCanKillNextTurn,
           bestPcCanBeKilledNextTurn,
           priorityNum);
        return isBetterPiece;
    }

    /// <summary>
    /// Takes in the stats for the current best piece and decides based on the
    /// priority from the if-statement calling it, tile types, and 
    /// risk factor if the current piece is better than the current best piece
    /// </summary>
    /// <returns><c>true</c>, if better piece was checked, <c>false</c> otherwise.</returns>
    /// <param name="killThisTurn">If set to <c>true</c> kill this turn.</param>
    /// <param name="killNextTurn">If set to <c>true</c> kill next turn.</param>
    /// <param name="killedNextTurn">If set to <c>true</c> killed next turn.</param>
    /// <param name="callerStatsPriority">priority of the if statement calling this method</param>
    private bool ComparePieceStatHelper(bool killThisTurn, bool killNextTurn, bool killedNextTurn, int callerStatsPriority)
    {
        int priority = 0;
        if (killThisTurn && killNextTurn && killedNextTurn)
        {
            priority = 1;
        }
        else if (killThisTurn && killNextTurn && !killedNextTurn) 
        {
            priority = 3;
        }
        else if (killThisTurn && !killNextTurn && killedNextTurn) 
        {
            priority = 2;
        }
        else if (killThisTurn && !killNextTurn && !killedNextTurn) 
        {
            priority = 4;
        }
        else if (!killThisTurn && killNextTurn && killedNextTurn) 
        {
            priority = 6;
        }
        else if (!killThisTurn && killNextTurn && !killedNextTurn) 
        {
            priority = 7;
        }
        else if (!killThisTurn && !killNextTurn && killedNextTurn) 
        {
            priority = 5;
        }
        else if (!killThisTurn && !killNextTurn && !killedNextTurn) 
        {
            priority = 8;
        }
        else
        {
            Debug.LogError("AI priority check failed!");
        }
        return (callerStatsPriority < priority);
    }


    /// <summary>
    /// Returns the key-value pair dictionary of the
    /// pieces and the tiles that they can move to
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
            if (!piece.CheckPieceCanMoveToTile(rolledNum))
            {
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
    /// Returns a dictionary with the piece(s) that can kill a piece next turn, 
    /// checking which tile each AI piece will be on for that turn, 
    /// and the piece(s) that can be killed, provided the correct number is rolled. 
    /// </summary>
    /// <returns>The piece and killable pieces dict.</returns>
    /// <param name="numRolled">The number we have rolled this turn.</param>
    private Dictionary<Piece, List<Piece>> GetPieceAndNextTurnKillablePiecesDict(int numRolled)
    {
        Dictionary<Piece, List<Piece>> piecesToKillablePiecesMap =
                                            new Dictionary<Piece, List<Piece>>();
        // Check each piece for any pieces it could kill next turn
        foreach (Piece piece in allPieces)
        {
            // The piece and a list with the piece(s) it could kill
            KeyValuePair<Piece, List<Piece>> pieces = piece.GetPieceAndKillablePiecesNextTurn(numRolled);
            if (!pieces.Equals(default(KeyValuePair<Piece, List<Piece>>)))
            {
                piecesToKillablePiecesMap.Add(pieces.Key, pieces.Value);
            }
        }
        return piecesToKillablePiecesMap;
    }

    /// <summary>
    /// Returns a dictionary with the piece(s) that can kill a piece this turn, 
    /// checking which tile each AI piece would land on for this turn,
    /// and the piece(s) that can be killed.
    /// </summary>
    /// <returns>The piece and killable pieces dict.</returns>
    /// <param name="numRolled">The number we have rolled this turn.</param>
    private Dictionary<Piece, List<Piece>> GetPieceAndThisTurnKillablePiecesDict(int numRolled)
    {
        Dictionary<Piece, List<Piece>> piecesToKillablePiecesMap =
                                            new Dictionary<Piece, List<Piece>>();
        // Check each piece for any pieces it could kill next turn
        foreach (Piece piece in allPieces)
        {
            // The piece and a list with the piece(s) it could kill
            KeyValuePair<Piece, List<Piece>> pieces = piece.GetPieceAndKillablePiecesThisTurn(numRolled);
            if (pieces.Value.Count!=0)
            {
                piecesToKillablePiecesMap.Add(pieces.Key, pieces.Value);
            }
        }
        return piecesToKillablePiecesMap;
    }

    /// <summary>
    /// Disables the turn ending for this turn
    /// </summary>
    internal override void SetTurnRepeat()
    {
        turnEndDisabled = true;
    }

}
