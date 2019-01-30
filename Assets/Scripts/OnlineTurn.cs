using Photon.Pun;

/// <summary>
/// A turn that is intended to be used online.
/// </summary>
public class OnlineTurn : Turn
{
    /// <summary>
    /// During turn setup, this turn is informed of whether or not 
    /// </summary>
    /// <value>The turn order number.</value>
    [PunRPC]
    private int TurnOrderNum { get; set; }

    [PunRPC]
    internal override void ActivatePhase()
    {
        base.ActivatePhase();

        //
    }

    [PunRPC]
    public override void EndTurn(bool AITurnEnded = false, OfflineStateController phaseController = null)
    {
        SetFreezePanelVisible(false);
        turnEndPanel.SetActive(true);

        UnfreezeBoardPieces();

        // Disable clicking pieces
        PieceSelectionPhase = false;
        SetAllPiecesUnSelectable();
    }
}
