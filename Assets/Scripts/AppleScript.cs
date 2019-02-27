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
        //Signal the event
        Debug.Log("collision_"+colObject.name + " with " + gameObject.name);
        if (colObject.CompareTag("Dummy") && !triggered)
            EventManager.TriggerEvent("box_collision");
        else if (!colObject.CompareTag("Dummy") && !triggered)
           EventManager.TriggerEvent("floor_collision");

        if(!triggered)
        {
            triggered = true;
            gameObject.transform.parent = null;
            //gameObject.transform.parent = colObject.transform;
            if (gameObject.GetComponent<Outline>() != null)
                gameObject.GetComponent<Outline>().enabled = false;
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
