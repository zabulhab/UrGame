using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks {
    [SerializeField]
    private Button btnConnectToMaster;
    [SerializeField]
    private GameObject onlineOptionsPanel;
    [SerializeField]
    private Button btnConnectToHost;

    private bool tryingToConnectToMaster;
    private bool tryingToConnectToRoom;

	// Use this for initialization
	private void Start () 
    {
        tryingToConnectToMaster = false;
        tryingToConnectToRoom = false;
    }
	
	// Update is called once per frame
	private void Update () 
    {
        // if trying to go online
        btnConnectToMaster.gameObject.SetActive(!PhotonNetwork.IsConnected && !tryingToConnectToMaster);
        btnConnectToHost.gameObject.SetActive(PhotonNetwork.IsConnected && !tryingToConnectToMaster && !tryingToConnectToRoom);
        //btnConnectToHost.gameObject.
	}

    public void OnClickConnectToMaster()
    {
        PhotonNetwork.OfflineMode = false; // set to false by default
        PhotonNetwork.NickName = "PlayerName";
        PhotonNetwork.AutomaticallySyncScene = true; // to call PhotonNetwork.LoadLevel()
        PhotonNetwork.GameVersion = "v1";

        tryingToConnectToMaster = true;
        PhotonNetwork.ConnectUsingSettings();
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

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        tryingToConnectToMaster = false;
        Debug.Log("Connected to Master!");
    }

    public void OnClickConnectToRoom()
    {
        if (PhotonNetwork.IsConnected)
            return;
        tryingToConnectToRoom = true;
        PhotonNetwork.JoinRandomRoom();
    }

    /// <summary>
    /// Always called when creating a room
    /// </summary>
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        tryingToConnectToRoom = false;
        Debug.Log("Master: " + PhotonNetwork.IsMasterClient + " | Players in room: " + PhotonNetwork.CurrentRoom.PlayerCount);
    }

    /// <summary>
    /// If no room available
    /// </summary>
    /// <param name="returnCode">Return code.</param>
    /// <param name="message">Message.</param>
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.Log(message);
        tryingToConnectToRoom = false;
    }
}
