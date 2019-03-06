using UnityEngine;

public class DebugScript : MonoBehaviour
{
    public UtilitiesScript utilities;
    public GameObject gate;
    public GameObject redPointPrefab;
    public GameObject greenPointPrefab;
    public GameObject bluePointPrefab;
    private Vector3 position = Vector3.zero;
    private Vector3 position2 = Vector3.zero;
    private Vector3 position3 = Vector3.zero;
    private GameObject redO, blueO, greenO;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(gate.transform.forward);
        Vector3 center = gate.GetComponent<Renderer>().bounds.center;
        //Vector3 size = gate.GetComponent<Renderer>().bounds.size;
        Vector3 angles = gate.transform.eulerAngles;
        position.y = center.y;
        position2.y = center.y;
        position3.y = center.y;

        position.x = center.x + Mathf.Sin((angles.y) * Mathf.Deg2Rad) * 0.2f;
        position.z = center.z + Mathf.Cos((angles.y) * Mathf.Deg2Rad) * 0.2f;
        position2.x = gate.transform.position.x + Mathf.Sin((angles.y +90) * Mathf.Deg2Rad) * 0.5f;
        position2.z = gate.transform.position.z + Mathf.Cos((angles.y +90) * Mathf.Deg2Rad) * 0.5f;
        position3.x = center.x + Mathf.Sin((angles.y - 90) * Mathf.Deg2Rad) * 0.2f;
        position3.z = center.z + Mathf.Cos((angles.y - 90) * Mathf.Deg2Rad) * 0.2f;
        /*
        lineRenderer.positionCount = 5;
        var points = new Vector3[6];
        points[0] = cameraPosition;
        points[1] = position2;
        points[2] = position;
        points[3] = position3;
        points[4] = cameraPosition;
        points[5] = position;
        lineRenderer.SetPositions(points);  
        */

        for (float angle = -90f; angle <83f; angle+=4)
        {
            position.x = center.x + Mathf.Sin((angles.y + angle) * Mathf.Deg2Rad) * 0.5f;
            position.z = center.z + Mathf.Cos((angles.y + angle) * Mathf.Deg2Rad) * 0.5f;
            Instantiate(redPointPrefab, position, Quaternion.LookRotation(Vector3.zero));
        }

        blueO = Instantiate(bluePointPrefab, position2, Quaternion.LookRotation(Vector3.zero));
        blueO.transform.Rotate(0, 0, -90f);

    }

    // Update is called once per frame
    void Update()
    {

    }
}
