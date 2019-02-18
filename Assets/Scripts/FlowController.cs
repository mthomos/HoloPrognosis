using UnityEngine;

public class FlowController : MonoBehaviour
{
    public HandsTrackingController handsTrackingController;
    public GazeCursor gazeCursor;
    public UiController uiController;
    public ObjectPlacer placer;
    private DataScript dataScript;

    // Training
    private bool trainingMode, rightHandPlaying, manipulationInProgress;
    private CalibrationController rightController, leftController, currentControlller;
    private GameObject manipulatedObject;
    public int success, fail, violation;

    //Create timer variables
    private float timer;
    public float timerForGate;
    public float timerForRightPose = 3.0f;
    public float timerForHighPose = 3.0f;

    // Gate variables
    private GateScript gateScript;
    private bool objectInGateDetected;
    private bool freeToRelease;

    private void Start ()
    {
        dataScript = GameObject.Find("Data").GetComponent<DataScript>();
    }

    private void manipulationStarted()
    {
        if (!trainingMode)
            return;
        //Appear Gate
        ObjectCollectionManager.Instance.appearGate(currentControlller);
        //Get manipulatedObject
        manipulatedObject = handsTrackingController.getManipulatedObject();
        manipulationInProgress = true;
    }

    private void successfulTry()
    {
        if (freeToRelease)
        {
            dataScript.addManipulationResult(true);
            success++;
            PrepareNextManipulation();
        }
        else
            failedTry();
    }

    private void failedTry()
    {
        dataScript.addManipulationResult(false);
        fail++;
        PrepareNextManipulation();
    }

    public void PrepareNextManipulation()
    {
        if (!trainingMode) return;
        //Disable Gate
        ObjectCollectionManager.Instance.disappearGate();
        violation = 0;
        manipulationInProgress = false;
        //Destroy object
        if (manipulatedObject != null)
        {
            ObjectCollectionManager.Instance.destoryActiveHologram(manipulatedObject.name);
            Destroy(manipulatedObject);
            manipulatedObject = null;
        }
        //Swap hands
        rightHandPlaying = !rightHandPlaying;
        if (rightHandPlaying)
            currentControlller = rightController;
        else
            currentControlller = leftController;

        //Load (possible) next object for manipulation
        GameObject nowPlayingObject = ObjectCollectionManager.Instance.getLowestFruit(currentControlller.getHighestPoseHandHeight());
        if (nowPlayingObject == null)
        {
            //Switch to the other hand if for the current hand object doesn't exist
            rightHandPlaying = !rightHandPlaying;
            if (rightHandPlaying)
                currentControlller = rightController;
            else
                currentControlller = leftController;
            nowPlayingObject = ObjectCollectionManager.Instance.getLowestFruit(currentControlller .getHighestPoseHandHeight());
        }
        // If no objects exist, finish
        if (nowPlayingObject == null)
            finishGame();
        else
        {
            uiController.prepareUserManipulation(rightHandPlaying);
            UtilitiesScript.Instance.EnableOutline(nowPlayingObject, null);
            nowPlayingObject.tag = "User";
        }
    }

    private void Update ()
    {
        if (trainingMode && manipulationInProgress)
        {
            // Calculate distance of manipulated object and gate
            if (gateScript == null)
                gateScript = ObjectCollectionManager.Instance.getCreatedGate().GetComponent<GateScript>();

            if (gateScript.objectInsideGate(manipulatedObject))
            {
                if (!objectInGateDetected)
                {
                    freeToRelease = false;
                    objectInGateDetected = true;
                    //Reset Timer
                    timer = 0.0f;
                    if (timerForGate == 0)
                        timerForGate = 3.0f;
                }
                else
                {   //Refresh Timer
                    timer += Time.deltaTime;
                    if (timer > timerForGate)
                    {
                        freeToRelease = true;
                    }
                }
            }
        }
	}

    public void startPlaying()
    {
        //Set generic use for gaze
        gazeCursor.setGenericUse();
        //Start hand calibration
        handsTrackingController.enableHandCalibration();
    }

    public void calibrationFinished()
    {
        uiController.printText("");
        TextToSpeech.Instance.StartSpeaking("Now the tree will be appeared");
        placer.CreateScene();
        //Set training use for gaze
        gazeCursor.setTrainingMode();
        // Enable manipulation with hands
        handsTrackingController.enableHandManipulation();
        // Enable data collection
        handsTrackingController.enableDataCollection();
        // Enable Timer enableTimer();
        trainingMode = true;
        //Enable Events
        EventManager.StartListening("manipulation_started", manipulationStarted);
        EventManager.StartListening("box_collision", successfulTry);
        EventManager.StartListening("floor_collision", failedTry);
    }

    public void finishGame()
    {
        TextToSpeech.Instance.StartSpeaking("Training finished");
        //Prepare UI
        uiController.moveToResultsScreen();
        //Reset
        trainingMode = false;
        gazeCursor.setGenericUse();
        trainingMode = false;
        success = 0;
        fail = 0;
        rightController = null;
        leftController = null;

        //Disable Events
        EventManager.StopListening("manipulation_started", manipulationStarted);
        EventManager.StopListening("box_collision", successfulTry);
        EventManager.StopListening("floor_collision", failedTry);
    }

    public bool addCalibrationController(CalibrationController controller)
    {
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

    public void userViolationDetected()
    {
        violation++;
    }
}
