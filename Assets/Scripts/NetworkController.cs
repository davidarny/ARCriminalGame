using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Lean.Gui;
using Photon.Pun;
using Photon.Realtime;

[Serializable]
public class LeftRoomEvent : UnityEvent { }
[Serializable]
public class JoinRoomEvent : UnityEvent { }

public class NetworkController : MonoBehaviourPunCallbacks
{
    #region Photon configuration

    [SerializeField]
    private string gameVersion = "1.0";

    [SerializeField]
    private string roomName = "basic";
    [SerializeField]
    private byte maxPlayers = 4;

    #endregion

    #region UI configuration

    [SerializeField]
    private string playNowText;
    [SerializeField]
    private Color playNowActiveColor;
    [SerializeField]
    private Color playNowInactiveColor;

    #endregion

    #region Canvases

    [SerializeField]
    private GameObject lobbyCanvas;
    [SerializeField]
    private GameObject roomCanvas;

    #endregion

    #region UI

    [SerializeField]
    private GameObject playNowButton;

    #endregion

    #region Events

    public LeftRoomEvent leftRoomEvent { get; private set; } = new LeftRoomEvent();
    public JoinRoomEvent joinRoomEvent { get; private set; } = new JoinRoomEvent();

    #endregion


    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = gameVersion;

        var gameController = GameObject.FindObjectOfType<GameController>();
        gameController.gameStateToggledEvent.AddListener(OnFinishGame);

        SetPlayNowButtonColor(playNowInactiveColor);
    }

    #region UI callbacks

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

    private void OnFinishGame(GameState gameState)
    {
        if (gameState != GameState.Finishing)
        {
            return;
        }

        lobbyCanvas.SetActive(true);
        roomCanvas.SetActive(false);

        PhotonNetwork.LeaveRoom();

        leftRoomEvent.Invoke();
    }

    #endregion

    #region PUN callbacks

    public override void OnCreatedRoom()
    {
        Debug.LogFormat("PUN: OnCreatedRoom() was called by PUN with room {0}", PhotonNetwork.CurrentRoom.ToString());
    }

    public override void OnJoinedRoom()
    {
        lobbyCanvas.SetActive(false);
        roomCanvas.SetActive(true);

        joinRoomEvent.Invoke();

        Debug.LogFormat("PUN: OnJoinedRoom() was called by PUN with room {0}", PhotonNetwork.CurrentRoom.ToString());
    }

    public override void OnLeftRoom()
    {
        leftRoomEvent.Invoke();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        lobbyCanvas.SetActive(true);
        roomCanvas.SetActive(false);

        Debug.LogFormat("PUN: OnCreateRoomFailed() was called by PUN with reason {0}", message);
    }

    public override void OnConnectedToMaster()
    {
        SetPlayNowButtonActive();
        SetPlayNowButtonColor(playNowActiveColor);

        Debug.Log("PUN: OnConnectedToMaster() was called by PUN");
    }

    public void OnConnectionFail(DisconnectCause cause)
    {
        Debug.LogFormat("PUN: OnConnectionFail() was called by PUN with reason {0}", cause);
    }

    public void OnFailedToConnectToPhoton(DisconnectCause cause)
    {
        Debug.LogFormat("PUN: OnFailedToConnectToPhoton() was called by PUN with reason {0}", cause);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogFormat("PUN: OnDisconnected() was called by PUN with reason {0}", cause);
    }

    #endregion

    private void SetPlayNowButtonActive()
    {
        var leanButton = playNowButton.GetComponent<LeanButton>();
        leanButton.interactable = true;

        var buttonCap = playNowButton.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject;
        var capText = buttonCap.GetComponent<Text>();
        capText.text = playNowText;
    }

    private void SetPlayNowButtonColor(Color color)
    {
        var leanButton = playNowButton.GetComponent<LeanButton>();
        var buttonCap = playNowButton.transform.GetChild(1).gameObject;
        var capImage = buttonCap.GetComponent<Image>();
        capImage.color = color;
    }
}
