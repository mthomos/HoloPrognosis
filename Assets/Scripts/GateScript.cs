using UnityEngine;

public class GateScript : MonoBehaviour
{
    //For editor
    public Color objectColor;
    //For scripting use
    private bool checkForObject;
    public bool objectInGate = false;
    public GameObject approachingObject = null;

    void Start()
    {

    }

    void Update()
    {

    }

    public bool objectInsideGate(GameObject obj)
    {
        Vector3 objPos = obj.transform.position;
        Vector3 gateSize = GetComponent<Renderer>().bounds.size;
        Rect boxXZ = new Rect(transform.position.x, transform.position.z, gateSize.x / 2, gateSize.z / 2);
        Rect boxXY = new Rect(transform.position.x, transform.position.y, gateSize.x / 2, gateSize.y / 2);

       if (boxXZ.Contains(new Vector2(objPos.x, objPos.z)) && boxXY.Contains(new Vector2(objPos.x, objPos.y)) )
       {
            UtilitiesScript.Instance.EnableOutline(obj, Color.white);
            UtilitiesScript.Instance.EnableOutline(gameObject, Color.white);
            return true;
       }
        else
        {
            UtilitiesScript.Instance.EnableOutline(obj, Color.red);
            UtilitiesScript.Instance.DisableOutline(gameObject);
            return false;
        }
    
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
