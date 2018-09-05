using UnityEngine;

public class GazeCursor : MonoBehaviour
{
    //Public Variables-For Editor
    //public GameObject Cursor;
    //Private Variables
    private GameObject FocusedObject; // The object which user is staring at
    private GazeBuffer buffer; // Gaze stabilizer
    private MeshRenderer cursorMeshRenderer; // Using this to disable cursor
    //private RaycastHit hitInfo; //Better for this variable to be cached
    private Vector3 gazeOrigin; // same
    private Vector3 gazeDirection; // same
    
    void Start ()
    {
        buffer = new GazeBuffer();
        cursorMeshRenderer = gameObject.GetComponentInChildren<MeshRenderer>();
    }
	
	void Update ()
    {
        gazeOrigin = Camera.main.transform.position;
        gazeDirection = Camera.main.transform.forward;
        RaycastHit hitInfo;
        /*
        if (Physics.Raycast(gazeOrigin, gazeDirection, out hitInfo))
        {
            buffer.addSamples(gazeOrigin, gazeDirection);
            buffer.UpdateStability(gazeOrigin, gazeDirection);
            gazeOrigin = buffer.getStableGazeOrigin();
            gazeDirection = buffer.getStablegazeDirection();
        }
        */
        if (Physics.Raycast(gazeOrigin, gazeDirection, out hitInfo))
        {
            cursorMeshRenderer.enabled = true;
            this.transform.position = hitInfo.point;
            this.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            FocusedObject = hitInfo.collider.gameObject;
        }
        else
        {
            /*
            Vector3 tagalongTargetPosition = Camera.main.transform.position + Camera.main.transform.forward * 1.75f;
            this.transform.position = tagalongTargetPosition;

            Vector3 directionToTarget = Camera.main.transform.position - this.transform.position;
            directionToTarget.y = 0.0f;

            if (directionToTarget.sqrMagnitude >= 0.005f)
                transform.rotation = Quaternion.LookRotation(-directionToTarget);
            */

            cursorMeshRenderer.enabled = false;
            FocusedObject = null;
        }
    }

    public GameObject getFocusedObject()
    {
        return FocusedObject;
    }
}
