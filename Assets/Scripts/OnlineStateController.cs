using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Handles setup and overall control flow of turns while online
/// </summary>
public class OnlineStateController : StateController
{
    /// <summary>
    /// Reference to this player's OnlineTurn
    /// </summary>
    private OnlineTurn localTurn;

    /// <summary>
    /// Reference to the other player's OnlineTurn
    /// </summary>
    private OnlineTurn remoteTurn;

    [SerializeField]
    /// <summary>
    /// Used to setup the tiles with a reference to this statecontroller
    /// </summary>
    private GridSystem grid;

    /// <summary>
    /// The online player who goes first
    /// </summary>
    private OnlineTurn player1;

    /// <summary>
    /// The online player who goes second
    /// </summary>
    private OnlineTurn player2;

    /// <summary>
    /// The online turn that is currently active
    /// </summary>
    private OnlineTurn activeTurn;

    /// <summary>
    /// The online turn that is currently inactive
    /// </summary>
    private OnlineTurn inactiveTurn;

    /// <summary>
    /// Starts the online version of the game.
    /// </summary>
    public void StartOnline2PMode()
    {
        // ONLY the host player should setup the turns
        if (PhotonNetwork.IsMasterClient)
        {
            SetupTurns();
        }
        // Both sides need to give their tiles a 
        // reference to their own instance of this class
        SetReferenceInTiles();
    }

    private void SetupTurns()
    {
        PhotonView photonView = PhotonView.Get(this);
        localTurn = photonView.RPC("GetOnlineTurn", RpcTarget.Others);

        bool areWeFirstTurn = (Random.Range(0, 2) == 1) ? true : false;
        if (areWeFirstTurn)
        {
            // player1 = this client's turn
            // player2 = other client's turn
        }
        else
        {
            // player1 = other client's turn
            // player2 = this client's turn
        }
        //[Random.Range(0, turnList.Length)];

        // do the random decision of who goes first
        // inform the other player of the order
        // force the other side to wait for this decision
        // let both sides set themselves up

        remoteTurn.TurnSetup(Turn.SideName.EnemySide);
        localTurn.TurnSetup(Turn.SideName.PlayerSide);

        activeTurn = ChooseStartSide(opponentTurn);
        player1 = activeTurn;

        player2 = (player1 == playerTurn) ? opponentTurn : playerTurn;
        inactiveTurn = player2; // initialize inactive turn

        activeTurn.ActivatePhase();
        activeTurn.SetTurnTitleVisible(true);

    }

    [PunRPC]
    public override void SwitchTurn()
    {
        if (PhotonNetwork.IsMasterClient) // switch the turn ourselves
        {
            if (activeTurn.GetInstanceID() == player1.GetInstanceID())
            {
                activeTurn = player2;
                inactiveTurn = player1;

            }
            else if (activeTurn.GetInstanceID() == player2.GetInstanceID())
            {
                activeTurn = player1;
                inactiveTurn = player2;
            }
            activeTurn.ActivatePhase();
            SwapTurnTitleText();
        }
        else // tell the master client to switch the turn
        {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("SwitchTurn", RpcTarget.MasterClient);
        }
    }
    internal override Turn GetActiveTurn()
    {
        return activeTurn;
    }

    internal override Turn GetInactiveTurn()
    {
        return inactiveTurn;
    }

    /// <summary>
    /// Sets the reference to this state controller for each tile
    /// </summary>
    protected override void SetReferenceInTiles()
    {
        grid.SetTurnReferenceForAllTiles(this);
    }
}
