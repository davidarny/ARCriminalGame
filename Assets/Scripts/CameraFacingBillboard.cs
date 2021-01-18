using UnityEngine;

[ExecuteInEditMode]
public class CameraFacingBillboard : MonoBehaviour
{
    private new Camera camera;

    void Start()
    {
        camera = Camera.main;
    }

    // Orient the camera after all movement is completed this frame to avoid jittering
    void LateUpdate()
    {
        transform.LookAt(transform.position + camera.transform.rotation * Vector3.forward, camera.transform.rotation * Vector3.up);
    }
}
