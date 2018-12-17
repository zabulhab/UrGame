using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A piece that can move on the board. Handles movement of pieces.
/// </summary>
public class Piece : MonoBehaviour
{
    // A type for the current status of the piece (undeployed, active, finished)
    internal enum PieceStatus { Undeployed, Deployed, Finished };

    // The piece's actual current status
    private PieceStatus status;

    public Turn.SideName SideName { get; set; }

    // This piece's associated Turn object, and getter & setter
    private Turn associatedTurnObject;

    // This piece's index in the list of start positions
    // Used to place it in the correct place in the undeployed area
    private int startIndex;

    private Color startColor;

    // Reference to the grid
    [SerializeField]
    private GridSystem grid;

    // The index of the tile the piece is currently on; ranges from 0-13
    internal int CurrentTileIdx { get; set; }

    // The tile that this piece is currently on
    private Tile currentTile;

    // Reference to the tiles on this side, 
    // provided by the turn subclass this piece is in
    private Tile[] tilesOnSide;

    private static readonly int TILE_COUNT = 14;

    // Reference to the tile we want to land on
    private Tile tileDestination;

    // The board-index of the tile we want to land on
    private int tileDestIndex;

    // Whether or not to allow this piece to move. 
    // True if associated turn is active.
    // Set true when turn becomes activated.
    // False if piece cannot be moved during a turn
    private bool pieceCanMove;

    private void Start()
    {
        CurrentTileIdx = -1;
        startColor = GetComponent<Renderer>().material.color;
        status = PieceStatus.Undeployed;
    }


    // Assign the associated turn object. Used while initializing
    internal void SetAssociatedTurnObject(Turn turn)
    {
        associatedTurnObject = turn;
    }

    // Return the Turn associated with this piece
    internal Turn GetAssociatedTurnObject()
    {
        return associatedTurnObject;
    }

    /// <summary>
    /// Calculates & stores the target tile for this piece for when it moves
    /// </summary>
    /// <param name="numberOfSpaces">Number of spaces.</param>
    internal void SetNumSpacesToMove(int numberOfSpaces)
    {
        if (numberOfSpaces == 0)
        {
            return;
        }

        int desiredIdx = CurrentTileIdx + numberOfSpaces;

        if (desiredIdx > 13) // Loop back around to 0
        {
            desiredIdx = 0;
        }

        tileDestination =
            grid.TileToLandOn
                (desiredIdx,
                 SideName);
        tileDestIndex = desiredIdx;
    }

    /// <summary>
    /// Returns the tile that the piece will be targeting if/when it moves.
    /// </summary>
    internal Tile GetTargetTile(int numberOfSpaces)
    {
        if (numberOfSpaces == 0)
        {
            return null;
        }

        int desiredIdx = CurrentTileIdx + numberOfSpaces;

        if (desiredIdx > 13) // Loop back around to 0
        {
            desiredIdx = 0;
        }

        return grid.TileToLandOn(desiredIdx, SideName);
    }

    /// <summary>
    /// Updates the transform position of the piece, 
    /// and updates the current tile index, as well.
    /// Also activates the function of the tile the piece lands on.
    /// </summary>
    internal void MoveToTargetTile()
    {
        // Don't let it loop around
        if (tileDestIndex > TILE_COUNT || tileDestIndex < CurrentTileIdx)
        {
            status = PieceStatus.Finished;
            currentTile.RemovePiece(this);
            this.Disappear();
            associatedTurnObject.EndTurn();
            if (associatedTurnObject.AreAllPiecesFinished())
            {
                associatedTurnObject.GameIsOver = true;
            }
            return;
        }
        int otherPiecesOnTile = tileDestination.GetNumPiecesOnTile();
        transform.position = new Vector3
                                (tileDestination.transform.position.x,
                                 ((0.6f) + 
                                  (otherPiecesOnTile * GetComponent<Renderer>().bounds.size.y)),
                                 tileDestination.transform.position.z);

        // Update the current tile, current tile index, 
        // and the old tile's and new tile's lists

        CurrentTileIdx = tileDestIndex;

        // TODO: Update this to use the undeployed status of the piece
        // If the piece is not starting out, eg is on a tile already,
        // then remove it from that tile
        if (currentTile)
        {
            currentTile.RemovePiece(this);
        }
        currentTile = tileDestination;
        currentTile.AddPiece(this);
        status = PieceStatus.Deployed;

        // Tell the tile to activate any special behavior
        currentTile.ActivateTileFunction();

    }

    internal bool CheckPieceCanMoveToTile(int rolledNumber)
    {
        //// if (get tile at piece position + spaces to move)
        //Tile targetTile = piece.GetTargetTile(rolledNumber);
        ////      has too many tiles of the same color on it already
        //if (targetTile != null && (!targetTile.IsMaxNumSamePieceOnTop()))
        //{
        //    piece.SetPieceCanMove(true);
        //    return true;
        //}
        //return false;
        Tile targetTile = this.GetTargetTile(rolledNumber);
        if (targetTile != null && (!targetTile.IsMaxNumSamePieceOnTop()))
        {
            return true;
        }
        // If this piece is at the end of the board and
        // theoretically targeting a tile at the beginning 
        // of the board that has a piece on it already
        else if (targetTile != null && (targetTile.IsMaxNumSamePieceOnTop()))
        {
            if (targetTile.TileNumber < this.CurrentTileIdx)
            {
                return true;
            }
        }
        return false;
    }

    // Makes a piece that has finished the board disappear
    private void Disappear()
    {
        this.GetComponent<Renderer>().enabled = false;
        //Destroy(this.gameObject);
    }

    /// <summary>
    /// Calls the highlight method on the mouse enter event if this 
    /// piece is on the side that has a currently active team
    /// </summary>
    private void OnMouseEnter()
    {
        // If this piece is one that is ready to be moved
        if (this.pieceCanMove)
        {
            HighlightGreen();
        }
    }

    private void OnMouseExit()
    {
        UnHighlight();
    }

    /// <summary>
    /// Whether or not this piece is able to be selected for movement.
    /// Called from the associated Turn object.
    /// </summary>
    internal void SetPieceCanMove(bool canMove)
    {
        pieceCanMove = canMove;
    }

    internal bool GetPieceCanMove()
    {
        return pieceCanMove;
    }

    /// <summary>
    /// Set this piece to its appropriate status.
    /// Based on whether it is undeployed, deployed, or finished.
    /// </summary>
    /// <param name="newStatus">Changes piece to this status</param>
    internal void SetPieceStatus(PieceStatus newStatus)
    {
        status = newStatus;
    }

    /// <summary>
    /// Returns the deployment status of this piece. 
    /// </summary>
    /// <returns>The piece status.</returns>
    internal Piece.PieceStatus GetPieceStatus()
    {
        return status;
    }

    ///<summary>
    /// Sets the color of the piece to green to indicate that it can be moved
    /// </summary>
    internal void HighlightGreen()
    {
        GetComponent<Renderer>().material.color = Color.green;
    }

    /// <summary>
    /// Resets the color of the piece to its original, un-highlighted color
    /// </summary>
    internal void UnHighlight()
    {
        GetComponent<Renderer>().material.color = startColor;
    }

    /// <summary>
    /// Kicks the piece back to its undeployed starting state. Called when the 
    /// active turn lands on a restart tile, with a 1/2 chance.
    /// </summary>
    internal void KickBackToStart()
    {
        SetPieceStatus(PieceStatus.Undeployed);
        MovePieceToStart();
    }

    /// <summary>
    /// Sets the index of this piece in terms of undeployed starting position
    /// </summary>
    /// <param name="index">Index.</param>
    internal void SetStartIndex(int index)
    {
        startIndex = index;
    }

    /// <summary>
    /// Moves this piece into its starting position. Called from restart tiles.
    /// </summary>
    private void MovePieceToStart()
    {
        gameObject.transform.position =
                      associatedTurnObject.pieceStartLocations[startIndex];
        CurrentTileIdx = -1;
    }

    // TODO: Use for lerping the piece
    private void Update()
    {

    }

    /// <summary>
    /// Returns this piece and any piece[s] that this piece could kill on its 
    /// next turn, assuming this turn rolls the number needed to land there.
    /// </summary>
    /// <returns>The piece(s) that this piece could kill on its next turn,
    /// provided this side rolls the right number.</returns>
    internal KeyValuePair<Piece, List<Piece>> GetPieceAndKillablePiecesNextTurn(int numRolled)
    {
        KeyValuePair<Piece, List<Piece>> pieceAndKillablePieces = new KeyValuePair<Piece, List<Piece>>();
        List<Piece> overallKillables = new List<Piece>();
        for (int potentialRollNum = numRolled+1; potentialRollNum < numRolled+4; potentialRollNum++)
        {
            List<Piece> numSpecificKillables = GetKillablePieces(potentialRollNum);
            if (!numSpecificKillables.Equals(default(List<Piece>)))
            {
                // add each killable piece for this number 
                // to the overall killable pieces list
                foreach (Piece piece in numSpecificKillables)
                {
                    overallKillables.Add(piece);
                }
            }
        }
        pieceAndKillablePieces = new KeyValuePair<Piece, List<Piece>>(this, overallKillables);
        return pieceAndKillablePieces;
    }

    /// <summary>
    /// Returns this piece and any piece[s] that this piece could kill on this turn
    /// </summary>
    /// <returns>The piece(s) that this piece could kill on this turn.</returns>
    internal KeyValuePair<Piece, List<Piece>> GetPieceAndKillablePiecesThisTurn(int numRolled)
    {
        return new KeyValuePair<Piece, List<Piece>>(this, GetKillablePieces(numRolled));
    }

    /// <summary>
    /// Helper method for GetPieceAndKillablePieces to check if any pieces
    /// can be killed on the next turn given a potential rolled number as input
    /// </summary>
    /// <returns>The killable pieces.</returns>
    /// <param name="rollNum">Roll number, used to get the landing tile.</param>
    private List<Piece> GetKillablePieces(int rollNum)
    {
        List<Piece> enemyPieces = new List<Piece>();
        Tile destinationTile = GetTargetTile(rollNum);
        if (destinationTile != null) // not the end of the board
        {
            List<Piece> allPiecesOnTile = destinationTile.PiecesOnTop;
            foreach (Piece piece in allPiecesOnTile)
            {
                if (SideName != piece.SideName) // if other side's piece
                {
                    enemyPieces.Add(piece);
                }
            }
        }
        return enemyPieces;
    }
}
