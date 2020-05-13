using System;
using UnityEngine;

public class EyeTracking : MonoBehaviour
{
    public Transform LeftEyeController;
    public Transform RightEyeController;
    public float MinDistance;
    public Camera CameraToLookAt;

    void Start()
    {
        if (LeftEyeController == null)
            LeftEyeController = this.transform.Find("Armature/Hips/Spine/Chest/Neck/Head/LeftEye");
        if (LeftEyeController == null)
            RightEyeController = this.transform.Find("Armature/Hips/Spine/Chest/Neck/Head/RightEye");
    }

    void Update()
    {
        float cloestDist = 0;
        foreach (Camera camera in Camera.allCameras)
        {
            if (camera == Camera.main)
                continue;
            var dist = Vector3.Distance(camera.transform.position, transform.position);
            if (dist < MinDistance && dist > 0.3)
            {
                cloestDist = dist;
                CameraToLookAt = camera;
            }
        }
        if (cloestDist == 0)
            CameraToLookAt = null;
        if (LeftEyeController != null && RightEyeController != null && CameraToLookAt != null)
        {
            Vector3 cameraPos = CameraToLookAt.transform.position;
            var targetRotation = Quaternion.LookRotation(cameraPos - LeftEyeController.position, LeftEyeController.up);
            var targetRotation2 = Quaternion.LookRotation(cameraPos - RightEyeController.position, RightEyeController.up);

            // Smoothly rotate towards the target point.
            LeftEyeController.rotation = Quaternion.Slerp(LeftEyeController.rotation, targetRotation, 10 * Time.deltaTime);
            RightEyeController.rotation = Quaternion.Slerp(RightEyeController.rotation, targetRotation2, 10 * Time.deltaTime);
        }
        else
        {
            var targetRotation = new Quaternion(0, 0, 0, 1);
            // Smoothly rotate towards the target point.
            LeftEyeController.localRotation = Quaternion.Slerp(LeftEyeController.localRotation, targetRotation, 10 * Time.deltaTime);
            RightEyeController.localRotation = Quaternion.Slerp(RightEyeController.localRotation, targetRotation, 10 * Time.deltaTime);
        }
    }
}
