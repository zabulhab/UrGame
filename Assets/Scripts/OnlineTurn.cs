using Photon.Pun;

/// <summary>
/// A turn that can be seen by the other player in a room.
/// </summary>
public class OnlineTurn : Turn
{
    [PunRPC]
    internal override void ActivatePhase()
    {
        base.ActivatePhase();

        //
    }
}
