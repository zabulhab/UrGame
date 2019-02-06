using Photon.Pun;

/// <summary>
/// Moves the piece that landed on it back to its 
/// starting position, with a 50-50 chance. 
/// TODO: Add a rolling visualization to this chance.
/// </summary>
public class RestartTile : Tile 
{
    protected void Start()
    {
        base.Start("You might go back to start with a 50% chance. Holds one piece.",
                                            TileType.Restart, gameObject.name);
    }

    /// <summary>
    /// Used to make a time delay before kicking a piece back to start.
    /// Ends the turn here, as well, instead of outside of the coroutine.
    /// </summary>
    /// <returns>The and restart piece.</returns>
    private void RestartPieceAndEndTurn()
    {
        //yield return new WaitForSeconds(.5f);
        topMostPiece.KickBackToStart();
        this.RemovePiece(topMostPiece);
        stateController.EndActiveTurn();
    }

    internal override void ActivateTileFunction()
    {
        if (PhotonNetwork.IsConnectedAndReady) // online mode
        {
            // only one player should get the random number
            if (PhotonNetwork.LocalPlayer.IsMasterClient) 
            {
                System.Random rand = new System.Random();

                PhotonView pView = PhotonView.Get(this);
                pView.RPC("OnlineTileFunction", RpcTarget.All, rand.Next(0, 2));
            }
        }
        else // offline mode
        {
            OfflineTileFunction();
        }
        // TODO: Wait for some lag time before teleporting back to start
        // TODO: Make sure that only one side activates this tile function
    }

    private void OfflineTileFunction()
    {
        System.Random rand = new System.Random();
        if (rand.Next(0, 2) != 0) // With 1/2 chance, move this piece to start
        {
            RestartPieceAndEndTurn();
        }
        else
        {
            TryKickEnemyOut();
            stateController.EndActiveTurn();
        }

    }

    [PunRPC]
    private void OnlineTileFunction(int randChance)
    {
        if (randChance != 0) // With 1/2 chance, move this piece to start
        {
            RestartPieceAndEndTurn();
        }
        else
        {
            TryKickEnemyOut();
            stateController.EndActiveTurn();
        }
    }
}
