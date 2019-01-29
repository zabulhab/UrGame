using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Handles setup and overall control flow of turns while online
/// </summary>
public class OnlineStateController : StateController
{
    /// <summary>
    /// Reference to the other player in online mode.
    /// </summary>
    private Player onlineOpponent;

    [SerializeField]
    /// <summary>
    /// Used to setup the tiles with a reference to this statecontroller
    /// </summary>
    private GridSystem grid;

    /// <summary>
    /// Starts the online version of the game.
    /// </summary>
    public void StartOnline2PMode()
    {
        //this.isOnline2PMode = true;
        //onlineOpponent = PhotonNetwork.PlayerListOthers[0];
        //PhotonView photonView = PhotonView.Get(this);
        //onlineOpponent.GetComponent<PhotonView>().RPC("methodToCallHere",);

        // ONLY the host player should setup the turns
        if (PhotonNetwork.IsMasterClient)
        {
            OnlineTurnSetup();
        }
    }

    private void OnlineTurnSetup()
    {
        SetReferenceInTiles();

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

    }

    public override void SwitchTurn()
    {
        // tell it to 
    }

    internal override Turn GetActiveTurn()
    {
        return null;
    }

    internal override Turn GetInactiveTurn()
    {
        return null;
    }

    /// <summary>
    /// Sets the reference to this state controller for each tile
    /// </summary>
    protected override void SetReferenceInTiles()
    {
        grid.SetTurnReferenceForAllTiles(this);
    }
}
