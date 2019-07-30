using UnityEngine;

/*
 * Gate script
 * Script for controlling gate behaviour
 */

public class GateScript : MonoBehaviour
{
    public bool gateOpened = false;
    private Renderer gateRenderer;

    void Start()
    {

    }

    public bool objectInsideGate(GameObject obj)
    {
        if (obj == null)
            return false;

        if (gateRenderer == null)
            gateRenderer = GetComponent<Renderer>();

        if (gateRenderer == null)
            return false;

        if (!gateOpened)
            return false;

        if (gateRenderer.bounds.Contains(obj.transform.position))
        {
            UtilitiesScript.Instance.EnableOutline(gameObject, Color.green, true);
            return true;
        }
        else
        {
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
