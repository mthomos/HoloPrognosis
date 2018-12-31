using UnityEngine;

public class GateScript : MonoBehaviour
{
    //For editor
    public Color objectColor;
    //For scripting use
    public bool objectInGate = false;
    public GameObject approachingObject = null;

    private bool checkForObject;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Renderer>().material.color = objectColor;
    }

    // Update is called once per frame
    void Update()
    {
        if (checkForObject && approachingObject != null)
        {
            objectInGate = objectInsideTree(approachingObject);
        }
    }

    public bool objectInsideTree(GameObject obj)
    {
        Vector3 gatePos = this.transform.position;
        Vector3 objPos = obj.transform.position;
        Vector3 gateSize = this.GetComponent<Renderer>().bounds.size;
        float radius = (gateSize.y + gateSize.z) / 2;
        float gateObjdistance2D = new Vector2(gatePos.y - objPos.y, gatePos.z - objPos.z).magnitude;
        //Check if is inside gate circle
        if (Mathf.Abs(gatePos.x - objPos.x) < gateSize.x && gateObjdistance2D <= radius)
            return true;

        return false;
    }

    public void setCheckStatus(bool check)
    {
        checkForObject = check;
    }

    public void setApproachingObject(GameObject obj)
    {
        approachingObject = obj;
    }
}
