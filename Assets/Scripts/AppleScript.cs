using UnityEngine;

/*
 * Apple script
 * Script attached to apple object to define properties
 * and to events.
 * There is also code for wind effect but no being used or maintained
 */
 
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

    void OnCollisionEnter(Collision collision)
    {
        //Get collision object
        GameObject colObject = collision.gameObject;
        //Signal the event
        if (colObject.CompareTag("Dummy") && !triggered)
            EventManager.TriggerEvent("box_collision");
        else if (!colObject.CompareTag("Dummy") && !triggered)
           EventManager.TriggerEvent("floor_collision");

        if(!triggered)
        {
            triggered = true;
            gameObject.transform.parent = null;
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
