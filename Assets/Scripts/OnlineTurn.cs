using Photon.Pun;

/// <summary>
/// A turn that is intended to be used online.
/// </summary>
public class OnlineTurn : Turn
{
    [PunRPC]
    private int TurnOrderNum { get; set; }

    [PunRPC]
    internal override void ActivatePhase()
    {
        base.ActivatePhase();

        //
    }
}
