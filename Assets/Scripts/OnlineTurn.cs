using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// A turn that is intended to be used online.
/// </summary>
public class OnlineTurn : Turn
{
    //// Just done to hide this field from the parent class in the editor,
    //// as we aren't manually dragging and dropping them in anymore
    //[HideInInspector]
    //protected new List<Piece> allPieces = new List<Piece>();

    //[HideInInspector]
    //protected new GameObject turnEndPanel;
       

    [SerializeField]
    [Header("7 white pieces, followed by 7 black pieces")]
    private List<Piece> piecesBothSides = new List<Piece>();

    /// <summary>
    /// During turn setup, this turn is told whether it goes first or second
    /// </summary>
    /// <value>The turn order number.</value>
    internal int TurnOrderNum { get; set; }

    /// <summary>
    /// Used to keep track of whether or not this turn is the active one.
    /// </summary>
    /// <value>The turn order number.</value>
    internal bool IsActiveTurn { get; set; }

    /// <summary>
    /// Called by each OnlineTurn. Sets this instance's sidename and assigns
    /// pieces to the turn based on whether or not we are the master client.
    /// TODO: P2 will also change its camera angle.
    /// </summary>
    internal void SetupLocalOnlineTurn()
    {
        if (PhotonNetwork.IsMasterClient) // effectively P1; use white pieces
        {
            allPieces = piecesBothSides.GetRange(0, 7);
            TurnSetup(Turn.SideName.PlayerSide);
            foreach (Piece piece in piecesBothSides.GetRange(7, 7))
            {
                piece.SideName = Turn.SideName.EnemySide;
            }
        }
        else // Effectively P2, use black pieces
        {
            allPieces = piecesBothSides.GetRange(7, 7);
            TurnSetup(Turn.SideName.EnemySide);
            foreach (Piece piece in piecesBothSides.GetRange(0,7))
            {
                piece.SideName = Turn.SideName.PlayerSide;
            }
        }
    }

    private void OnValidate()
    {
        if (piecesBothSides.Count != 14)
        {
            Debug.LogError("The length of the Pieces Both Sides array must be 14!");
        }
    }

    /// <summary>
    /// Called by a hit piece; tries to move it
    /// to that one
    /// </summary>
    internal override void PieceHitTryMove(Piece hitPiece)
    {

        selectedPiece = hitPiece;
        rolledNumberText.SetActive(false);
        clickSFX.Play(0); // play the SFX

        PhotonView pView = PhotonView.Get(this);
        pView.RPC("ChangeSelectedPiece", RpcTarget.Others, selectedPiece.IdxInAllPieces);
        pView.RPC("MovePiece", RpcTarget.All, rolledNumber);

    }

    [PunRPC]
    /// <summary>
    /// Sets the selected piece to the given one. Used to tell the remote
    /// player which piece the local player has clicked on
    /// </summary>
    internal void ChangeSelectedPiece(int pieceID)
    {
        selectedPiece = FindPieceMatchingID(pieceID);
    }

    /// <summary>
    /// Helper method to return a piece based on its ID
    /// </summary>
    /// <returns>The piece matching identifier.</returns>
    /// <param name="ID">Identifier.</param>
    private Piece FindPieceMatchingID(int ID)
    {
        // Use index in list piecesBothSides
        return piecesBothSides[ID];
    }

    [PunRPC]
    public override void MovePiece(int rolledNum)
    {
        selectedPiece.SetTargetTile(rolledNum);
        selectedPiece.UnHighlight();
        SetAllPiecesUnSelectable(); // disable selecting other pieces

        UnfreezeBoardPieces();

        if (rolledNum != 0)
        {
            selectedPiece.MovePieceForward();
        }
    }
}
