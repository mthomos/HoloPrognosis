using System;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;


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

    public bool turtorialEnabled, manipulationInProgress;
    private int turtorialStep, manipulations;
    private GameObject manipulatedObject;
    private List<GameObject> guidanceObjects = new List<GameObject>();
    private Vector3 applePosition = Vector3.zero;

    void Start()
    {
        turtorialEnabled = false;
        turtorialStep = -1;
        manipulations = 1;
        EventManager.StartListening("tap", tapUiReceived);
    }

    void Update()
    {
        if (turtorialEnabled && turtorialStep == 2)
        {

        }
    }

    public void PrepareFirstTurtorial()
    {
        if (manipulatedObject != null)
            Destroy(manipulatedObject);

        turtorialEnabled = true;
        turtorialStep = 1;
        appearAppleDemo();
    }

    public void PrepareSecondTurtorial()
    {
        if (manipulatedObject != null)
            Destroy(manipulatedObject);

        turtorialEnabled = true;
        turtorialStep = 2;
        appearAppleDemo();
        // Create Gate and hide it
        Vector3 cameraPos = Camera.main.transform.position;
        Vector3 angles = Camera.main.transform.eulerAngles;
        Vector3 pos = new Vector3(cameraPos.x + Mathf.Sin((angles.y) * Mathf.Deg2Rad) * 1.5f,
                                   cameraPos.y,
                                   cameraPos.z + Mathf.Cos((angles.y) * Mathf.Deg2Rad) * 1.5f);
        Quaternion rot = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
        ObjectCollectionManager.Instance.CreateGate(pos, rot);
        ObjectCollectionManager.Instance.disappearGate();
        // Listen Events
        EventManager.StartListening("manipulation_started", manipulationStarted);
        EventManager.StartListening("manipulation_finished", manipulationFinished);
    }

    private void appearAppleDemo()
    {
        // Appear an apple in front of user
        manipulatedObject = Instantiate(AppleObject);
        //Destroy Rigidbody to disable gravity
        Destroy(manipulatedObject.GetComponent<Rigidbody>());
        if (applePosition != Vector3.zero)
            manipulatedObject.transform.position = applePosition;
        else
        {
            applePosition = Camera.main.transform.position + Camera.main.transform.forward * 2.0f;
            manipulatedObject.transform.position = applePosition;
        }
    }

    private void manipulationStarted()
    {
        if (!turtorialEnabled || turtorialStep != 2)
            return;

        //Appear Gate according to hand
        ObjectCollectionManager.Instance.appearGate(1.0f, 2.0f, manipulations % 2 == 1 ? true : false);
        //Get manipulatedObject
        manipulatedObject = handsTrackingController.getManipulatedObject();
        //Delete parent
        if (manipulatedObject != null)
            manipulatedObject.transform.parent = null;
        //Enable manipulation in flow controller
        manipulationInProgress = true;
        //Create balls to guide user
        createGuidance(manipulations % 2 == 1 ? true : false);
    }

    private void createGuidance(bool toRight)
    {
        Vector3 center = ObjectCollectionManager.Instance.getCreatedGate().GetComponent<Renderer>().bounds.center;
        Vector3 angles = ObjectCollectionManager.Instance.getCreatedGate().transform.eulerAngles;
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

    private void manipulationFinished()
    {
        // Destroy guidance
        foreach (GameObject point in guidanceObjects)
            Destroy(point);
        guidanceObjects.Clear();
    }

    private void tapUiReceived()
    {
        GameObject tappedObj = gazeCursor.getFocusedObject();
        if (tappedObj == null)
            return;

        if (tappedObj.CompareTag("UI"))
        {
            if (turtorialStep == 1)
            {
                if (tappedObj.name == "Next")
                    PrepareSecondTurtorial();
                else if (tappedObj.name == "Back")
                    ReturnToStartMenu();
            }
            else if (turtorialStep ==2)
            {
                if (tappedObj.name == "Next")
                    ProcceedToGame();
                else if (tappedObj.name == "Back")
                    PrepareFirstTurtorial();
            }
        }
    }

    private void ReturnToStartMenu()
    {
        //Clear Scene and reset variables
        FinishTurtorial();
        flowController.DisableTurtorialMode();
        uiController.ReturnToStartMenu();
    }

    private void ProcceedToGame()
    {
        //Clear Scene and reset variables
        FinishTurtorial();
        // Initiate Calibration
        string text = "Place your hand in right angle pose for 2 seconds ";
        uiController.printText(text);
        TextToSpeech.Instance.StartSpeaking(text);
        // Prepare Logic
        flowController.startPlaying();
    }


    private void FinishTurtorial()
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
        manipulations = 1;
        // Stop Listening for evens
        EventManager.StopListening("tap", tapUiReceived);
        EventManager.StopListening("manipulation_started", manipulationStarted);
        EventManager.StopListening("manipulation_finished", manipulationFinished);
    }
}
