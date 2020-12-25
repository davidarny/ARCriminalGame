using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private string gameVersion = "1.0";

    [SerializeField]
    private string roomName = "basic";
    [SerializeField]
    private byte maxPlayers = 4;

    [SerializeField]
    private GameObject lobbyCanvas;
    [SerializeField]
    private GameObject roomCanvas;

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        // if (PhotonNetwork.IsConnected)
        // {
        //     PhotonNetwork.JoinRandomRoom();
        // }
        // else
        // {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = gameVersion;
        // }
    }

    public void OnClickPlay()
    {
        if (!PhotonNetwork.IsConnected)
        {
            return;
        }

        var options = new RoomOptions();
        options.MaxPlayers = maxPlayers;
        options.PublishUserId = true;

        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
    }

    public override void OnCreatedRoom()
    {
        lobbyCanvas.SetActive(false);
        roomCanvas.SetActive(true);
    }

    public override void OnJoinedRoom()
    {
        lobbyCanvas.SetActive(false);
        roomCanvas.SetActive(true);

        Debug.LogFormat("PUN: OnJoinedRoom() was called by PUN with room {0}", PhotonNetwork.CurrentRoom.ToString());
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        lobbyCanvas.SetActive(true);
        roomCanvas.SetActive(false);

        Debug.LogFormat("PUN: OnCreateRoomFailed() was called by PUN with reason {0}", message);

    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("PUN: OnConnectedToMaster() was called by PUN");
    }


    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogFormat("PUN: OnDisconnected() was called by PUN with reason {0}", cause);
    }
}
