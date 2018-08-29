using UnityEngine;

public class GazeCursor : MonoBehaviour
{
    public GameObject Cursor;
    private GameObject FocusedObject; // The object which user is staring at
    private GazeBuffer buffer; // Gaze stabilizer
    private MeshRenderer meshRenderer;
    private RaycastHit hitInfo;
    private Vector3 gazeOrigin;
    private Vector3 gazeForward;
    
    void Start ()
    {
        meshRenderer = Cursor.GetComponent<MeshRenderer>();
        //meshRenderer = Cursor.gameObject.GetComponentInChildren<MeshRenderer>();
        buffer = new GazeBuffer();
    }
	
	void Update ()
    {
        RaycastHit hitInfo;
        gazeOrigin = Camera.main.transform.position;
        gazeForward = Camera.main.transform.forward;

        if (Physics.Raycast(gazeOrigin, gazeForward, out hitInfo))
        {
            buffer.addSamples(gazeOrigin, gazeForward);
            buffer.UpdateStability(gazeOrigin, gazeForward);
            gazeOrigin = buffer.getStableGazeOrigin();
            gazeForward = buffer.getStableGazeForward();
        }

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
