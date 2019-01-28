using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class OnlineStateController : StateController
{
    /// <summary>
    /// Reference to the other player in online mode.
    /// </summary>
    private Player onlineOpponent;

    /// <summary>
    /// Starts the online version of the game.
    /// </summary>
    public void StartOnline2PMode()
    {
        //this.isOnline2PMode = true;
        //onlineOpponent = PhotonNetwork.PlayerListOthers[0];
        //PhotonView photonView = PhotonView.Get(this);
        //onlineOpponent.GetComponent<PhotonView>().RPC("methodToCallHere",);
        OnlineTurnSetup();
    }

    private void OnlineTurnSetup()
    {
        // if we are the host player
        if (PhotonNetwork.IsMasterClient)
        {
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
        }
        // do the random decision of who goes first
        // inform the other player of the order
        // force the other side to wait for this decision
        // let both sides set themselves up
    }

    protected override void SwitchTurn()
    {

    }

    internal override Turn GetActiveTurn()
    {

    }

    internal override Turn GetInactiveTurn()
    {

    }
}
