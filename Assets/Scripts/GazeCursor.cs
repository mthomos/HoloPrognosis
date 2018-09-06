using UnityEngine;

public class GazeCursor : MonoBehaviour
{
    //Public Variables-For Editor
    //public GameObject Cursor;
    public bool useBuffer = false;
    //Private Variables
    private GameObject FocusedObject; // The object which user is staring at
    private GazeBuffer buffer; // Gaze stabilizer
    private Renderer cursorMeshRenderer; // Using this to disable cursor
    //private RaycastHit hitInfo; //Better for this variable to be cached
    private Vector3 gazeOrigin; // same
    private Vector3 gazeDirection; // samme
    
    void Start ()
    {
        buffer = new GazeBuffer();
        cursorMeshRenderer = gameObject.GetComponentInChildren<Renderer>();
    }
	
	void Update ()
    {
        gazeOrigin = Camera.main.transform.position;
        gazeDirection = Camera.main.transform.forward;
        RaycastHit hitInfo;


        if (Physics.Raycast(gazeOrigin, gazeDirection, out hitInfo) && useBuffer)
        {
            buffer.addSamples(gazeOrigin, gazeDirection);
            buffer.UpdateStability(gazeOrigin, gazeDirection);
            gazeOrigin = buffer.getStableGazeOrigin();
            gazeDirection = buffer.getStableGazeForward();
        }


        if (Physics.Raycast(gazeOrigin, gazeDirection, out hitInfo))
        {
            cursorMeshRenderer.enabled = true;
            this.transform.position = hitInfo.point;
            this.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            FocusedObject = hitInfo.collider.gameObject;
        }
        else
        {
            cursorMeshRenderer.enabled = false;
            FocusedObject = null;
        }
    }

    public GameObject getFocusedObject()
    {
        return FocusedObject;
    }
}
