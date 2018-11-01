using UnityEngine;

public class GazeCursor : MonoBehaviour
{
    //Public Variables-For Editor
    public TextMesh StatusText; // Text for debugging (for now)
    public FlowController flowController;
    //Private Variables
    private GameObject FocusedObject; // The object which user is staring at
    //private bool calibrationMode = false;
    private bool calculationMode = false;
    private bool trainingMode = false;
    private float height = .0f;
    //Cached variables
    private Renderer cursorMeshRenderer; // Using this to disable cursor
    private RaycastHit hitInfo;
    private Vector3 gazeOrigin;
    private Vector3 gazeDirection;
    private Camera mainCamera;

    void Start ()
    {
        cursorMeshRenderer = gameObject.GetComponent<Renderer>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        gazeOrigin = mainCamera.transform.position;
        gazeDirection = mainCamera.transform.forward;
        if (Physics.Raycast(gazeOrigin, gazeDirection, out hitInfo))
        {
            cursorMeshRenderer.enabled = true;
            gameObject.transform.position = hitInfo.point;
            gameObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);

            if (calculationMode)
                calculateHeight();
            else if (trainingMode) // Use for gameplay
                refreshFocusedObject();
            else // for !training
                FocusedObject = hitInfo.collider.gameObject;
        }
        else
            cursorMeshRenderer.enabled = false;
    }

    private void calculateHeight()
    {
        //refreshUserHeight
        float tempHeight = Mathf.Abs(hitInfo.point.y - mainCamera.transform.position.y);
        if (height < tempHeight)
            height = tempHeight;
        if (hitInfo.collider.gameObject.CompareTag("Calibration"))
        {
            //Terminate Calculation
            flowController.finishCalculateMode(height);
            calculationMode = false;
        }
    }
    private void refreshFocusedObject()
    {
        if (FocusedObject.CompareTag("User") && FocusedObject != hitInfo.collider.gameObject)
            FocusedObject = hitInfo.collider.gameObject;
    }

    public void setCalculationMode()
    {
        calculationMode = true;
        //calibrationMode = false;
        trainingMode = false;
        FocusedObject = null;
    }

    public void setTrainingMode()
    {
        calculationMode = false;
        //calibrationMode = false;
        trainingMode = true;
        FocusedObject = null;
    }

    public void setGenericUse()
    {
        calculationMode = false;
        //calibrationMode = false;
        trainingMode = false;
        FocusedObject = null;
    }

    public GameObject getFocusedObject()
    {
        return FocusedObject;
    }

}
