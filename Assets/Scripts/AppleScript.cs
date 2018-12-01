using UnityEngine;

public class AppleScript : MonoBehaviour
{
    private bool triggered = false;
    //private WindEffect effect;
    private bool windEnabled = false;
    // Use this for initialization
    void Start()
    {
        /*
        windEnabled = false;
        if (windEnabled)
            effect = GameObject.Find("Wind").GetComponent<WindEffect>();
        */
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if(windEnabled)
            GetComponent<Rigidbody>().AddForce(effect.windDirection * effect.windSpeed);
        */
    }

    void OnCollisionEnter(Collision collision)
    {
        //Get collision object
        GameObject colObject = collision.gameObject;
        if (colObject.name == "Box")
        {
            if(!triggered)
            {
                triggered = true;
                EventManager.TriggerEvent("box_collision");
            }
            gameObject.transform.parent = colObject.transform;
            if(gameObject.GetComponent<Outline>()!=null)
                gameObject.GetComponent<Outline>().enabled = false;
            //ObjectCollectionManager.Instance.disappearBox();
        }
        else
        {
            gameObject.transform.parent = null;
            if(!triggered)
            {
                triggered = true;
                EventManager.TriggerEvent("floor_collision");
            }
            if(gameObject.GetComponent<Outline>()!=null)
                gameObject.GetComponent<Outline>().enabled = false;
            //Destroy(gameObject);
            //ObjectCollectionManager.Instance.disappearBox();
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
