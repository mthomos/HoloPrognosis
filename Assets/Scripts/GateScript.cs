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

        if (GetComponent<Renderer>().bounds.Contains(obj.transform.position))
        {
            if (!TextToSpeech.Instance.IsSpeaking())
                TextToSpeech.Instance.StartSpeaking("Apple inside the Circle");

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
