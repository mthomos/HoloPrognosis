using UnityEngine;

// Just a debug script

public class DebugScript : MonoBehaviour
{
    public UtilitiesScript utilities;
    public GameObject gate;
    public GameObject redPointPrefab;
    public GameObject greenPointPrefab;
    public GameObject bluePointPrefab;
    private Vector3 position = Vector3.zero;
    private GameObject blueO;
    // Start is called before the first frame update
    void Start()
    {
        Vector3 cameraAngles = Camera.main.transform.eulerAngles;
        Vector3 gatePosition = gate.transform.position;
        gatePosition.x = Camera.main.transform.position.x + Mathf.Sin((cameraAngles.y + 90) * Mathf.Deg2Rad) * 0.2f;
        gatePosition.z = Camera.main.transform.position.z + Mathf.Cos((cameraAngles.y + 90) * Mathf.Deg2Rad) * 0.2f;
        gate.transform.position = gatePosition;

        Vector3 center = gate.GetComponent<Renderer>().bounds.center;
        Vector3 angles = gate.transform.eulerAngles;
        position.y = center.y;
        Vector3 dx = Camera.main.transform.position - center;
        float distance = new Vector2(dx.x, dx.z).magnitude;
        Debug.Log("distance:" + distance);
        Debug.Log("dx:" + dx);

        for (float angle = 0f; angle <91f; angle+=5)
        {
            position.x = Camera.main.transform.position.x + Mathf.Sin((cameraAngles.y + angle) * Mathf.Deg2Rad) * distance;
            position.z = Camera.main.transform.position.z + Mathf.Cos((cameraAngles.y + angle) * Mathf.Deg2Rad) * distance;
            Instantiate(redPointPrefab, position, Quaternion.LookRotation(Vector3.up));
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
