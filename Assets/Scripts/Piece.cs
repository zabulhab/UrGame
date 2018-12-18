using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A piece that can move on the board. Handles movement of pieces.
/// </summary>
public class Piece : MonoBehaviour
{
    // A type for the current status of the piece (undeployed, active, finished)
    internal enum PieceStatus { Undeployed, Deployed, Finished };

    /// <summary>
    /// The tile index we use to represent an "undeployed" piece not yet on one
    /// </summary>
    private const int UNDEPLOYED_IDX = -1;

    /// <summary>
    /// Number of accessible tiles per side
    /// </summary>
    private static readonly int TILE_COUNT = 14;

    /// <summary>
    /// This piece's current deployment status
    /// </summary>
    internal PieceStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the name of the sidename for this piece
    /// </summary>
    /// <value>The name of the side.</value>
    public Turn.SideName SideName { get; set; }

    /// <summary>
    /// This piece's associated Turn object, and getter & setter
    /// </summary>
    private Turn associatedTurnObject;

    /// <summary>
    /// This piece's index in the list of start positions
    /// Used to place it in the correct place in the undeployed area
    /// </summary>
    /// <value>The start index.</value>
    internal int StartIndex { get; set; }

    /// <summary>
    /// The color of the piece before highlighting, so we can change it back
    /// </summary>
    private Color startColor;

    /// <summary>
    /// The grid system that stores data about the tiles
    /// </summary>
    [SerializeField]
    private GridSystem grid;

    /// <summary>
    /// Gets or sets index of the tile the piece is currently on; ranges from 0-13
    /// </summary>
    /// <value>The index of the current tile.</value>
    internal int CurrentTileIdx { get; set; }

    /// <summary>
    /// The tile object that this piece is currently on
    /// </summary>
    private Tile currentTile;

    /// <summary>
    /// Reference to the tiles on this side, 
    /// provided by the turn subclass this piece is in
    /// </summary>
    private Tile[] tilesOnSide;

    /// <summary>
    /// Reference to the tile that this piece wants to land on
    /// </summary>
    private Tile tileDestination;

    /// <summary>
    /// The board-index of the tile that this piece wants to land on
    /// </summary>
    private int tileDestIndex;

    /// <summary>
    /// Whether or not to allow this piece to move. 
    /// Set true when turn becomes activated and this piece meets requirements.
    /// Set false if piece cannot be moved during the turn
    /// </summary>
    internal bool PieceCanMove { get; set; }

    private void Start()
    {
        CurrentTileIdx = UNDEPLOYED_IDX; 
        startColor = GetComponent<Renderer>().material.color;
        Status = PieceStatus.Undeployed;
    }

    /// <summary>
    /// Assign the associated turn object for this piece. 
    /// Used while initializing that turn.
    /// </summary>
    /// <param name="turn">Turn.</param>
    internal void SetAssociatedTurnObject(Turn turn)
    {
        associatedTurnObject = turn;
    }

    /// <summary>
    /// Return the Turn associated with this piece
    /// </summary>
    /// <returns>The associated turn object.</returns>
    internal Turn GetAssociatedTurnObject()
    {
        return associatedTurnObject;
    }

    /// <summary>
    /// Calculates and stores the target tile for this piece for when it moves
    /// </summary>
    /// <param name="numberOfSpaces">Number of spaces.</param>
    internal void SetTargetTile(int numberOfSpaces)
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

        tileDestination = grid.TileToLandOn(desiredIdx,SideName);
        tileDestIndex = desiredIdx;
    }

    /// <summary>
    /// Checks and returns the tile that the piece will be targeting if/when it moves.
    /// Used to take a glimpse instead of storing data.
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
    /// Moves the piece to a new tile farther on the board, or off of the board
    /// </summary>
    internal void MovePieceForward()
    {
        // Don't let pieces at end loop around
        if (tileDestIndex > TILE_COUNT || tileDestIndex < CurrentTileIdx)
        {
            MoveToFinish();
        }
        // moving piece within board regularly
        else
        {
            MoveToTargetTile();
        }
    }

    /// <summary>
    /// Updates the transform position of the piece, 
    /// and updates the current tile index, as well.
    /// Also activates the function of the tile the piece lands on.
    /// </summary>
    private void MoveToTargetTile()
    {
        // Stack on top of other pieces on the tile
        int numOtherPiecesOnDestTile = tileDestination.GetNumPiecesOnTile();
        transform.position = new Vector3
                                (tileDestination.transform.position.x,
                                 ((0.6f) +
                                  (numOtherPiecesOnDestTile * GetComponent<Renderer>().bounds.size.y)),
                                 tileDestination.transform.position.z);

        // If the piece is not starting out, eg is on a tile already,
        // then remove it from that tile
        if (!(this.Status == PieceStatus.Undeployed))
        {
            currentTile.RemovePiece(this);
        }
        else // set undeployed piece to be deployed now
        {
            Status = PieceStatus.Deployed;
        }

        // Update the current tile, current tile index, 
        // and the old tile's and new tile's lists
        CurrentTileIdx = tileDestIndex;
        currentTile = tileDestination;
        currentTile.AddPiece(this);

        // Tell the tile to activate any special behavior
        currentTile.ActivateTileFunction();
    }

    /// <summary>
    /// Used to move this piece off of the board
    /// instead of regularly onto another tile
    /// </summary>
    private void MoveToFinish()
    {
        Status = PieceStatus.Finished;
        currentTile.RemovePiece(this);

        // If this was the last piece to finish
        if (associatedTurnObject.AreAllPiecesFinished())
        {
            associatedTurnObject.ActivateGameOver();
        }
        else // end turn normally
        {
            associatedTurnObject.EndTurn();
        }
        this.Disappear();
    }

    /// <summary>
    /// Used to check if a piece can move to a tile, given an input roll number
    /// </summary>
    /// <returns><c>true</c>, if piece can move to tile, <c>false</c> otherwise.</returns>
    /// <param name="rolledNumber">Rolled number.</param>
    internal bool CheckPieceCanMoveToTile(int rolledNumber)
    {
        Tile targetTile = this.GetTargetTile(rolledNumber);
        if (this.Status == PieceStatus.Finished)
        {
            return false;
        }
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

    /// <summary>
    /// Makes a piece that has finished disappear from the board,
    /// both visually and functionally
    /// </summary>
    private void Disappear()
    {
        this.gameObject.SetActive(false); // disable interaction and hide
    }

    /// <summary>
    /// Calls the highlight method on the mouse enter event if this 
    /// piece is on the side that is currently active
    /// </summary>
    private void OnMouseEnter()
    {
        // If this piece is one that is ready to be moved
        if (this.PieceCanMove)
        {
            HighlightGreen();
        }
    }

    /// <summary>
    /// Unhighlights the piece
    /// </summary>
    private void OnMouseExit()
    {
        UnHighlight();
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
    /// Kicks this piece back to its undeployed starting state. Called when the 
    /// active turn lands on a restart tile, with a 1/2 chance.
    /// </summary>
    internal void KickBackToStart()
    {
        Status = PieceStatus.Undeployed;
        gameObject.transform.position =
              associatedTurnObject.pieceStartLocations[StartIndex];
        CurrentTileIdx = UNDEPLOYED_IDX;
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
