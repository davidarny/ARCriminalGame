using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Lean.Touch;
using UnityEngine.EventSystems;
using Photon.Pun;

public class GameController : MonoBehaviour
{
    private static readonly string GAME_OBJECT_TAG = "GameModel";
    private static readonly string OBJECT_SELECTION_TAG = "ModelSelection";

    [SerializeField]
    private TrackableType trackable;


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

    private GameObject objectToSpawn = null;
    private GameObject selectedObject = null;

    private ARSessionOrigin arSessionOrigin;
    private ARRaycastManager arRaycastManager;

    void Awake()
    {
        arSessionOrigin = FindObjectOfType<ARSessionOrigin>();
        arRaycastManager = arSessionOrigin.GetComponent<ARRaycastManager>();
    }

    public void OnFingerTap(LeanFinger finger)
    {
        var hits = new List<ARRaycastHit>();
        if (arRaycastManager.Raycast(finger.ScreenPosition, hits, trackable))
        {
            var pose = hits[0].pose;

            RaycastHit hit;
            if (Physics.Raycast(finger.GetRay(), out hit) && hit.collider.CompareTag(GAME_OBJECT_TAG))
            {
                return;
            }

            var photonGameObject = PhotonNetwork.Instantiate(objectToSpawn.name, pose.position, pose.rotation);

            var selectionController = photonGameObject.GetComponent<SelectionController>();
            selectionController.leanSelectedEvent.AddListener(OnSelectObject);
            selectionController.leanDeselectedEvent.AddListener(OnDeselectObject);
        }
    }

    private void OnSelectObject(LeanFinger finger)
    {
        RaycastHit hit;
        if (Physics.Raycast(finger.GetRay(), out hit) && hit.collider.CompareTag(GAME_OBJECT_TAG))
        {
            var gameObject = hit.collider.gameObject;
            var outline = gameObject.GetComponent<Outline>();
            outline.OutlineMode = Outline.Mode.OutlineAll;
            selectedObject = gameObject;
        }
    }

    private void OnDeselectObject()
    {
        var outline = selectedObject.GetComponent<Outline>();
        outline.OutlineMode = Outline.Mode.OutlineHidden;
        selectedObject = null;
    }


    private void HideMenuAllItemsOutlines()
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

    private void ShowMenuCurrentItemOutline()
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
        HideMenuAllItemsOutlines();
        ShowMenuCurrentItemOutline();
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
}
