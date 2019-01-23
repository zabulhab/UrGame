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
    //[SerializeField]
    //private GameObject joinRoomButton;
    //[SerializeField]
    //private GameObject startGameButton;

    // whether or not this instance of the game is the host
    private bool isHost;

    // If we are currently trying to connect to the network
    private bool tryingToConnectToMaster;

    // If we are currently trying to connect to a room. Random for now.
    private bool tryingToConnectToRoom;

	// Use this for initialization
	private void Start () 
    {
        tryingToConnectToMaster = false;
        tryingToConnectToRoom = false;
    }

    public void OnClickConnectToMaster()
    {
        PhotonNetwork.OfflineMode = false; // set to false by default
        PhotonNetwork.NickName = "PlayerName";
        PhotonNetwork.AutomaticallySyncScene = true; // to call PhotonNetwork.LoadLevel()
        PhotonNetwork.GameVersion = "v1";

        // don't allow clicking anymore buttons while connecting
        DisableAllButtonsInPanel(mainMenuPanel);
        // TODO: decide whether or not to keep disabling other buttons 
        // or to have a loading animation or something
        //mainMenuPanel.SetActive(false); 
        tryingToConnectToMaster = true; 

        PhotonNetwork.ConnectUsingSettings();
    }

    /// <summary>
    /// Once we have joined the server
    /// </summary>
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        tryingToConnectToMaster = false;

        // hide the menu but reenable its buttons for next time it opens
        mainMenuPanel.SetActive(false);
        EnableAllButtonsInPanel(mainMenuPanel);

        onlineOptionsPanel.SetActive(true);

        Debug.Log("Connected to Master!");
    }

    /// <summary>
    /// When you've intentionally disconnected or
    /// have become disconnected from the server
    /// </summary>
    /// <param name="cause">Cause.</param>
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        tryingToConnectToMaster = false;
        tryingToConnectToRoom = false;
        Debug.Log(cause);
    }

    public void OnClickConnectToRoom()
    {
        if (!PhotonNetwork.IsConnected)
            return;
        tryingToConnectToRoom = true;

        // don't allow clicking to join room again
        DisableAllButtonsInPanel(onlineOptionsPanel);
        PhotonNetwork.JoinRandomRoom();
    }

    /// <summary>
    /// Always called when creating a room
    /// </summary>
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        tryingToConnectToRoom = false;

        // hide the join room button but reenable it for next time it shows
        //onlineOptionsPanel.SetActive(false);
        EnableAllButtonsInPanel(onlineOptionsPanel);

        Debug.Log("Master: " + PhotonNetwork.IsMasterClient + " | Players in room: " + PhotonNetwork.CurrentRoom.PlayerCount);
        this.isHost = PhotonNetwork.IsMasterClient;
    }

    /// <summary>
    /// Called when the player clicks the start button
    /// </summary>
    public void OnClickStartGame()
    {
        onlineOptionsPanel.SetActive(false);
    }

    /// <summary>
    /// If no room is available, create our own
    /// </summary>
    /// <param name="returnCode">Return code.</param>
    /// <param name="message">Message.</param>
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
    }

    /// <summary>
    /// If a room could not be made, get the error
    /// </summary>
    /// <param name="returnCode">Return code.</param>
    /// <param name="message">Message.</param>
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.Log(message);
        tryingToConnectToRoom = false;
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
            button.enabled = false;
        }
    }
}
