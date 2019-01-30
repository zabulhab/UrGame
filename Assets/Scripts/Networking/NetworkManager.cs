using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles all PUN callback events, enabling standard network events
/// </summary>
public class NetworkManager : MonoBehaviourPunCallbacks 
{
    [SerializeField]
    private Button btnConnectToMaster;
    [SerializeField]
    private Button btnConnectToHost;
    [SerializeField]
    private GameObject onlineOptionsPanel;
    [SerializeField]
    private GameObject mainMenuPanel;
    [SerializeField]
    private GameObject joinRoomButton;
    [SerializeField]
    private Button startGameButton;
    [SerializeField]
    private Button findNewRoomButton;
    [SerializeField]
    private GameObject loadingLoopImage;
    [SerializeField]
    private Animator loadingLoopAnimator;
    [SerializeField]
    private OnlineStateController onlineStateController;

    // whether or not this instance of the game is the host
    private bool isHost;

    // If we are currently trying to connect to the network
    private bool tryingToConnectToMaster;

    // If we are currently trying to connect to a room. Random for now.
    private bool tryingToConnectToRoom;

    /// <summary>
    /// Whether or not the online game has been started yet.
    /// </summary>
    private bool isOnlineGameInProgress;

    // Use this for initialization
    private void Start () 
    {
        tryingToConnectToMaster = false;
        tryingToConnectToRoom = false;
        isOnlineGameInProgress = false;
    }

    /// <summary>
    /// Once we have joined the server
    /// </summary>
    public override void OnConnectedToMaster()
    {
        tryingToConnectToMaster = false;

        StopLoadingImage();

        onlineOptionsPanel.SetActive(true);

        Debug.Log("Connected to Master!");
    }

    public void OnClickConnectToMaster()
    {
        PhotonNetwork.OfflineMode = false; // set to false by default
        PhotonNetwork.NickName = "PlayerName";
        PhotonNetwork.AutomaticallySyncScene = true; // to call PhotonNetwork.LoadLevel()
        PhotonNetwork.GameVersion = "v1";

        // TODO: decide whether or not to keep disabling other buttons 
        // or to have a loading animation or something
        //mainMenuPanel.SetActive(false); 
        tryingToConnectToMaster = true;
        PlayLoadingImage();

        // hide the menu but reenable its buttons for next time it opens
        mainMenuPanel.SetActive(false);

        PhotonNetwork.ConnectUsingSettings();
    }


    public void OnClickConnectToRoom()
    {
        if (!PhotonNetwork.IsConnected)
            return;
        tryingToConnectToRoom = true;

        // don't allow clicking to join room again
        DisableAllButtonsInPanel(onlineOptionsPanel);
        PhotonNetwork.JoinRandomRoom();
        PlayLoadingImage();
    }

    public void OnClickConnectToNewRoom()
    {
        startGameButton.gameObject.SetActive(false);
        PlayLoadingImage();
        PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// When you've intentionally disconnected or
    /// have become disconnected from the server
    /// </summary>
    /// <param name="cause">Cause.</param>
    public override void OnDisconnected(DisconnectCause cause)
    {
        tryingToConnectToMaster = false;
        tryingToConnectToRoom = false;
        isOnlineGameInProgress = false;
        Debug.Log(cause);
    }

    /// <summary>
    /// Always called when creating a room
    /// </summary>
    public override void OnJoinedRoom()
    {
        tryingToConnectToRoom = false;
        loadingLoopImage.gameObject.SetActive(false);

        // hide the join room button but reenable it for next time it shows
        //onlineOptionsPanel.SetActive(false);
        EnableAllButtonsInPanel(onlineOptionsPanel);

        StopLoadingImage();

        Debug.Log("Master: " + PhotonNetwork.IsMasterClient + " | Players in room: " + PhotonNetwork.CurrentRoom.PlayerCount);
        this.isHost = PhotonNetwork.IsMasterClient;
    }

    /// <summary>
    /// Called when the player clicks the start button
    /// </summary>
    public void OnClickStartGame()
    {
        onlineOptionsPanel.SetActive(false);
        this.isOnlineGameInProgress = true;
        onlineStateController.StartOnline2PMode();
    }

    /// <summary>
    /// If no room is available, create our own
    /// </summary>
    /// <param name="returnCode">Return code.</param>
    /// <param name="message">Message.</param>
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
        StopLoadingImage();
    }

    /// <summary>
    /// If a room could not be made, get the error
    /// </summary>
    /// <param name="returnCode">Return code.</param>
    /// <param name="message">Message.</param>
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log(message);
        StopLoadingImage();
        tryingToConnectToRoom = false;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        startGameButton.gameObject.SetActive(true);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (isOnlineGameInProgress)
        {

        }
        // the players are both in the room, but the remote player leaves
        else // go back to the join random room button
        {
            startGameButton.gameObject.SetActive(false);
            findNewRoomButton.gameObject.SetActive(false);
            joinRoomButton.gameObject.SetActive(true);
        }

    }

    /// <summary>
    /// Helper method to disable all buttons in panel
    /// </summary>
    /// <param name="panel">Panel.</param>
    private void DisableAllButtonsInPanel(GameObject panel)
    {
        foreach (Button button in panel.GetComponentsInChildren<Button>())
        {
            button.enabled = false;
        }
    }

    /// <summary>
    /// Helper method to enable all buttons in panel.
    /// </summary>
    /// <param name="panel">Panel.</param>
    private void EnableAllButtonsInPanel(GameObject panel)
    {
        foreach (Button button in panel.GetComponentsInChildren<Button>())
        {
            button.enabled = true;
        }
    }

    /// <summary>
    /// Makes the loading gif appear and play
    /// </summary>
    private void PlayLoadingImage()
    {
        loadingLoopImage.gameObject.SetActive(true);
        loadingLoopAnimator.Play("loading_spritesheet");
    }

    private void StopLoadingImage()
    {
        loadingLoopImage.gameObject.SetActive(false);
    }
}
