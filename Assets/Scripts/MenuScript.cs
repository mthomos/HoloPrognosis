using HoloToolkit.Unity;
using UnityEngine;

public class MenuScript : MonoBehaviour
{
    public float PositionUpdateSpeed = 30f;
    public float SmoothingFactor = 0.5f;
    public float dist = 1f;

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
        Vector3 tagalongTargetPosition = mainCamera.transform.position + mainCamera.transform.forward * 1.5f;
        if (Vector3.Magnitude(tagalongTargetPosition - this.transform.position) > dist)
        {
            interpolator.PositionPerSecond = PositionUpdateSpeed;
            interpolator.SetTargetPosition(tagalongTargetPosition);
            Vector3 directionToTarget = mainCamera.transform.position - transform.position;
            directionToTarget.y = 0.0f;

            if (directionToTarget.sqrMagnitude > 0.005f)
                transform.rotation = Quaternion.LookRotation(-directionToTarget);
        }
    }
}