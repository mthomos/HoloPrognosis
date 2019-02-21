using UnityEngine;

public class GateScript : MonoBehaviour
{
    //For editor
    public Color gateColor;
    //For scripting use
    private bool objectInGate;
    public bool gateOpened;

    void Start()
    {
        if (gateColor != null)
        {
            Renderer rend = GetComponent<Renderer>();
            rend.material.shader = Shader.Find("_Color");
            rend.material.SetColor("_Color", gateColor);
        }
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
        Rect boxXZ = new Rect(transform.position.x, transform.position.z, gateSize.x / 2, gateSize.z / 2);
        Rect boxXY = new Rect(transform.position.x, transform.position.y, gateSize.x / 2, gateSize.y / 2);

       if (boxXZ.Contains(new Vector2(objPos.x, objPos.z)) && boxXY.Contains(new Vector2(objPos.x, objPos.y)) )
       {
            UtilitiesScript.Instance.EnableOutline(obj, Color.white, false);
            UtilitiesScript.Instance.EnableOutline(gameObject, Color.white, false);
            return true;
       }
        else
        {
            UtilitiesScript.Instance.EnableOutline(obj, Color.blue, false);
            UtilitiesScript.Instance.DisableOutline(gameObject);
            return false;
        }
    
    }
}
