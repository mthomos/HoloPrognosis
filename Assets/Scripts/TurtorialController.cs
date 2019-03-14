using System.Collections.Generic;
using UnityEngine;


public class TurtorialController : MonoBehaviour
{
    // Fill in Editor
    public FlowController flowController;
    public GazeCursor gazeCursor;
    public HandsTrackingController handsTrackingController;
    public UiController uiController;
    public GameObject TurtorialMenu;
    public GameObject PointPrefab;
    public GameObject AppleObject;

    public bool turtorialEnabled;
    public int turtorialStep, manipulations;
    private GameObject manipulatedObject;
    private List<GameObject> guidanceObjects = new List<GameObject>();
    private Vector3 applePosition = Vector3.zero;
    private GameObject createdGate;

    void Start()
    {
        turtorialEnabled = false;
        turtorialStep = -1;
    }

    void Update()
    {
        
        if (turtorialStep == 2)
        {
            GateScript gateScript = createdGate.GetComponent<GateScript>();
            if (gateScript.objectInsideGate(manipulatedObject))
            {
                if (!TextToSpeech.Instance.IsSpeaking())
                    TextToSpeech.Instance.StartSpeaking("Apple inside the Circle");
            }
        }
    }

    public void PrepareFirstTurtorial()
    {
        if (manipulatedObject != null)
            Destroy(manipulatedObject);

        turtorialEnabled = true;
        manipulations = 1;
        turtorialStep = 1;
        AppearAppleDemo();

        // Stop Listening for events
        EventManager.StopListening("manipulation_started", ManipulationStarted);
        EventManager.StopListening("manipulation_finished", ManipulationFinished);
    }

    public void PrepareSecondTurtorial()
    {
        if (manipulatedObject != null)
            Destroy(manipulatedObject);

        turtorialEnabled = true;
        manipulations = 1;
        turtorialStep = 2;
        AppearAppleDemo();
        // Create Gate and hide it
        Vector3 cameraPos = Camera.main.transform.position;
        Vector3 angles = Camera.main.transform.eulerAngles;
        Vector3 pos = new Vector3(cameraPos.x + Mathf.Sin((angles.y) * Mathf.Deg2Rad) * 1.5f,
                                   cameraPos.y,
                                   cameraPos.z + Mathf.Cos((angles.y) * Mathf.Deg2Rad) * 1.5f);
        Quaternion rot = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
        ObjectCollectionManager.Instance.CreateGate(pos, rot);
        ObjectCollectionManager.Instance.DisappearGate();
        // Listen Events
        EventManager.StartListening("manipulation_started", ManipulationStarted);
        EventManager.StartListening("manipulation_finished", ManipulationFinished);
    }

    private void AppearAppleDemo()
    {
        // Appear an apple in front of user
        manipulatedObject = Instantiate(AppleObject);
        //Destroy Rigidbody to disable gravity
        if (manipulatedObject.GetComponent<Rigidbody>() != null)
            Destroy(manipulatedObject.GetComponent<Rigidbody>());
        if (applePosition == Vector3.zero)
            applePosition = Camera.main.transform.position + Camera.main.transform.forward * 2.0f;

        manipulatedObject.transform.position = applePosition;

    }

    private void ManipulationStarted()
    {
        if (!turtorialEnabled || turtorialStep != 2)
            return;

        //Appear Gate according to hand
        ObjectCollectionManager.Instance.AppearGate(1.0f, 2.0f, manipulations % 2 == 1 ? true : false);
        createdGate = ObjectCollectionManager.Instance.GetCreatedGate();
        //Get manipulatedObject
        manipulatedObject = handsTrackingController.GetManipulatedObject();
        //Delete parent
        if (manipulatedObject != null)
            manipulatedObject.transform.parent = null;
        //Create balls to guide user
        CreateGuidance(manipulations % 2 == 1 ? true : false);
    }

    private void CreateGuidance(bool toRight)
    {
        Vector3 center = ObjectCollectionManager.Instance.GetCreatedGate().GetComponent<Renderer>().bounds.center;
        Vector3 angles = ObjectCollectionManager.Instance.GetCreatedGate().transform.eulerAngles;
        //Vector3 dx = Camera.main.transform.position - manipulatedObject.transform.position;
        float distance = 1.5f; // new Vector2(dx.x, dx.z).magnitude;
        Vector3 position = new Vector3(0, center.y, 0);

        if (toRight)
        {
            for (float angle = 0; angle < 85f; angle += 4)
            {
                position.x = center.x + Mathf.Sin((angles.y + angle) * Mathf.Deg2Rad) * distance;
                position.z = center.z + Mathf.Cos((angles.y + angle) * Mathf.Deg2Rad) * distance;
                guidanceObjects.Add(Instantiate(PointPrefab, position, Quaternion.LookRotation(Vector3.zero)));
            }
        }
        else
        {
            for (float angle = -84; angle < 1.0f; angle += 4)
            {
                position.x = center.x + Mathf.Sin((angles.y + angle) * Mathf.Deg2Rad) * distance;
                position.z = center.z + Mathf.Cos((angles.y + angle) * Mathf.Deg2Rad) * distance;
                guidanceObjects.Add(Instantiate(PointPrefab, position, Quaternion.LookRotation(Vector3.zero)));
            }
        }
    }

    private void ManipulationFinished()
    {
         if (!turtorialEnabled || turtorialStep != 2)
            return;

        // Destroy guidance
        foreach (GameObject point in guidanceObjects)
            Destroy(point);
        guidanceObjects.Clear();
        ObjectCollectionManager.Instance.DisappearGate();
        manipulatedObject = null;
        createdGate = null;
    }

    public void FinishTurtorial()
    {
        // Destroy guidance
        foreach (GameObject point in guidanceObjects)
            Destroy(point);
        guidanceObjects.Clear();

        if (manipulatedObject != null)
        {
            Destroy(manipulatedObject);
            manipulatedObject = null;
        }
        turtorialEnabled = false;
        turtorialStep = -1;
        // Stop Listening for events
        EventManager.StopListening("manipulation_started", ManipulationStarted);
        EventManager.StopListening("manipulation_finished", ManipulationFinished);
    }
}
