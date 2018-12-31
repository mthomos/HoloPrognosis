using UnityEngine;

public class AppleScript : MonoBehaviour
{
    private bool triggered = false; //Collision Triggered
    private WindEffect effect;
    private bool windEnabled = false;
    // Use this for initialization
    void Start()
    {
        if (windEnabled)
            effect = GameObject.Find("Wind").GetComponent<WindEffect>();
    }

    // Update is called once per frame
    void Update()
    {
        if(windEnabled)
            GetComponent<Rigidbody>().AddForce(effect.windDirection * effect.windSpeed);
    }

    void OnCollisionEnter(Collision collision)
    {
        //Get collision object
        GameObject colObject = collision.gameObject;
        if (colObject.CompareTag("Dummy"))
        {
            if(!triggered)
            {
                triggered = true;
                EventManager.TriggerEvent("box_collision");
                gameObject.transform.parent = colObject.transform;
                if (gameObject.GetComponent<Outline>() != null)
                    gameObject.GetComponent<Outline>().enabled = false;
            }
        }
        else
        {
            gameObject.transform.parent = null;
            if(!triggered)
            {
                triggered = true;
                EventManager.TriggerEvent("floor_collision");
                gameObject.transform.parent = colObject.transform;
                if (gameObject.GetComponent<Outline>() != null)
                    gameObject.GetComponent<Outline>().enabled = false;
            }
        }
    }

    public void disableWind()
    {
        windEnabled = false;
    }

    public void enableWind()
    {
        windEnabled = true;
    }
}
