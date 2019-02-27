using UnityEngine;

public class GateScript : MonoBehaviour
{
    public bool gateOpened = false;

    void Start()
    {
    
    }

    void Update()
    {

    }

    public bool objectInsideGate(GameObject obj)
    {
        if (!gateOpened || obj == null)
            return false;

        Vector3 objPos = obj.transform.position;
        Vector3 gateSize = GetComponent<Renderer>().bounds.size;
        Vector3 maxD = GetComponent<Renderer>().bounds.max;
        Vector3 minD = GetComponent<Renderer>().bounds.min;
        Vector3 points = maxD + minD / 2;
        //Rect boxXZ = new Rect(points.x, points.z, gateSize.x / 2, gateSize.z / 2);
        //Rect boxXY = new Rect(points.x, points.y, gateSize.x / 2, gateSize.y / 2);
        Rect boxXZ = new Rect(transform.position.x , transform.position.z , gateSize.x / 2, gateSize.z / 2);
        Rect boxXY = new Rect(transform.position.x , transform.position.y , gateSize.x / 2, gateSize.y / 2);

       if (boxXZ.Contains(new Vector2(objPos.x, objPos.z)) && boxXY.Contains(new Vector2(objPos.x, objPos.y)) )
       {
            UtilitiesScript.Instance.EnableOutline(obj, Color.white, true);
            UtilitiesScript.Instance.EnableOutline(gameObject, Color.white, true);
            return true;
       }
        else
        {
            UtilitiesScript.Instance.DisableOutline(obj);
            UtilitiesScript.Instance.DisableOutline(gameObject);
            return false;
        }
    
    }

    public void enableCollider()
    {
        GetComponent<MeshCollider>().enabled = true;
    }

    public void disableCollider()
    {
        GetComponent<MeshCollider>().enabled = false;
    }
}
