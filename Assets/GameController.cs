using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Lean.Touch;

public class GameController : MonoBehaviour
{
    private static readonly string GAME_MODEL_TAG = "GameModel";

    public TrackableType trackable;

    public GameObject gunModel;
    public GameObject bloodModel;
    private GameObject currentObject = null;

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

            var spawned = Instantiate(currentObject, pose.position, pose.rotation);
            var selectable = spawned.GetComponent<LeanSelectable>();

            spawned.SetActive(true);
            selectable.Select();
        }
    }

    public void OnGunClick()
    {
        currentObject = gunModel;
    }

    public void OnBloodClick()
    {
        currentObject = bloodModel;
    }
}
