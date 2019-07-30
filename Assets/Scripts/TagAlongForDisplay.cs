using HoloToolkit.Unity;
using UnityEngine;

/*
 * TagAlongForDisplay script
 * Script for placing info text in front of user
 */

public class TagAlongForDisplay : MonoBehaviour
{
    public float TagalongDistance = 1.75f;
    public float PositionUpdateSpeed = 30f;
    public float SmoothingFactor = 0.5f;
    public float PositionOffsetX, PositionOffsetY, PositionOffsetZ;

    private Interpolator interpolator;
    private Camera mainCamera;

    void Start()
    {
        interpolator = gameObject.GetComponent<Interpolator>();
        interpolator.SmoothLerpToTarget = true;
        interpolator.SmoothPositionLerpRatio = SmoothingFactor;
        mainCamera = Camera.main;
    }

    void Update()
    {
        Vector3 tagalongTargetPosition = mainCamera.transform.position + mainCamera.transform.forward * TagalongDistance;
        tagalongTargetPosition.x = tagalongTargetPosition.x + PositionOffsetX;
        tagalongTargetPosition.y = tagalongTargetPosition.y + PositionOffsetY;
        tagalongTargetPosition.z = tagalongTargetPosition.z + PositionOffsetZ;
        interpolator.PositionPerSecond = PositionUpdateSpeed;
        interpolator.SetTargetPosition(tagalongTargetPosition);

        Vector3 directionToTarget = mainCamera.transform.position - transform.position;

        directionToTarget.y = 0.0f;

        // If we are right next to the camera the rotation is undefined. 
        if (directionToTarget.sqrMagnitude < 0.005f) return;

        transform.rotation = Quaternion.LookRotation(-directionToTarget);
    }
}