using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The generic tile class, from which we derive each subclass of tile
/// </summary>
public abstract class Tile : MonoBehaviour //Scriptable Object?
{
    /// <summary>
    /// Whether or not to show info for this tile when hovering over it
    /// </summary>
    private bool TileInfoShow = true;

    /// <summary>
    /// The index of this tile in the list of tiles for a player's side
    /// </summary>
    /// <value>The tile number.</value>
    internal int TileNumber { get; set; }

    /// <summary>
    /// The types for the tile subclasses
    /// </summary>
    internal enum TileType { OnePiece, TwoPiece, FourPiece, Freeze, Repeat, Restart};

    /// <summary>
    /// The tile type for this tile.
    /// </summary>
    /// <value>The type of tile.</value>
    internal TileType TypeOfTile { get; set; }

    /// <summary>
    /// A list of all pieces on top of this tile, which can be added to
    /// </summary>
    /// <value>The pieces on top.</value>
    internal List<Piece> PiecesOnTop { get; set; }

    /// <summary>
    /// The ID for this tile, which has an E, P, or N,
    /// (enemy, player, or neutral side) and a number after.
    /// Intended to be more readable as opposed to raw numbers.
    /// </summary>
    /// <value>The tile identifier.</value>
    internal string TileID { get; set; }

    /// <summary>
    /// The piece that most recently landed on this tile
    /// </summary>
    protected Piece topMostPiece;

    /// <summary>
    /// The maximum amount of pieces of the same color that can be on this tile
    /// </summary>
    protected int maxNumberSamePiece;

    /// <summary>
    /// The state controller for this game
    /// </summary>
    public StateController stateController;

    /// <summary>
    /// The string to put in the tile function pop-up window
    /// </summary>
    protected string tileFunctionSummary;

    /// <summary>
    /// Reference to the panel used to display the tile description
    /// </summary>
    [SerializeField]
    private GameObject TileDescriptionPanel;

    protected abstract void Start();

    /// <summary>
    /// Sets the tile's function summary.
    /// </summary>
    protected void setSummary(string text)
    {
        this.tileFunctionSummary = text;
    }

    /// <summary>
    /// Adds a piece reference to this tile's piece list
    /// </summary>
    /// <param name="piece">Piece.</param>
    internal void AddPiece(Piece piece)
    {
        PiecesOnTop.Add(piece);
        topMostPiece = PiecesOnTop[PiecesOnTop.Count - 1];
    }

    /// <summary>
    /// Removes a piece from this tile's piece list
    /// </summary>
    /// <param name="piece">Piece.</param>
    internal void RemovePiece(Piece piece)
    {
        PiecesOnTop.Remove(piece);

        // If this wasn't the only piece, set topmost piece to last in list
        if (PiecesOnTop.Count != 0)
        {
            topMostPiece = PiecesOnTop[PiecesOnTop.Count - 1];
        }
        else // No more topmost piece
        {
            topMostPiece = null;
        }
    }

    /// <summary>
    /// Returns how many other pieces are on this tile. 
    /// Called in Piece so that the piece moving to this tile can 
    /// know where to position itself to avoid overlap.
    /// </summary>
    /// <returns>The number pieces on tile.</returns>
    internal int GetNumPiecesOnTile()
    {
        return PiecesOnTop.Count;
    }

    /// <summary>
    /// Returns whether or not there is already the max 
    /// number of same-side pieces on top of this tile
    /// </summary>
    /// <returns><c>true</c>, if max number same piece on top, <c>false</c> otherwise.</returns>
    internal bool IsMaxNumSamePieceOnTop()
    {
        int numCurSamePiece = 0;
        foreach (Piece piece in PiecesOnTop)
        {
            Turn.SideName pieceSide = piece.GetAssociatedTurnObject().getSideName();
            if (pieceSide == stateController.GetActiveTurn().getSideName())
            {
                numCurSamePiece++;
            }
        }
        return numCurSamePiece == maxNumberSamePiece;
    }

    /// <summary>
    /// Tells the effect of the tile to activate.
    /// Implemented in each tile subclass
    /// </summary>
    internal abstract void ActivateTileFunction();

    /// <summary>
    /// Used when a piece lands on a tile with an enemy piece on it. Kicks
    /// out any/all enemy pieces, removing them from this tile's piece list.
    /// </summary>
    protected void TryKickEnemyOut()
    {
        if (GetNumPiecesOnTile() > 1)
        {
            List<Piece> piecesToRemove = new List<Piece>();
            foreach (Piece piece in PiecesOnTop)
            {
                Turn.SideName pieceSide = piece.GetAssociatedTurnObject().getSideName();
                if (pieceSide != stateController.GetActiveTurn().getSideName())
                {
                    piece.KickBackToStart();
                    piecesToRemove.Add(piece);
                }
            }
            // Safely remove any pieces that need removing
            foreach (Piece piece in piecesToRemove)
            {
                RemovePiece(piece);
            }
        }
    }

    /// <summary>
    /// Makes the tile info window pop up
    /// </summary>
    private void OnMouseEnter()
    {
        if (TileInfoShow == true && stateController.GetActiveTurn().PieceSelectionPhase)
        {
            ShowTileInfo();
        }
    }

    /// <summary>
    /// Makes the title info window disappear
    /// </summary>
    private void OnMouseExit()
    {
        HideTileInfo();
    }

    /// <summary>
    /// Passes in and displays the information about this tile in a window.
    /// </summary>
    protected void ShowTileInfo()
    {
        string infoPlusTilesOnTop = tileFunctionSummary;
        TileDescriptionPanel.GetComponentInChildren<Text>().text = infoPlusTilesOnTop;
        TileDescriptionPanel.SetActive(true);

    }

    /// <summary>
    /// Hides the info window about this tile.
    /// </summary>
    protected void HideTileInfo()
    {
        TileDescriptionPanel.SetActive(false);
    }
}
