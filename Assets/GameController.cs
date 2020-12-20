using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Lean.Touch;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    private static readonly string GAME_MODEL_TAG = "GameModel";
    private static readonly string MODEL_SELECTION_TAG = "ModelSelection";

    public TrackableType trackable;

    public GameObject gun;
    public GameObject blood;
    public GameObject knife;
    public GameObject bullet;
    public GameObject footprint;
    public GameObject fingerprint;
    public GameObject body;
    public GameObject hair;


    private GameObject current = null;

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
            if (Physics.Raycast(finger.GetRay(), out hit) && hit.collider.CompareTag(GAME_MODEL_TAG))
            {
                return;
            }

            var spawned = Instantiate(current, pose.position, pose.rotation);
            spawned.SetActive(true);
        }
    }

    private void HideAllOutlines()
    {
        var items = GameObject.FindGameObjectsWithTag(MODEL_SELECTION_TAG);

        foreach (var item in items)
        {
            var outline = item.GetComponentInChildren<Outline>();
            if (outline != null)
            {
                outline.enabled = false;
            }
        }
    }

    private void ShowCurrentOutline()
    {
        var current = EventSystem.current.currentSelectedGameObject;
        var outline = current.GetComponentInChildren<Outline>();
        if (outline != null)
        {
            outline.enabled = true;
        }
    }

    private void HandleSelect(Action action)
    {
        HideAllOutlines();
        ShowCurrentOutline();
        action.Invoke();
    }

    public void OnGunSelect()
    {
        HandleSelect(() => current = gun);
    }

    public void OnBloodSelect()
    {
        HandleSelect(() => current = blood);
    }

    public void OnKnifeSelect()
    {
        HandleSelect(() => current = knife);
    }

    public void OnBulletSelect()
    {
        HandleSelect(() => current = bullet);
    }

    public void OnFootprintSelect()
    {
        HandleSelect(() => current = footprint);
    }

    public void OnBodySelect()
    {
        HandleSelect(() => current = body);
    }

    public void OnFingerprintSelect()
    {
        HandleSelect(() => current = fingerprint);
    }

    public void OnHairSelect()
    {
        HandleSelect(() => current = hair);
    }
}
