using System.Collections.Generic;
using UnityEngine;

public class TurtorialController : MonoBehaviour
{
    // Fill in Editor
    public FlowController flowController;
    public AudioSource audioSource;
    public GazeCursor gazeCursor;
    public HandsTrackingController handsTrackingController;
    public UiController uiController;
    public GameObject TurtorialMenu;
    public GameObject PointPrefab;
    public GameObject AppleObject;

    public int turtorialStep;
    private GameObject manipulatedObject;
    private List<GameObject> guidanceObjects = new List<GameObject>();
    private Vector3 applePosition = Vector3.zero;
    private GameObject createdGate;
    private GateScript createdGateScript = null;
    private bool feedbackTriggered;
    //
    private SpriteRenderer imageViewRenderer = null;
    int inPic = 1;
    public Sprite pic1, pic2, pic3;

    void Start()
    {
        turtorialStep = -1;
        // Listen Events
        //EventManager.StartListening("double_click", ClickerDoubleClickTriggered);
        EventManager.StartListening("click", ClickerClickTriggered);
    }

    void Update()
    {
        if (turtorialStep == 2)
        {
            if (createdGate == null)
                return;

            if (createdGateScript.objectInsideGate(manipulatedObject))
            {
                if (uiController.greekEnabled)
                {
                    if (!audioSource.isPlaying)
                        audioSource.Play();
                }
                else
                {
                    if (!TextToSpeech.Instance.IsSpeaking())
                        TextToSpeech.Instance.StartSpeaking("Apple inside the Circle");
                }
            }
        }
    }

    private void ClickerClickTriggered()
    {
        if (imageViewRenderer == null)
        {
            imageViewRenderer = Object.FindObjectOfType<SpriteRenderer>();
        }
        if (imageViewRenderer == null)
            return;

        if (inPic == 1)
        {
            imageViewRenderer.sprite = pic2;
            inPic = 2;
        }
        else if (inPic == 2)
        {
            imageViewRenderer.sprite = pic3;
            inPic = 3;
        }
        else if (inPic == 3)
        {
            imageViewRenderer.sprite = pic1;
            inPic = 1;
        }
    }

    public void PrepareFirstTurtorial()
    {
        turtorialStep = 1;
        AppearAppleDemo();

        // Stop Listening for events
        EventManager.StopListening("manipulation_started", ManipulationStarted);
        EventManager.StopListening("manipulation_finished", ManipulationFinished);
    }

    public void PrepareSecondTurtorial()
    {
        turtorialStep = 2;
        AppearAppleDemo();
        // Create Gate and hide it
        Vector3 cameraPos = Camera.main.transform.position;
        Vector3 angles = Camera.main.transform.eulerAngles;
        Vector3 pos = new Vector3(cameraPos.x + Mathf.Sin((angles.y) * Mathf.Deg2Rad) * 1.3f,
                                   cameraPos.y,
                                   cameraPos.z + Mathf.Cos((angles.y) * Mathf.Deg2Rad) * 1.3f);
        Quaternion rot = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
        createdGate = ObjectCollectionManager.Instance.CreateGate(pos, rot);
        createdGateScript = ObjectCollectionManager.Instance.GetCreatedGateScript();
        UtilitiesScript.Instance.DisableObject(createdGate);
        // Listen Events
        EventManager.StartListening("manipulation_started", ManipulationStarted);
        EventManager.StartListening("manipulation_finished", ManipulationFinished);
        // For greek audio feedback
        if (!feedbackTriggered)
        {
            feedbackTriggered = true;
            if (uiController.greekEnabled)
            {
                audioSource.Stop();
                audioSource.clip = uiController.GateClip;
                audioSource.Play();
            }
            else
            {
                TextToSpeech.Instance.StopSpeaking();
                TextToSpeech.Instance.StartSpeaking("Now every time you move an object , a circle will be appeared. " +
                    "If you use your right hand, the circle will appear to the right, otherwise to the left." +
                    "Guidance points will also appear to guide you the circle. Your goal is to move the object with a turn of the body and your hand stretched out");
            }
        }
    }

    private void AppearAppleDemo()
    {
        if (manipulatedObject != null)
            return;
        // Appear an apple in front of user
        manipulatedObject = Instantiate(AppleObject);
        //Destroy Rigidbody to disable gravity
        if (manipulatedObject.GetComponent<Rigidbody>() != null)
            Destroy(manipulatedObject.GetComponent<Rigidbody>());

        if (applePosition == Vector3.zero)
            applePosition = Camera.main.transform.position + Camera.main.transform.forward * 1.0f;

        manipulatedObject.transform.position = applePosition;
        UtilitiesScript.Instance.EnableOutline(manipulatedObject, Color.magenta, true);
    }

    private void ManipulationStarted()
    {
        if (turtorialStep != 2)
            return;

        //Get manipulatedObject
        manipulatedObject = handsTrackingController.GetManipulatedObject();
        //Appear Gate according to hand
        Vector3 dx = Camera.main.transform.position - manipulatedObject.transform.position;
        float distance = new Vector2(dx.x, dx.z).magnitude;
        float d_height = createdGate.GetComponent<Renderer>().bounds.size.y * 0.45f;
        ObjectCollectionManager.Instance.AppearGate(manipulatedObject.transform.position.y - d_height, distance, handsTrackingController.IsHandRight() ? true : false);
        //Create balls to guide user
        CreateGuidance(handsTrackingController.IsHandRight() ? true : false, distance);
    }

    private void CreateGuidance(bool toRight, float distance)
    {
        Vector3 center = manipulatedObject.transform.position;
        Vector3 userPosition = Camera.main.transform.position;
        Vector3 angles = Camera.main.transform.eulerAngles;
        Vector3 position = new Vector3(0, center.y, 0);
        
        if (toRight)
        {
            for (float angle = 0; angle < 84f; angle += 8)
            {
                position.x = userPosition.x + Mathf.Sin((angles.y + angle) * Mathf.Deg2Rad) * distance;
                position.z = userPosition.z + Mathf.Cos((angles.y + angle) * Mathf.Deg2Rad) * distance;
                guidanceObjects.Add(Instantiate(PointPrefab, position, Quaternion.LookRotation(Vector3.zero)));
            }
        }
        else
        {
            for (float angle = -84; angle < 1.0f; angle += 8)
            {
                position.x = userPosition.x + Mathf.Sin((angles.y + angle) * Mathf.Deg2Rad) * distance;
                position.z = userPosition.z + Mathf.Cos((angles.y + angle) * Mathf.Deg2Rad) * distance;
                guidanceObjects.Add(Instantiate(PointPrefab, position, Quaternion.LookRotation(Vector3.zero)));
            }
        }
    }

    private void ManipulationFinished()
    {
         if (turtorialStep != 2)
            return;

        // Destroy guidance
        foreach (GameObject point in guidanceObjects)
            Destroy(point);
        guidanceObjects.Clear();
        ObjectCollectionManager.Instance.DisappearGate();
    }

    public void FinishTurtorial()
    {
        // Destroy guidance
        foreach (GameObject point in guidanceObjects)
            Destroy(point);
        guidanceObjects.Clear();
        ObjectCollectionManager.Instance.DisappearGate();

        if (manipulatedObject != null)
        {
            Destroy(manipulatedObject);
            manipulatedObject = null;
        }

        turtorialStep = -1;
        // Stop Listening for events
        EventManager.StopListening("manipulation_started", ManipulationStarted);
        EventManager.StopListening("manipulation_finished", ManipulationFinished);
    }
}
