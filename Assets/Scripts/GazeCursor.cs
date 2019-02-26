using UnityEngine;

public class GazeCursor : MonoBehaviour
{
    //Public Variables-For Editor
    public FlowController flowController;
    //Private Variables
    private GameObject FocusedObject = null; // The object which user is staring at
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
            if ( hitInfo.collider.gameObject.CompareTag("User") ||
                 hitInfo.collider.gameObject.CompareTag("UI")) // ||
            //hitInfo.collider.gameObject.CompareTag("Dummy"))
                FocusedObject = hitInfo.collider.gameObject;
            else
                FocusedObject = null;
        }
        else
            cursorMeshRenderer.enabled = false;
    }

    public GameObject getFocusedObject()
    {
        return FocusedObject;
    }
}
