using UnityEngine;
using UnityEngine.XR.WSA.Input;

public struct HandStruct
{
    public GameObject hand;
    public uint id;
    public bool rightHand;
    public bool interactionDetected;

    public HandStruct(GameObject hand, uint id, bool rightHand)
    {
        this.hand = hand;
        this.id = id;
        this.rightHand = rightHand;
        interactionDetected = true;
    }

    public HandStruct(bool status)
    {
        hand = null;
        id = 1;
        rightHand = status;
        interactionDetected = status;
    }
}

public class HandsTrackingController : MonoBehaviour
{
    //Public Variables - For Editor
    //GameObjects
    public DataScript dataScript;
    public GazeCursor cursor; // Cusrsor used for defining FocusedObject
    public GameObject TrackingObject; // GameObject representing the hand in holographic space
    public FlowController flowController;
    public UiController uiController;
    public UtilitiesScript utilities;
    //Colors
    public Color DefaultColor = Color.green;
    public Color TapColor = Color.white;
    public Color TouchedColor = Color.magenta;
    public Color OutlineHoldColor = Color.blue;
    public Color OutlineDefaultColor = Color.red;
    public Color OutlineManipulateColor = Color.green;
    public Color HoldFinished = Color.white;
    //Private Variables
    //Booleans
    private bool ManipulationEnabled = false;
    private bool ColorOutlineChanged = false;
    private bool DataCollectionMode = false;
    private bool HandCalibrationMode = false;
    private bool ObjectTouched = false;
    private bool ObjectManipulationInProgress = false;
    // trackingHands: In this dictionary, hands which generate intaractions and scanned by Hololens are stored
    private HandStruct trackingHand = new HandStruct(false);
    //Checking Manipulation Objects
    private GameObject TouchedObject; // GameObject which user touched and is candidate for Manipulation
    private GameObject ManipulatedObject;// GameObject which is being Manipulated by the user
    private GameObject FocusedObject; // GameObject which the user gazes at
    private GestureRecognizer gestureRecognizer;
    private CalibrationController calibrationController;
    //Training
    public float offset;
    public float bodyOffset;
    private float startTime;
    private bool RightPoseInProgress;
    Vector3 initUserPos = Vector3.zero;

    void Awake()
    {
        InteractionManager.InteractionSourcePressed += InteractionManager_InteractionSourcePressed;
        InteractionManager.InteractionSourceDetected += InteractionManager_InteractionSourceDetected;
        InteractionManager.InteractionSourceUpdated += InteractionManager_InteractionSourceUpdated;
        InteractionManager.InteractionSourceLost += InteractionManager_InteractionSourceLost;
        InteractionManager.InteractionSourceReleased += InteractionManager_InteractionSourceReleased;
        /*
        gestureRecognizer = new GestureRecognizer();
        gestureRecognizer.SetRecognizableGestures(GestureSettings.Tap);
        gestureRecognizer.Tapped += GestureRecognizer_Tapped;
        gestureRecognizer.ManipulationUpdated += GestureRecognizer_ManipulationUpdated;
        gestureRecognizer.StartCapturingGestures();
        */
    }

    void Update()
    {
        if(ManipulationEnabled)
            CheckForHands();
    }

    void OnDestroy()
    {
        InteractionManager.InteractionSourcePressed -= InteractionManager_InteractionSourcePressed;
        InteractionManager.InteractionSourceDetected -= InteractionManager_InteractionSourceDetected;
        InteractionManager.InteractionSourceUpdated -= InteractionManager_InteractionSourceUpdated;
        InteractionManager.InteractionSourceLost -= InteractionManager_InteractionSourceLost;
        InteractionManager.InteractionSourceReleased -= InteractionManager_InteractionSourceReleased;
        /*
        gestureRecognizer.StopCapturingGestures();
        gestureRecognizer.Tapped -= GestureRecognizer_Tapped;
        */
    }

    private void CheckForHands()
    {
        FocusedObject = cursor.getFocusedObject();
        if (FocusedObject != null && trackingHand.interactionDetected == true)
            ManipulationForOneHand();
    }

    private void ManipulationForOneHand()
    {
        if (FocusedObject != null || trackingHand.hand != null)
            return;

        Vector3 focusPos = FocusedObject.transform.position; //The player gazes at the item which will catch
        Vector3 handPos = trackingHand.hand.transform.position; //The position of user's hand in the Holospace
        if (FocusedObject.GetComponent<Renderer>() == null)
            return;

        Bounds focusedObjectBounds = FocusedObject.GetComponent<Renderer>().bounds;
        focusedObjectBounds.Expand(.10f);
        if (focusedObjectBounds.Contains(handPos))
        {
            if (!ObjectManipulationInProgress) // Execute only before manipulation
            {
                UtilitiesScript.Instance.ChangeObjectColor(trackingHand.hand, TouchedColor);
                ObjectTouched = true;
                TouchedObject = FocusedObject;
                //Refresh Outline
                if (!ColorOutlineChanged)
                {
                    UtilitiesScript.Instance.EnableOutline(TouchedObject, Color.magenta, true);
                    ColorOutlineChanged = true;
                }
                //Gather data
                if (DataCollectionMode)
                    dataScript.interactionTouched(trackingHand.rightHand);
            }
        }
        else
        {
            UtilitiesScript.Instance.ChangeObjectColor(trackingHand.hand, DefaultColor);
            ObjectTouched = false;
            TouchedObject = null;
            ColorOutlineChanged = false;
        }
    }

    private void InteractionManager_InteractionSourceDetected(InteractionSourceDetectedEventArgs args)
    {
        if (args.state.source.kind == InteractionSourceKind.Hand)
        {
            //Get hand position and illustrate it
            var hand = Instantiate(TrackingObject) as GameObject;
            if (args.state.sourcePose.TryGetPosition(out Vector3 pos))
                hand.transform.position = pos;

            //Define if it's the right hand
            bool isRightHand = UtilitiesScript.Instance.isRightFromHead(pos);
            trackingHand = new HandStruct(hand, args.state.source.id, isRightHand);
            // Control calibration
            if (HandCalibrationMode)
            {
                if (!flowController.IsHandCalibrated(isRightHand))
                {
                    calibrationController = new CalibrationController(isRightHand);
                    startTime = Time.time;
                    RightPoseInProgress = true;
                }
                else
                {
                    string handText = isRightHand ? "Right hand " : "Left hand ";
                    uiController.printText(handText + "already calibrated");
                    TextToSpeech.Instance.StartSpeaking(handText + "already calibrated");
                }
            }
            //Gather hand data
            if (DataCollectionMode) //Gather values for every hand movement
            {
                float height = Mathf.Abs(pos.y - Camera.main.transform.position.y);
                dataScript.addValue(pos, height, trackingHand.rightHand);
            }
            // Control object manipulation
            if (trackingHand.interactionDetected = true && ObjectTouched)
            {
                if (TouchedObject == null)
                    return;

                ObjectManipulationInProgress = true;
                ManipulatedObject = TouchedObject;
                //Viusal feedback
                UtilitiesScript.Instance.ChangeColorOutline(ManipulatedObject, OutlineManipulateColor);
                ManipulatedObject.transform.position = trackingHand.hand.transform.position;
                //Store initial position of hand and head
                initUserPos = Camera.main.transform.position;
                /*
                //Disable Wind of object
                if(ManipulatedObject.GetComponent<AppleScript>() != null)
                    ManipulatedObject.GetComponent<AppleScript>().disableWind();
                */
                //Gather data
                if (DataCollectionMode)
                    dataScript.manipulationStarted(trackingHand.hand);
            }
        }
        EventManager.TriggerEvent("manipulation_started");
    }

    private void InteractionManager_InteractionSourceUpdated(InteractionSourceUpdatedEventArgs args)
    {
        //Debug.Log("-----" + args.state.source.kind + "----- GestureRecognizer_ManipulationUpdated");
        if (args.state.source.kind == InteractionSourceKind.Hand) // Detect Hand
        {
            //Update hand position
            if (args.state.sourcePose.TryGetPosition(out Vector3 pos) && trackingHand.hand != null)
                trackingHand.hand.transform.position = pos;

            //Update calibration
            if (HandCalibrationMode && calibrationController != null)
            {
                float dist = (Camera.main.transform.position - pos).magnitude;
                if (RightPoseInProgress)
                {
                    calibrationController.addValue(dist, pos.y);
                    if (Time.time - startTime > flowController.timerForRightPose) //Start max pose calibration
                    {
                        calibrationController.finishRightPose();
                        uiController.calibrationMaxPose();
                        RightPoseInProgress = false;
                    }
                }
                else // High pose calibration
                    calibrationController.addValue(dist, pos.y);
            }
            if (DataCollectionMode)
                dataScript.addValue(pos, pos.y, trackingHand.rightHand);

            if (ObjectManipulationInProgress)
            {
                if (utilities.getDistanceObjects(trackingHand.hand.transform, Camera.main.transform) < flowController.getHeadDistanceUpperLimit(trackingHand.hand) + offset && // Check head-hand distance
                    utilities.getDistanceObjects(trackingHand.hand.transform, Camera.main.transform) < flowController.getHeadDisatnceLowerLimit(trackingHand.hand) - offset &&  // min max
                    Vector3.Magnitude(Camera.main.transform.position - initUserPos) < bodyOffset)
                {
                    flowController.UserViolationDetected();
                }
                //Move hand during manipulation
                if (ManipulatedObject != null)
                    ManipulatedObject.transform.position = trackingHand.hand.transform.position;
            }
        }
        EventManager.TriggerEvent("manipulation_updated");
    }

    private void InteractionManager_InteractionSourceReleased(InteractionSourceReleasedEventArgs args)
    {
        if (args.state.source.kind == InteractionSourceKind.Hand)
            manipulationEnded();

        EventManager.TriggerEvent("manipulation_completed");
    }

    private void InteractionManager_InteractionSourceLost(InteractionSourceLostEventArgs args)
    {
        if (args.state.source.kind == InteractionSourceKind.Hand)
            manipulationEnded();

        EventManager.TriggerEvent("manipulation_canceled");
    }

    private void manipulationEnded()
    {
        if (HandCalibrationMode && calibrationController != null && !RightPoseInProgress)
        {
            if (flowController.addCalibrationController(calibrationController) &&
                flowController.leftHandEnabled && flowController.rightHandEnabled)
            {
                HandCalibrationMode = false;
                flowController.calibrationFinished();
            }
            else
            {
                if (calibrationController.isRightHand() && flowController.leftHandEnabled)
                {
                    Debug.Log("Right hand calibrated, waith for left");
                    uiController.printText("Right Hand calibrated successfully. Now let's calibrate the left one");
                    TextToSpeech.Instance.StartSpeaking("Right Hand calibrated successfully. Now let's calibrate the left one");
                }
                else if (!calibrationController.isRightHand() && flowController.rightHandEnabled)
                {
                    Debug.Log("Left hand calibrated, waith for right");
                    uiController.printText("Left Hand calibrated successfully. Now let's calibrate the right one");
                    TextToSpeech.Instance.StartSpeaking("Left Hand calibrated successfully. Now let's calibrate the right one");
                }
                else
                {
                    HandCalibrationMode = false;
                    flowController.calibrationFinished();
                }
                calibrationController = null;
            }

            if (ObjectManipulationInProgress)
            {
                if (ManipulatedObject != null)
                {
                    UtilitiesScript.Instance.DisableOutline(ManipulatedObject);
                    UtilitiesScript.Instance.EnableGravity(ManipulatedObject, true);
                }

                ObjectManipulationInProgress = false;
                ManipulatedObject = null;
                //TouchedObject = null;

                if (DataCollectionMode)
                    dataScript.manipulationEnded(trackingHand.hand);
            }

            if (trackingHand.hand != null)
            {
                Destroy(trackingHand.hand);
                trackingHand.hand = null;
                trackingHand.interactionDetected = false;
            }
        }
    }

    private void InteractionManager_InteractionSourcePressed(InteractionSourcePressedEventArgs args)
    {
        //Debug.Log("-----" + args.state.source.kind + "----- GestureRecognizer_Tapped");
        if (args.state.source.kind == InteractionSourceKind.Hand)
        {
            if (trackingHand.hand != null)
            {
                Destroy(trackingHand.hand);
                trackingHand.hand = null;
                trackingHand.interactionDetected = false;
            }
            EventManager.TriggerEvent("tap");
        }
    }

    public void enableHandCalibration()
    {
        HandCalibrationMode = true;
        ManipulationEnabled = false;
    }

    public void enableHandManipulation()
    {
        HandCalibrationMode = false;
        ManipulationEnabled = true;
    }

    public void enableDataCollection()
    {
        DataCollectionMode = true;
    }

    public void disableDataCollection()
    {
        DataCollectionMode = false;
    }

    public GameObject getManipulatedObject()
    {
        return ManipulatedObject;
    }
}