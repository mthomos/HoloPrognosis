using UnityEngine;

public class FlowController : MonoBehaviour
{
    public HandsTrackingController handsTrackingController;
    public GazeCursor gazeCursor;
    public UiController uiController;
    public ObjectPlacer placer;
    public DataScript dataScript;
    public TurtorialController turtorialController;

    private bool trainingMode, turtorialMode, rightHandPlaying;
    private CalibrationController rightController, leftController, currentControlller;
    public bool rightHandEnabled, leftHandEnabled;
    public float  maxHeightRightHand, maxHeightLeftHand; // Max hand height of user hands during manipulations
    // Timer variables
    private float timer;
    public float timerForGate = 1.0f;
    public float timerForRightPose = 3.0f;
    // Gate variables
    private GateScript gateScript;
    // Manipulation variables -- reset in every manipulation
    private bool objectInGateDetected, manipulationInProgress, freeToRelease;
    private GameObject manipulatedObject = null;
    private int manipulations, violation;
    public int success, fail;

    private void Start ()
    {
        
    }

    private void Update()
    {
        if (trainingMode && manipulationInProgress && !turtorialMode)
        {
            // Calculate distance of manipulated object and gate
            if (gateScript == null && ObjectCollectionManager.Instance.GetCreatedGate() != null)
                gateScript = ObjectCollectionManager.Instance.GetCreatedGate().GetComponent<GateScript>();

            if (gateScript == null || manipulatedObject == null)
                return;

            if (gateScript.objectInsideGate(manipulatedObject))
            {
                if (!objectInGateDetected)
                {
                    freeToRelease = false;
                    objectInGateDetected = true;
                    //Reset Timer
                    timer = 0.0f;
                }
                else
                {   //Refresh Timer
                    timer += Time.deltaTime;
                    if (timer > timerForGate)
                    {
                        freeToRelease = true;
                        if (gateScript != null)
                            gateScript.enableCollider();
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        //Disable Events
        EventManager.StopListening("manipulation_started", ManipulationStarted);
        EventManager.StopListening("box_collision", SuccessfulTry);
        EventManager.StopListening("floor_collision", FailedTry);
        EventManager.StopListening("world_created", PrepareNextManipulation);
    }

    // Event functions
    private void ManipulationStarted()
    {
        if (!trainingMode || turtorialMode)
            return;

        //Appear Gate according to hand
        Debug.Log("Gate appeared");
        ObjectCollectionManager.Instance.AppearGate(currentControlller.GetRightPoseHandHeight(), 
                2.0f, currentControlller.IsRightHand());
        //Get manipulatedObject
        manipulatedObject = handsTrackingController.GetManipulatedObject();
        //Delete parent
        if (manipulatedObject != null)
            manipulatedObject.transform.parent = null;
        ObjectCollectionManager.Instance.DisAppearTree();
        //Enable manipulation in flow controller
        manipulationInProgress = true;
    }

    private void SuccessfulTry()
    {
        if (turtorialMode)
            return;

        if (freeToRelease && violation < 50)
        {
            dataScript.AddManipulationResult(true, currentControlller.IsRightHand());
            success++;
            PrepareNextManipulation();
        }
        else
            FailedTry();
    }

    private void FailedTry()
    {
        if (turtorialMode)
            return;

        dataScript.AddManipulationResult(false, currentControlller.IsRightHand());
        fail++;
        PrepareNextManipulation();
    }

    public void PrepareNextManipulation()
    {
        // If training mode is disable exit
        if (!trainingMode || turtorialMode)
            return;

        ObjectCollectionManager.Instance.AppearGate();
        ObjectCollectionManager.Instance.AppearTree();
        manipulations++;
        string debugString = "Manipulation_" + manipulations + "->";
        //Disable Gate
        ObjectCollectionManager.Instance.DisappearGate();
        //Reset variables
        violation = 0;
        manipulationInProgress = false;
        objectInGateDetected = false;
        //Destroy object
        if (manipulatedObject != null)
        {
            if (rightHandPlaying)
                maxHeightRightHand = manipulatedObject.transform.position.y;
            else
                maxHeightLeftHand = manipulatedObject.transform.position.y;

            Debug.Log(debugString + "destroy_hologram");
            ObjectCollectionManager.Instance.DestoryActiveHologram(manipulatedObject.name);
            Destroy(manipulatedObject);
            manipulatedObject = null;
        }
        GameObject nowPlayingObject = null;
        //Swap hands 
        if (rightHandEnabled && leftHandEnabled) // if both hands enabled
        {
            rightHandPlaying = !rightHandPlaying;
            Debug.Log(debugString+" right hand:" + rightHandPlaying);
            if (rightHandPlaying)
                currentControlller = rightController;
            else
                currentControlller = leftController;

            //Load (possible) next object for manipulation
            nowPlayingObject = ObjectCollectionManager.Instance.GetLowestFruit(currentControlller.GetHighestPoseHandHeight());
            if (nowPlayingObject == null)
            {
                Debug.Log(debugString + "for the current hand, no nowPlayingObject, switch hand");
                //Switch to the other hand if for the current hand object doesn't exist
                rightHandPlaying = !rightHandPlaying;
                if (rightHandPlaying)
                    currentControlller = rightController;
                else
                    currentControlller = leftController;
                nowPlayingObject = ObjectCollectionManager.Instance.GetLowestFruit(currentControlller.GetHighestPoseHandHeight());
            }
        }
        else if (rightHandEnabled && !leftHandEnabled) // Only right hand enabled
        {
            Debug.Log(debugString + "for the right hand");
            nowPlayingObject = ObjectCollectionManager.Instance.GetLowestFruit(rightController.GetHighestPoseHandHeight());
        }
        else if (!rightHandEnabled && leftHandEnabled) // Only left hand enabled
        {
            Debug.Log(debugString + "for the left hand");
            nowPlayingObject = ObjectCollectionManager.Instance.GetLowestFruit(leftController.GetHighestPoseHandHeight());
        }
        // If no objects exist, finish
        if (nowPlayingObject == null)
            FinishGame();
        else
        {
            // Notify user for next manipulation
            string text = "Next manipulation is with the " + (rightHandPlaying ? "right": "left") + "hand";
            TextToSpeech.Instance.StartSpeaking(text);
            UtilitiesScript.Instance.EnableOutline(nowPlayingObject, null, false);
            // Enable new object
            nowPlayingObject.tag = "User";
            nowPlayingObject.GetComponent<SphereCollider>().enabled = true;
            Debug.Log(debugString + "object_name->"+nowPlayingObject.name);
        }
    }

    public void StartPlaying()
    {
        // Reset variables
        success = 0;
        fail= 0;
        timer = 0;
        maxHeightRightHand = 0;
        maxHeightLeftHand = 0;
        manipulations = 0;
        violation = 0;
        rightHandPlaying = false;
        // Start hand calibration
        handsTrackingController.EnableHandCalibration();
        rightController = null;
        leftController = null;
    }

    public void CalibrationFinished()
    {
        uiController.PrintText("");
        TextToSpeech.Instance.StartSpeaking("Now the tree will be appeared");
        placer.CreateScene();
        // Enable manipulation with hands
        handsTrackingController.EnableHandManipulation();
        // Enable data collection
        handsTrackingController.EnableDataCollection();
        trainingMode = true;
        //Enable Events
        EventManager.StartListening("manipulation_started", ManipulationStarted);
        EventManager.StartListening("box_collision", SuccessfulTry);
        EventManager.StartListening("floor_collision", FailedTry);
        EventManager.StartListening("world_created", PrepareNextManipulation);
    }

    public void FinishGame()
    {
        Debug.Log("training finished");
        TextToSpeech.Instance.StartSpeaking("Training finished");
        //Save data
        dataScript.FinishSession();
        //Prepare UI
        uiController.MoveToResultsScreen();
        //Reset
        trainingMode = false;
    }

    public bool AddCalibrationController(CalibrationController controller)
    {
        //Store controllers
        if (controller.IsRightHand())
            rightController = controller;
        else
            leftController = controller;

        //Are controllers full ?
        if (rightController != null && leftController != null)
            return true;
        else
            return false;
    }

    public float GetHeadDistanceUpperLimit(bool hand)
    {
        CalibrationController currentController;
        if (hand)
            currentController = rightController;
        else
            currentController = leftController;

        if (currentController.GetHighestPoseHeadHandDistance() > currentController.GetRightPoseHeadHandDistance())
            return currentController.GetHighestPoseHeadHandDistance();
        else
            return currentController.GetRightPoseHeadHandDistance();
    }

    public float GetHeadDisatnceLowerLimit(bool hand)
    {
        CalibrationController currentController;
        if (hand)
            currentController = rightController;
        else
            currentController = leftController;

        if (currentController.GetHighestPoseHeadHandDistance() > currentController.GetRightPoseHeadHandDistance())
            return currentController.GetRightPoseHeadHandDistance();
        else
            return currentController.GetHighestPoseHeadHandDistance();
    }

    public void UserViolationDetected()
    {
        violation++;
    }

    public CalibrationController GetRightCalibrationController()
    {
        return rightController;
    }

    public CalibrationController GetLeftCalibrationController()
    {
        return leftController;
    }

    public void EnableTurtorialMode()
    {
        turtorialMode = true;
    }

    public void DisableTurtorialMode()
    {
        turtorialMode = false;
    }

    public bool IsHandCalibrated(bool rightHand)
    {
        if (rightHand)
        {
            return rightController == null ? false : true;
        }
        else
            return leftController == null ? false : true;
    }

    public void SetManipulationInProgress(bool set)
    {
        manipulationInProgress = set;
    }
}
