using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The generic tile class, from which we derive each subclass of tile
/// </summary>
public abstract class Tile : MonoBehaviour //Scriptable Object?
{
    private bool TileInfoShow = true;

    // The index of this tile in the list of tiles for a player's side
    internal int TileNumber { get; set; }

    // The types for the tile subclasses
    internal enum TileType { OnePiece, TwoPiece, FourPiece, Freeze, Repeat, Restart};

    internal TileType TypeOfTile { get; set; }

    // A list of all pieces on top of this tile, which can be added to
    internal List<Piece> PiecesOnTop { get; set; }

    internal string TileID { get; set; }

    // The piece that most recently landed on this tile
    protected Piece topMostPiece;

    // The maximum amount of pieces of the same color that can be on this tile
    protected int maxNumberSamePiece;

    public StateController stateController;

    // The string to put in the tile function pop-up window
    protected string tileFunctionSummary;

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

    // Adds a piece reference to this tile's piece list
    internal void AddPiece(Piece piece)
    {
        PiecesOnTop.Add(piece);
        topMostPiece = PiecesOnTop[PiecesOnTop.Count - 1];
    }

    // Removes a piece from this tile's piece list
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
        Debug.Log("PIECES ON TOP: " + PiecesOnTop.Count);
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
        Debug.Log(PiecesOnTop);
        return numCurSamePiece == maxNumberSamePiece;
    }

    /// <summary>
    /// Tells the effect of the tile to activate.
    /// Implemented in each tile subclass
    /// </summary>
    internal abstract void ActivateTileFunction();
	
    /// <summary>
    /// Sets the color of the tile. Used to indicate different tiles, for now
    /// TODO: Put actual textures in instead
    /// </summary>
    /// <param name="color">Color.</param>
    protected void setTileColor(Color color )
    {

    }

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
        if (TileInfoShow == true)
        {
            ShowTileInfo();
            Debug.Log("HI");
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
        string infoPlusTilesOnTop = tileFunctionSummary + ", " + 
                                        GetNumPiecesOnTile() + " pieces on top";
        TileDescriptionPanel.GetComponentInChildren<Text>().text = infoPlusTilesOnTop;
        //TileDescriptionPanel.GetComponentInChildren<Text>().SetActive(true);
        TileDescriptionPanel.SetActive(true);
        //TileDescriptionPanel.GetComponentInParent<GameObject>().SetActive(true);
    }

    /// <summary>
    /// Hides the info window about this tile.
    /// </summary>
    protected void HideTileInfo()
    {
        TileDescriptionPanel.SetActive(false);
    }

    /// <summary>
    /// Hides tile info window
    /// </summary>
    //protected void HideTileInfo()
    //{
    //    TileDescriptionPanel.SetActive(false);
    //}
}
