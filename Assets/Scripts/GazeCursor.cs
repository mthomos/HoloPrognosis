using UnityEngine;

public class GazeCursor : MonoBehaviour
{
    //Public Variables-For Editor
    public GameObject Cursor;
    //Private Variables
    private GameObject FocusedObject; // The object which user is staring at
    private GazeBuffer buffer; // Gaze stabilizer
    private MeshRenderer meshRenderer; // Using this to disable cursor
    private RaycastHit hitInfo; //Better for this variable to be cached
    private Vector3 gazeOrigin; // same
    private Vector3 gazeForward; // same
    
    void Start ()
    {
        meshRenderer = Cursor.GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;
        buffer = new GazeBuffer();
    }
	
	void Update ()
    {
        gazeOrigin = Camera.main.transform.position;
        gazeForward = Camera.main.transform.forward;
        /*
        if (Physics.Raycast(gazeOrigin, gazeForward, out hitInfo))
        {
            buffer.addSamples(gazeOrigin, gazeForward);
            buffer.UpdateStability(gazeOrigin, gazeForward);
            gazeOrigin = buffer.getStableGazeOrigin();
            gazeForward = buffer.getStableGazeForward();
        }
        */
        if (Physics.Raycast(gazeOrigin, gazeForward, out hitInfo))
        {
            meshRenderer.enabled = true;
            Cursor.transform.position = hitInfo.transform.position;
            Cursor.transform.rotation = hitInfo.transform.rotation;
            FocusedObject = hitInfo.collider.gameObject;
        }
        else
        {
            meshRenderer.enabled = false;
            FocusedObject = null;
        }
    }

    public GameObject getFocusedObject()
    {
        return FocusedObject;
    }
}
