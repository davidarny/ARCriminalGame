using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Lean.Touch;
using UnityEngine.EventSystems;
using Photon.Pun;
using System.Linq;

[Serializable]
public class FlashlightToggledEvent : UnityEvent<FlashState> { }
[Serializable]
public class GameStateToggledEvent : UnityEvent<GameState> { }

public class GameController : MonoBehaviour
{
    private static readonly string GAME_OBJECT_TAG = "GameModel";
    private static readonly string OBJECT_SELECTION_TAG = "ModelSelection";
    private static readonly string SECTION_INFO_TAG = "SectionInfo";

    #region Configuration

    [SerializeField]
    private TrackableType trackable;
    [SerializeField]
    private string preparingStageText;
    [SerializeField]
    private string playingStageText;

    #endregion

    #region Events

    public FlashlightToggledEvent flashlightToggleEvent { get; private set; } = new FlashlightToggledEvent();
    public GameStateToggledEvent gameStateToggledEvent { get; private set; } = new GameStateToggledEvent();

    #endregion

    #region Crime objects

    [SerializeField]
    private GameObject gun;
    [SerializeField]
    private GameObject blood;
    [SerializeField]
    private GameObject knife;
    [SerializeField]
    private GameObject bullet;
    [SerializeField]
    private GameObject footprints;
    [SerializeField]
    private GameObject fingerprint;
    [SerializeField]
    private GameObject body;
    [SerializeField]
    private GameObject hair;

    #endregion

    #region Current state

    private GameState gameState = GameState.Preparing;
    private FlashState flashState = FlashState.Off;
    private GameObject objectToSpawn = null;
    private GameObject selectedObject = null;
    private HashSet<GameObject> hiddenObjects = new HashSet<GameObject>();

    #endregion

    #region UI

    [SerializeField]
    private GameObject playButton;
    [SerializeField]
    private GameObject flashButton;
    [SerializeField]
    private GameObject finishButton;
    [SerializeField]
    private GameObject objectModalButton;
    [SerializeField]
    private GameObject stageText;

    #endregion

    #region AR

    private ARSessionOrigin arSessionOrigin;
    private ARRaycastManager arRaycastManager;

    #endregion

    void Awake()
    {
        arSessionOrigin = FindObjectOfType<ARSessionOrigin>();
        arRaycastManager = arSessionOrigin.GetComponent<ARRaycastManager>();
    }

    void Start()
    {
        var networkController = GameObject.FindObjectOfType<NetworkController>();
        networkController.leftRoomEvent.AddListener(OnLeftRoom);
        networkController.joinRoomEvent.AddListener(OnJoinRoom);
    }

    #region LeanTouch callbacks
    public void OnFingerTap(LeanFinger finger)
    {
        if (gameState == GameState.Playing)
        {
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(finger.GetRay(), out hit) && hit.collider.CompareTag(GAME_OBJECT_TAG))
        {
            return;
        }

        var hits = new List<ARRaycastHit>();
        if (arRaycastManager.Raycast(finger.ScreenPosition, hits, trackable))
        {
            var pose = hits[0].pose;

            var photonGameObject = PhotonNetwork.Instantiate(objectToSpawn.name, pose.position, pose.rotation);

            var selectionController = photonGameObject.GetComponent<SelectionController>();
            selectionController.leanSelectedEvent.AddListener(OnSelectObject);
            selectionController.leanDeselectedEvent.AddListener(OnDeselectObject);
        }
    }

    #endregion

    #region Show/hide objects

    private void PrepareObjectForPlaying(GameObject gameObject, bool hide)
    {
        // ----- Hide object if hideable

        var hideableDecorator = gameObject.GetComponent<HideableDecorator>();
        if (hideableDecorator.isHideable)
        {
            hideableDecorator.isHidden = hide;
            gameObject.SetActive(!hide);

            if (hide)
            {
                hiddenObjects.Add(gameObject);
            }
            else
            {
                hiddenObjects.Remove(gameObject);
            }
        }

        // -----

        // ----- Disable translate/rotate/scale

        gameObject.GetComponent<LeanDragTranslate>().enabled = false;
        gameObject.GetComponent<LeanTwistRotate>().enabled = false;
        gameObject.GetComponent<LeanPinchScale>().enabled = false;

        // -----
    }

    [PunRPC]
    private void PrepareObjectsForPlaying(bool hide)
    {
        var allGameObjects = new List<GameObject>();

        allGameObjects.AddRange(GameObject.FindGameObjectsWithTag(GAME_OBJECT_TAG));
        allGameObjects.AddRange(hiddenObjects);

        foreach (var gameObject in allGameObjects.ToArray())
        {
            PrepareObjectForPlaying(gameObject, hide);
        }
    }

    #endregion

    #region Toolbar callbacks

    public void OnGameReadyTap()
    {
        gameState = GameState.Playing;
        gameStateToggledEvent.Invoke(gameState);

        playButton.SetActive(false);
        flashButton.SetActive(true);
        finishButton.SetActive(true);
        objectModalButton.SetActive(false);

        var stageTextComp = stageText.GetComponent<UnityEngine.UI.Text>();
        stageTextComp.text = playingStageText;

        HideSelectionOutlines();

        PrepareObjectsForPlaying(true);

        if (PhotonNetwork.IsMasterClient)
        {
            var photonView = PhotonView.Get(this);
            photonView.RPC("ToggleHideableGameObjects", RpcTarget.Others, new object[] { true });
        }
    }

    public void OnFinishTap()
    {
        var gameObjects = GameObject.FindGameObjectsWithTag(GAME_OBJECT_TAG);
        foreach (var gameObject in gameObjects)
        {
            var photonView = gameObject.GetComponent<PhotonView>();
            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(photonView);
            }
            else
            {
                gameObject.SetActive(false);
                Destroy(gameObject, 1f);
            }
        }

        gameState = GameState.Finishing;
        gameStateToggledEvent.Invoke(gameState);
        flashState = FlashState.Off;
        flashlightToggleEvent.Invoke(flashState);
    }

    #endregion

    #region PUN connection callbacks
    private void OnLeftRoom()
    {
        playButton.SetActive(true);
        flashButton.SetActive(false);
        finishButton.SetActive(false);
        objectModalButton.SetActive(true);

        var stageTextComp = stageText.GetComponent<UnityEngine.UI.Text>();
        stageTextComp.text = preparingStageText;

        gameState = GameState.Preparing;
        gameStateToggledEvent.Invoke(gameState);
    }

    private void OnJoinRoom()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            OnGameReadyTap();
        }
    }

    #endregion

    #region Flashlight callbacks

    public void OnFlashlightTap()
    {
        switch (flashState)
        {
            case FlashState.Off:
                flashState = FlashState.On;
                break;
            case FlashState.On:
            default:
                flashState = FlashState.Off;
                break;
        }

        flashlightToggleEvent.Invoke(flashState);

        PrepareObjectsForPlaying(!FlashlightUtils.FlashStateToBool(flashState));
    }

    #endregion

    #region QuickOutline callbacks
    private void OnSelectObject(LeanFinger finger)
    {
        RaycastHit hit;
        if (Physics.Raycast(finger.GetRay(), out hit) && hit.collider.CompareTag(GAME_OBJECT_TAG))
        {
            var gameObject = hit.collider.gameObject;
            var outline = gameObject.GetComponent<Outline>();

            outline.OutlineMode = Outline.Mode.OutlineAll;
            selectedObject = gameObject;

            var sectionInfo = gameObject.FindObjectWithTag(SECTION_INFO_TAG).FirstOrDefault();
            sectionInfo.SetActive(true);
        }
    }

    private void OnDeselectObject()
    {
        var outline = selectedObject.GetComponent<Outline>();
        outline.OutlineMode = Outline.Mode.OutlineHidden;

        var sectionInfo = selectedObject.FindObjectWithTag(SECTION_INFO_TAG).FirstOrDefault();
        sectionInfo?.SetActive(false);

        selectedObject = null;
    }

    #endregion

    #region UI callbacks

    private void HideSelectionOutlines()
    {
        var items = GameObject.FindGameObjectsWithTag(OBJECT_SELECTION_TAG);

        foreach (var item in items)
        {
            var outline = item.GetComponentInChildren<UnityEngine.UI.Outline>();
            if (outline != null)
            {
                outline.enabled = false;
            }
        }
    }

    private void ShowSelectionOutline()
    {
        var current = EventSystem.current.currentSelectedGameObject;
        var outline = current.GetComponentInChildren<UnityEngine.UI.Outline>();
        if (outline != null)
        {
            outline.enabled = true;
        }
    }
    private void HandleSelectMenuItem(Action action)
    {
        HideSelectionOutlines();
        ShowSelectionOutline();
        action.Invoke();
    }

    public void OnGunSelect()
    {
        HandleSelectMenuItem(() => objectToSpawn = gun);
    }

    public void OnBloodSelect()
    {
        HandleSelectMenuItem(() => objectToSpawn = blood);
    }

    public void OnKnifeSelect()
    {
        HandleSelectMenuItem(() => objectToSpawn = knife);
    }

    public void OnBulletSelect()
    {
        HandleSelectMenuItem(() => objectToSpawn = bullet);
    }

    public void OnFootprintSelect()
    {
        HandleSelectMenuItem(() => objectToSpawn = footprints);
    }

    public void OnBodySelect()
    {
        HandleSelectMenuItem(() => objectToSpawn = body);
    }

    public void OnFingerprintSelect()
    {
        HandleSelectMenuItem(() => objectToSpawn = fingerprint);
    }

    public void OnHairSelect()
    {
        HandleSelectMenuItem(() => objectToSpawn = hair);
    }

    #endregion
}
