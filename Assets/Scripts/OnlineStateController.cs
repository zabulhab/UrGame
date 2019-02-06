using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Handles setup and overall control flow of turns while online.
/// Contains additional RPC methods to tell the remote instance of 
/// this class to manipulate its local OnlineTurn object. 
/// This subclass cannot keep store the remote OnlineTurn, so we
/// do not have references to an "ActiveTurn", "Player1", etc.,
/// instead performing a check at every point where this matters
/// </summary>
public class OnlineStateController : StateController
{
    [SerializeField]
    /// <summary>
    /// Reference to this player's OnlineTurn
    /// </summary>
    private OnlineTurn localTurn;

    /// <summary>
    /// Used to setup the tiles with a reference to this statecontroller
    /// </summary>
    [SerializeField]
    private GridSystem grid;

    /// <summary>
    /// The opponent title text.
    /// </summary>
    [SerializeField]
    private GameObject opponentTitleText;

    /// <summary>
    /// Starts the online version of the game.
    /// </summary>
    public void StartOnline2PMode()
    {
        // Both sides need to initialize their turns
        localTurn.SetupLocalOnlineTurn();

        // ONLY the host player should set up the turns
        if (PhotonNetwork.IsMasterClient)
        {
            SetupOnlineTurnFlow();
        }
        // Both sides need to give their tiles a 
        // reference to their own instance of this class
        SetReferenceInTiles();

    }

    /// <summary>
    /// Sets up the order of both online turns. 
    /// Can only be used by the master client.
    /// </summary>
    private void SetupOnlineTurnFlow()
    {

        bool areWeFirstTurn = (Random.Range(0, 2) == 1) ? true : false;
        if (areWeFirstTurn) // we, the masterclient, go first
        {
            localTurn.TurnOrderNum = 0;
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("AssignOrderToLocalTurn", RpcTarget.Others, 1);

            // activate the proper turn
            ActivateLocalTurn();
        }
        else // we, the masterclient, go second
        {
            localTurn.TurnOrderNum = 1;
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("AssignOrderToLocalTurn", RpcTarget.Others, 0);

            // activate the proper turn
            photonView.RPC("ActivateLocalTurn", RpcTarget.Others);
        }
        //activeTurn.SetTurnTitleVisible(true);

    }

    /// <summary>
    /// Passes control to the other Player.
    /// </summary>
    public override void SwitchTurn()
    {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("ActivateLocalTurn", RpcTarget.Others);

        SetLocalTurnActive(false);

        UpdateTurnTitleText();
    }

    /// <summary>
    /// Activates the local OnlineTurn associated with this player.
    /// Can be called from the remote turn to activate this 
    /// local turn when switching turns
    /// </summary>
    [PunRPC]
    internal void ActivateLocalTurn()
    {
        localTurn.ActivatePhase();
        SetLocalTurnActive(true);

        UpdateTurnTitleText();

    }

    /// <summary>
    /// Determines which turn is active and tells the
    /// appropriate Player to end its local turn
    /// </summary>
    [PunRPC]
    internal override void EndActiveTurn()
    {
        if (localTurn.IsActiveTurn) // we have the active turn
        {
            localTurn.EndTurn();
        }
        else // the other Player has the active turn
        { 
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("EndActiveTurn", RpcTarget.Others);
        }
    }

    internal override Turn.SideName GetActiveTurnSideName()
    {
        if (localTurn.IsActiveTurn) // we have the active turn
        {
            return localTurn.TurnSideName; // return our side's sidename
        }
        // return the opposite of what this turn's sidename is
        else
        {
            if (localTurn.TurnSideName == Turn.SideName.PlayerSide)
            {
                return Turn.SideName.EnemySide;
            }
            else
            {
                return Turn.SideName.PlayerSide;
            }
        }
    }

    /// <summary>
    /// For the online state controller, we only check the local turn, since
    /// this will only display the UI to the local player, anyways.
    /// </summary>
    /// <returns><c>true</c>, if active turn in piece selection phase <c>false</c> otherwise.</returns>
    internal override bool IsActiveTurnInPieceSelectionPhase()
    {
        // Only called locally when we hover over tiles on our end, 
        // so we don't even need to bother checking the other turn.
        return localTurn.IsInPieceSelectionPhase;
    }

    [PunRPC]
    internal override void FreezeInactiveTurn()
    {
        if (localTurn.IsActiveTurn) // freeze the remote turn
        {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("FreezeInactiveTurn", RpcTarget.Others);
        }
        else // freeze the local turn
        {
            localTurn.FreezeBoardPieces();
        }
    }

    /// <summary>
    /// Sets the reference to this state controller for each tile
    /// </summary>
    protected override void SetReferenceInTiles()
    {
        grid.SetTurnReferenceForAllTiles(this);
    }

    /// <summary>
    /// Tells the local turn for this instance of OnlineStateController
    /// to set its turn order number. 0 goes first, and 1 goes second.
    /// </summary>
    [PunRPC]
    internal void AssignOrderToLocalTurn(int orderNum)
    {
        localTurn.TurnOrderNum = orderNum;
    }

    /// <summary>
    /// Tells the local turn for this instance of OnlineStateController
    /// to set its active status to either true or false
    /// </summary>
    [PunRPC]
    internal void SetLocalTurnActive(bool active)
    {
        localTurn.IsActiveTurn = active;
    }

    /// <summary>
    /// Changes the text displayed at the top. Says "Player Turn" while
    /// the local player is active, and "Enemy Turn" otherwise
    /// </summary>
    private void UpdateTurnTitleText()
    {
        if (localTurn.IsActiveTurn)
        {
            opponentTitleText.SetActive(false);
            localTurn.SetTurnTitleVisible(true);
        }
        else
        {
            localTurn.SetTurnTitleVisible(false);
            opponentTitleText.SetActive(true);
        }
    }
}
