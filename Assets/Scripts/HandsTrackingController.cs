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
    private bool TrainingMode = false;
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
    private int manipulationCounter;
    public float offset;
    public float bodyOffset;
    private float timerForRightPose;
    private float startTime;
    private bool RightPoseInProgress;
    Vector3 initUserPos;

    void Awake()
    {
        InteractionManager.InteractionSourceDetected += InteractionManager_InteractionSourceDetected;
        InteractionManager.InteractionSourceUpdated += InteractionManager_InteractionSourceUpdated;
        InteractionManager.InteractionSourceLost += InteractionManager_InteractionSourceLost;
        InteractionManager.InteractionSourceReleased += InteractionManager_InteractionSourceReleased;
        gestureRecognizer = new GestureRecognizer();
        gestureRecognizer.SetRecognizableGestures(GestureSettings.ManipulationTranslate | GestureSettings.Tap); //  GestureSettings.Hold |
        gestureRecognizer.Tapped += GestureRecognizer_Tapped;
        gestureRecognizer.ManipulationStarted += GestureRecognizer_ManipulationStarted;
        gestureRecognizer.ManipulationCompleted += GestureRecognizer_ManipulationCompleted;
        gestureRecognizer.ManipulationCanceled += GestureRecognizer_ManipulationCanceled;
        gestureRecognizer.ManipulationUpdated += GestureRecognizer_ManipulationUpdated;
        gestureRecognizer.HoldStarted += GestureRecognizer_HoldStarted;
        gestureRecognizer.StartCapturingGestures();
    }

    void Update()
    {
        if(ManipulationEnabled)
            CheckForHands();
    }

    void OnDestroy()
    {
        InteractionManager.InteractionSourceDetected -= InteractionManager_InteractionSourceDetected;
        InteractionManager.InteractionSourceUpdated -= InteractionManager_InteractionSourceUpdated;
        InteractionManager.InteractionSourceLost -= InteractionManager_InteractionSourceLost;
        gestureRecognizer.StopCapturingGestures();
        gestureRecognizer.Tapped -= GestureRecognizer_Tapped;
        gestureRecognizer.ManipulationStarted -= GestureRecognizer_ManipulationStarted;
        gestureRecognizer.ManipulationCompleted -= GestureRecognizer_ManipulationCompleted;
        gestureRecognizer.ManipulationCanceled -= GestureRecognizer_ManipulationCanceled;
        gestureRecognizer.ManipulationUpdated -= GestureRecognizer_ManipulationUpdated;
        gestureRecognizer.HoldStarted -= GestureRecognizer_HoldStarted;
    }

    private void CheckForHands()
    {
        FocusedObject = cursor.getFocusedObject();
        if (FocusedObject != null && trackingHand.interactionDetected == true)
            ManipulationForOneHand();
    }

    private void ManipulationForOneHand()
    {
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
                    dataScript.interactionTouched();
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

    private void GestureRecognizer_ManipulationStarted(ManipulationStartedEventArgs args)
    {
        uint id = args.source.id;
        if (trackingHand.interactionDetected = true && ObjectTouched)
        {
            ObjectManipulationInProgress = true;
            ManipulatedObject = TouchedObject;
            //Viusal feedback
            UtilitiesScript.Instance.ChangeColorOutline(ManipulatedObject, OutlineManipulateColor);
            ManipulatedObject.transform.position = trackingHand.hand.transform.position;
            //Store initial position of hand and head
            Vector3 initHandPos = ManipulatedObject.transform.position;
            initUserPos = Camera.main.transform.position;
            //Disable Wind of object
            if(ManipulatedObject.GetComponent<AppleScript>() != null)
                ManipulatedObject.GetComponent<AppleScript>().disableWind();
            //Gather data
            if (DataCollectionMode)
                dataScript.manipulationStarted();
        }
        EventManager.TriggerEvent("manipulation_started");
    }

    private void GestureRecognizer_ManipulationUpdated(ManipulationUpdatedEventArgs args)
    {
        if (trackingHand.interactionDetected == true && ObjectManipulationInProgress)
        {    
            if( utilities.getDistanceObjects(trackingHand.hand.transform , Camera.main.transform) < flowController.getHeadDistanceUpperLimit(trackingHand.hand) + offset && // Check head-hand distance
                utilities.getDistanceObjects(trackingHand.hand.transform, Camera.main.transform) < flowController.getHeadDisatnceLowerLimit(trackingHand.hand) -offset &&  // min max
                Vector3.Magnitude(Camera.main.transform.position - initUserPos) <bodyOffset && TrainingMode)
            {
                flowController.UserViolationDetected();
            }
            //Move hand
            if(ManipulatedObject != null)
                ManipulatedObject.transform.position = trackingHand.hand.transform.position;
        }
        EventManager.TriggerEvent("manipulation_updated");
    }

    private void GestureRecognizer_ManipulationCompleted(ManipulationCompletedEventArgs args)
    {
        manipulationEnded();
        EventManager.TriggerEvent("manipulation_completed");
    }

    private void GestureRecognizer_ManipulationCanceled(ManipulationCanceledEventArgs args)
    {
        manipulationEnded();
        EventManager.TriggerEvent("manipulation_canceled");
    }

    private void manipulationEnded()
    {
        if (ObjectManipulationInProgress)
        {
            manipulationCounter++;
            UtilitiesScript.Instance.DisableOutline(ManipulatedObject);
            UtilitiesScript.Instance.EnableGravity(ManipulatedObject);

            ObjectManipulationInProgress = false;
            ManipulatedObject = null;
            TouchedObject = null;

            if (DataCollectionMode)
                dataScript.manipulationEnded();
        }
    }
    private void GestureRecognizer_Tapped(TappedEventArgs args)
    {            
        uint id = args.source.id;
        EventManager.TriggerEvent("tap");
    }

    private void GestureRecognizer_HoldStarted(HoldStartedEventArgs args)
    {
        if (TouchedObject != null)
            UtilitiesScript.Instance.ChangeColorOutline(TouchedObject, OutlineHoldColor);
    }

    private void InteractionManager_InteractionSourceDetected(InteractionSourceDetectedEventArgs obj)
    {
        if (obj.state.source.kind == InteractionSourceKind.Hand)
        {
            var hand = Instantiate(TrackingObject) as GameObject;
            Vector3 pos;
            if (obj.state.sourcePose.TryGetPosition(out pos))
                hand.transform.position = pos;

            bool rightHand = UtilitiesScript.Instance.isRightFromHead(pos);
            trackingHand = new HandStruct(hand, obj.state.source.id, rightHand);   
            if (HandCalibrationMode)
            {
                calibrationController = new CalibrationController(rightHand);
                //Calibration Right Pose
                timerForRightPose = flowController.timerForRightPose;
                startTime = Time.time;
                RightPoseInProgress = true;
            }
            if (DataCollectionMode) //Gather values for every hand movement
            {
                float height = Mathf.Abs(pos.y - Camera.main.transform.position.y);
                dataScript.addValue(pos, height);
            }
        }
    }

    private void InteractionManager_InteractionSourceUpdated(InteractionSourceUpdatedEventArgs args)
    {
        uint id = args.state.source.id;
        if (trackingHand.interactionDetected == true && args.state.source.kind == InteractionSourceKind.Hand)
        {
            Vector3 pos;
            if (args.state.sourcePose.TryGetPosition(out pos))
                trackingHand.hand.transform.position = pos;
            if (HandCalibrationMode && calibrationController != null)
            {
                float dist =(Camera.main.transform.position - pos).magnitude; 
                if(RightPoseInProgress)
                {
                    calibrationController.addValue(dist, pos.y);
                    if (Time.time - startTime > timerForRightPose) //Start max pose calibration
                    {
                        calibrationController.finishRightPose();
                        uiController.calibrationMaxPose();
                        RightPoseInProgress = false;
                    }
                }
                else // high pose
                    calibrationController.addValue(dist, pos.y);
            }
            if (DataCollectionMode)
                dataScript.addValue(pos, pos.y);
        }
    }   

    private void InteractionManager_InteractionSourceLost(InteractionSourceLostEventArgs args)
    {
        if (trackingHand.interactionDetected == true && args.state.source.kind == InteractionSourceKind.Hand)
        {
            interactionTerminated();
        }
    }

    private void InteractionManager_InteractionSourceReleased(InteractionSourceReleasedEventArgs args)
    {
        if (trackingHand.interactionDetected == true && args.state.source.kind == InteractionSourceKind.Hand)
        {
            interactionTerminated();
        }
    }

    private void interactionTerminated()
    {
        if (HandCalibrationMode && calibrationController != null)
        {
            bool finishCalibration = flowController.addCalibrationController(calibrationController);
            if (finishCalibration)
            {
                Debug.Log("Both hands calibrated. Last hand was right:" + calibrationController.isRightHand());
                HandCalibrationMode = false;
                calibrationController = null;
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
                else if (calibrationController.isRightHand() && flowController.rightHandEnabled)
                {
                    Debug.Log("Left hand calibrated, waith for right");
                    uiController.printText("Left Hand calibrated successfully. Now let's calibrate the right one");
                    TextToSpeech.Instance.StartSpeaking("Left Hand calibrated successfully. Now let's calibrate the right one");
                }
                else
                {
                    HandCalibrationMode = false;
                    calibrationController = null;
                    flowController.calibrationFinished();
                }
                calibrationController = null;
            }
        }
        Destroy(trackingHand.hand);
        trackingHand.interactionDetected = false;
    }

    public void enableHandCalibration()
    {
        HandCalibrationMode = true;
        ManipulationEnabled = false;
        TrainingMode = false;
    }

    public void enableHandManipulation()
    {
        HandCalibrationMode = false;
        ManipulationEnabled = true;
        TrainingMode = true;
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