using UnityEngine;

public class FlowController : MonoBehaviour
{
    public HandsTrackingController handsTrackingController;
    public GazeCursor gazeCursor;
    public UiController uiController;
    public ObjectPlacer placer;
    public DataScript dataScript;
    public TurtorialController turtorialController;
    // Training
    private bool trainingMode, turtorialMode, rightHandPlaying;
    private CalibrationController rightController, leftController, currentControlller;
    public int success, fail;
    public bool rightHandEnabled, leftHandEnabled;
    public float  maxHeightRightHand, maxHeightLeftHand;
    //Create timer variables
    private float timer;
    public float timerForGate = 1.0f;
    public float timerForRightPose = 3.0f;
    // Gate variables
    private GateScript gateScript;
    // Manipulation variables -- reset in every manipulation
    private bool objectInGateDetected, manipulationInProgress, freeToRelease;
    public int violation;
    private GameObject manipulatedObject = null;
    // Store manipulations
    private int manipulations;

    private void Start ()
    {
        
    }

    private void Update()
    {
        if (trainingMode && manipulationInProgress && !turtorialMode)
        {
            // Calculate distance of manipulated object and gate
            if (gateScript == null && ObjectCollectionManager.Instance.getCreatedGate() != null)
                gateScript = ObjectCollectionManager.Instance.getCreatedGate().GetComponent<GateScript>();

            if (gateScript == null)
                return;

            if (manipulatedObject == null)
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
        EventManager.StopListening("manipulation_started", manipulationStarted);
        EventManager.StopListening("box_collision", successfulTry);
        EventManager.StopListening("floor_collision", failedTry);
        EventManager.StopListening("world_created", PrepareNextManipulation);
    }

    // Event functions
    private void manipulationStarted()
    {
        if (!trainingMode || turtorialMode)
            return;

        //Appear Gate according to hand
        Debug.Log("Gate appeared");
        ObjectCollectionManager.Instance.appearGate(currentControlller.getRightPoseHandHeight(), 
                2.0f, currentControlller.isRightHand());
        //Get manipulatedObject
        manipulatedObject = handsTrackingController.getManipulatedObject();
        //Delete parent
        if (manipulatedObject != null)
            manipulatedObject.transform.parent = null;
        ObjectCollectionManager.Instance.disappearTree();
        //Enable manipulation in flow controller
        manipulationInProgress = true;
    }

    private void successfulTry()
    {
        if (turtorialMode)
            return;

        if (freeToRelease && violation < 50)
        {
            dataScript.addManipulationResult(true, currentControlller.isRightHand());
            success++;
            PrepareNextManipulation();
        }
        else
            failedTry();
    }

    private void failedTry()
    {
        if (turtorialMode)
            return;

        dataScript.addManipulationResult(false, currentControlller.isRightHand());
        fail++;
        PrepareNextManipulation();
    }

    public void PrepareNextManipulation()
    {
        // If training mode is disable exit
        if (!trainingMode || turtorialMode)
            return;

        ObjectCollectionManager.Instance.appearGate();
        ObjectCollectionManager.Instance.appearTree();
        manipulations++;
        string debugString = "Manipulation_" + manipulations + "->";
        //Disable Gate
        ObjectCollectionManager.Instance.disappearGate();
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
            ObjectCollectionManager.Instance.destoryActiveHologram(manipulatedObject.name);
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
            nowPlayingObject = ObjectCollectionManager.Instance.getLowestFruit(currentControlller.getHighestPoseHandHeight());
            if (nowPlayingObject == null)
            {
                Debug.Log(debugString + "for the current hand, no nowPlayingObject, switch hand");
                //Switch to the other hand if for the current hand object doesn't exist
                rightHandPlaying = !rightHandPlaying;
                if (rightHandPlaying)
                    currentControlller = rightController;
                else
                    currentControlller = leftController;
                nowPlayingObject = ObjectCollectionManager.Instance.getLowestFruit(currentControlller.getHighestPoseHandHeight());
            }
        }
        else if (rightHandEnabled && !leftHandEnabled) // Only right hand enabled
        {
            Debug.Log(debugString + "for the right hand");
            nowPlayingObject = ObjectCollectionManager.Instance.getLowestFruit(rightController.getHighestPoseHandHeight());
        }
        else if (!rightHandEnabled && leftHandEnabled) // Only left hand enabled
        {
            Debug.Log(debugString + "for the left hand");
            nowPlayingObject = ObjectCollectionManager.Instance.getLowestFruit(leftController.getHighestPoseHandHeight());
        }
        // If no objects exist, finish
        if (nowPlayingObject == null)
            finishGame();
        else
        {
            uiController.prepareUserManipulation(rightHandPlaying);
            UtilitiesScript.Instance.EnableOutline(nowPlayingObject, null, false);
            nowPlayingObject.tag = "User";
            nowPlayingObject.GetComponent<SphereCollider>().enabled = true;
            Debug.Log(debugString + "object_name->"+nowPlayingObject.name);
        }
    }

    public void startPlaying()
    {
        //Reset variables
        success = 0;
        fail= 0;
        timer = 0;
        maxHeightRightHand = 0;
        maxHeightLeftHand = 0;
        //Prepare UI
        uiController.moveToPlayspace();
        //Start hand calibration
        handsTrackingController.enableHandCalibration();
        rightController = null;
        leftController = null;
    }

    public void calibrationFinished()
    {
        uiController.printText("");
        TextToSpeech.Instance.StartSpeaking("Now the tree will be appeared");
        placer.CreateScene();
        // Enable manipulation with hands
        handsTrackingController.enableHandManipulation();
        // Enable data collection
        handsTrackingController.enableDataCollection();
        trainingMode = true;
        //Enable Events
        EventManager.StartListening("manipulation_started", manipulationStarted);
        EventManager.StartListening("box_collision", successfulTry);
        EventManager.StartListening("floor_collision", failedTry);
        EventManager.StartListening("world_created", PrepareNextManipulation);
    }

    public void finishGame()
    {
        Debug.Log("training finished");
        TextToSpeech.Instance.StartSpeaking("Training finished");
        //Save data
        //dataScript.finishSession();
        //Prepare UI
        uiController.moveToResultsScreen();
        //Reset
        trainingMode = false;
    }

    public bool addCalibrationController(CalibrationController controller)
    {
        //Store controllers
        if (controller.isRightHand())
            rightController = controller;
        else
            leftController = controller;

        //Are controllers full ?
        if (rightController != null && leftController != null)
            return true;
        else
            return false;
    }

    public float getHeadDistanceUpperLimit(bool hand)
    {
        CalibrationController currentController;
        if (hand)
            currentController = rightController;
        else
            currentController = leftController;

        if (currentController.getHighestPoseHeadHandDistance() > currentController.getRightPoseHeadHandDistance())
            return currentController.getHighestPoseHeadHandDistance();
        else
            return currentController.getRightPoseHeadHandDistance();
    }

    public float getHeadDisatnceLowerLimit(bool hand)
    {
        CalibrationController currentController;
        if (hand)
            currentController = rightController;
        else
            currentController = leftController;

        if (currentController.getHighestPoseHeadHandDistance() > currentController.getRightPoseHeadHandDistance())
            return currentController.getRightPoseHeadHandDistance();
        else
            return currentController.getHighestPoseHeadHandDistance();
    }

    public void UserViolationDetected()
    {
        if (!turtorialController.turtorialEnabled)
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
}
