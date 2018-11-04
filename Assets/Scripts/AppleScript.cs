using UnityEngine;

public class AppleScript : MonoBehaviour
{
    private WindEffect effect;
    private bool windEnabled;
    // Use this for initialization
    void Start()
    {
        effect = GameObject.Find("Wind").GetComponent<WindEffect>();
    }

    // Update is called once per frame
    void Update()
    {
        if(windEnabled)
            GetComponent<Rigidbody>().AddForce(effect.windDirection * effect.windSpeed);
    }

    void FixedUpdate()
    {

    }

    void OnCollisionEnter(Collision collision)
    {
        //Get collision object
        GameObject colObject = collision.gameObject;
        if (gameObject.name == "Box")
        {
            EventManager.TriggerEvent("box_collision");
            gameObject.transform.parent = colObject.transform;
            ObjectCollectionManager.Instance.disappearBox();
        }
        else
            EventManager.TriggerEvent("floor_collision");
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
