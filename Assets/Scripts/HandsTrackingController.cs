using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

public struct HandStruct
{
    public GameObject hand;
    public bool rightHand;
    public bool enabled;

    public HandStruct(GameObject hand, bool rightHand)
    {
        this.hand = hand;
        this.rightHand = rightHand;
        enabled = true;
    }

    public HandStruct(bool status)
    {
        hand = null;
        rightHand = false;
        enabled = status;
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
    private bool TurtorialModeEnabled = false;
    //
    private HandStruct trackingHand = new HandStruct(false);
    private GameObject handObject;
    //Checking Manipulation Objects
    private GameObject TouchedObject; // GameObject which user touched and is candidate for Manipulation
    private GameObject ManipulatedObject;// GameObject which is being Manipulated by the user
    private GameObject FocusedObject; // GameObject which the user gazes at
    private CalibrationController calibrationController;
    private GestureRecognizer gestureRecognizer;
    //Training
    public float offset;
    public float bodyOffset;
    private float startTime;
    private bool RightPoseInProgress;
    private Vector3 initUserPos = Vector3.zero;
    // Trick to encounter Unity bug in input events
    //private Vector3 HandPosition = Vector3.zero;

    void Awake()
    {
        InteractionManager.InteractionSourcePressed += InteractionManager_InteractionSourcePressed;
        InteractionManager.InteractionSourceDetected += InteractionManager_InteractionSourceDetected;
        InteractionManager.InteractionSourceUpdated += InteractionManager_InteractionSourceUpdated;
        InteractionManager.InteractionSourceLost += InteractionManager_InteractionSourceLost;
        InteractionManager.InteractionSourceReleased += InteractionManager_InteractionSourceReleased;
        gestureRecognizer = new GestureRecognizer();
        gestureRecognizer.SetRecognizableGestures(GestureSettings.ManipulationTranslate | GestureSettings.Hold);
        gestureRecognizer.ManipulationStarted += GestureRecognizer_ManipulationStarted;
        gestureRecognizer.ManipulationUpdated += GestureRecognizer_ManipulationUpdated;
        gestureRecognizer.ManipulationCompleted += GestureRecognizer_ManipulationCompleted;
        gestureRecognizer.ManipulationCanceled += GestureRecognizer_ManipulationCanceled;
        gestureRecognizer.StartCapturingGestures();
        handObject = Instantiate(TrackingObject);
        handObject.SetActive(false);
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
        gestureRecognizer.StopCapturingGestures();
        gestureRecognizer.ManipulationStarted -= GestureRecognizer_ManipulationStarted;
        gestureRecognizer.ManipulationCompleted -= GestureRecognizer_ManipulationCompleted;
        gestureRecognizer.ManipulationCanceled -= GestureRecognizer_ManipulationCanceled;
        gestureRecognizer.ManipulationUpdated -= GestureRecognizer_ManipulationUpdated;
    }

    private void CheckForHands()
    {
        FocusedObject = cursor.GetFocusedObject();
        ManipulationForOneHand();
    }

    private void ManipulationForOneHand()
    {
        if (FocusedObject == null || trackingHand.enabled == false)
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
                    dataScript.InteractionTouched(trackingHand.rightHand);
            }
        }
        else
        {
            UtilitiesScript.Instance.ChangeColorOutline(FocusedObject, Color.white);
            UtilitiesScript.Instance.ChangeObjectColor(trackingHand.hand, DefaultColor);
            ObjectTouched = false;
            TouchedObject = null;
            ColorOutlineChanged = false;
        }
    }

    private void GestureRecognizer_ManipulationStarted(ManipulationStartedEventArgs args)
    {
        uint id = args.source.id;
        if (trackingHand.enabled == true && ObjectTouched && TouchedObject != null)
        {
            ObjectManipulationInProgress = true;
            ManipulatedObject = TouchedObject;
            //Viusal feedback	
            UtilitiesScript.Instance.ChangeColorOutline(ManipulatedObject, OutlineManipulateColor);
            ManipulatedObject.transform.position = trackingHand.hand.transform.position;
            //ManipulatedObject.transform.position = HandPosition;
            //Store initial position of hand and head	
            Vector3 initHandPos = ManipulatedObject.transform.position;
            initUserPos = Camera.main.transform.position;
            //Gather data	
            if (DataCollectionMode)
                dataScript.ManipulationStarted(trackingHand.rightHand);

            EventManager.TriggerEvent("manipulation_started");
        }
    }

    private void GestureRecognizer_ManipulationUpdated(ManipulationUpdatedEventArgs args)
    {
        if (trackingHand.enabled == true && ObjectManipulationInProgress)
        {
            //Move hand during manipulation
            if (ManipulatedObject != null)
                ManipulatedObject.transform.position = trackingHand.hand.transform.position;

            if (TurtorialModeEnabled)
                return;

            if (utilities.GetDistanceObjects(trackingHand.hand.transform, Camera.main.transform) < flowController.GetHeadDistanceUpperLimit(trackingHand.hand) + offset && // Check head-hand distance
                    utilities.GetDistanceObjects(trackingHand.hand.transform, Camera.main.transform) < flowController.GetHeadDisatnceLowerLimit(trackingHand.hand) - offset &&  // min max
                    Vector3.Magnitude(Camera.main.transform.position - initUserPos) < bodyOffset)
            {
                flowController.UserViolationDetected();
            }
            EventManager.TriggerEvent("manipulation_updated");
        }
    }

    private void GestureRecognizer_ManipulationCompleted(ManipulationCompletedEventArgs args)
    {
        if (args.source.kind != InteractionSourceKind.Hand)
            return;

        ManipulationEnded();
        EventManager.TriggerEvent("manipulation_completed");
    }

    private void GestureRecognizer_ManipulationCanceled(ManipulationCanceledEventArgs args)
    {
        if (args.source.kind != InteractionSourceKind.Hand)
            return;

        ManipulationEnded();
        EventManager.TriggerEvent("manipulation_canceled");
    }

    private void ManipulationEnded()
    {
        if (ObjectManipulationInProgress)
        {
            if (ManipulatedObject != null)
            {
                UtilitiesScript.Instance.DisableOutline(ManipulatedObject);
                UtilitiesScript.Instance.EnableGravity(ManipulatedObject, TurtorialModeEnabled ? false : true);
            }

            ObjectManipulationInProgress = false;
            ManipulatedObject = null;
            TouchedObject = null;

            if (DataCollectionMode)
                dataScript.ManipulationEnded(trackingHand.hand);

            EventManager.TriggerEvent("manipulation_finished");
        }
    }

    private void InteractionManager_InteractionSourceDetected(InteractionSourceDetectedEventArgs args)
    {
        if (args.state.source.kind == InteractionSourceKind.Hand)
        {
            //Get hand position and illustrate it
            handObject.SetActive(true);
            if (args.state.sourcePose.TryGetPosition(out Vector3 pos))
                handObject.transform.position = pos;

            //Define if it's the right hand
            bool IsRightHand = UtilitiesScript.Instance.IsRightFromHead(pos);
            trackingHand = new HandStruct(handObject, IsRightHand);
            // Control calibration
            if (HandCalibrationMode)
            {
                if (!flowController.IsHandCalibrated(IsRightHand))
                {
                    calibrationController = new CalibrationController(IsRightHand);
                    startTime = Time.time;
                    RightPoseInProgress = true;
                }
                else
                {
                    string handText = IsRightHand ? "Right hand " : "Left hand ";
                    uiController.PrintText(handText + "already calibrated");
                    TextToSpeech.Instance.StopSpeaking();
                    TextToSpeech.Instance.StartSpeaking(handText + "already calibrated");
                }
            }
            //Gather hand data
            if (DataCollectionMode) //Gather values for every hand movement
            {
                float height = Mathf.Abs(pos.y - Camera.main.transform.position.y);
                dataScript.AddValue(pos, height, trackingHand.rightHand);
            }
        }
    }

    private void InteractionManager_InteractionSourceUpdated(InteractionSourceUpdatedEventArgs args)
    {
        if (trackingHand.enabled == true && args.state.source.kind == InteractionSourceKind.Hand) // Detect Hand
        {
            //Update hand position
            if (args.state.sourcePose.TryGetPosition(out Vector3 pos))
            {
                if (trackingHand.hand != null)
                {   
                    pos.y = pos.y - trackingHand.hand.GetComponent<Renderer>().bounds.size.y * 0.5f;
                    trackingHand.hand.transform.position = pos;
                }
            }

            //Update calibration
            if (HandCalibrationMode && calibrationController != null)
            {
                float dist = (Camera.main.transform.position - pos).magnitude;
                if (RightPoseInProgress)
                {
                    calibrationController.AddValue(dist, pos.y);
                    if (Time.time - startTime > flowController.timerForRightPose) //Start max pose calibration
                    {
                        calibrationController.FinishRightPose();
                        uiController.PrintText("Raise your hand as high as you can. When ready open your palm");
                        TextToSpeech.Instance.StartSpeaking("Raise your hand as high as you can. When ready open your palm");
                        RightPoseInProgress = false;
                    }
                }
                else // High pose calibration
                    calibrationController.AddValue(dist, pos.y);
            }

            if (DataCollectionMode)
                dataScript.AddValue(pos, pos.y, trackingHand.rightHand);

            if (ManipulatedObject != null)
                ManipulatedObject.transform.position = trackingHand.hand.transform.position;
        }
    }

    private void InteractionManager_InteractionSourceReleased(InteractionSourceReleasedEventArgs args)
    {
        if (args.state.source.kind != InteractionSourceKind.Hand)
            return;

        InteractionEnded();
    }

    private void InteractionManager_InteractionSourceLost(InteractionSourceLostEventArgs args)
    {
        if (args.state.source.kind != InteractionSourceKind.Hand)
            return;

        InteractionEnded();
    }

    private void InteractionEnded()
    {
        Debug.Log("Interaction_Ended");
        if (HandCalibrationMode && calibrationController != null && !RightPoseInProgress)
        {
            if (flowController.AddCalibrationController(calibrationController) &&
                flowController.leftHandEnabled && flowController.rightHandEnabled)
            {
                HandCalibrationMode = false;
                flowController.CalibrationFinished();
            }
            else
            {
                TextToSpeech.Instance.StopSpeaking();
                if (calibrationController.IsRightHand() && flowController.leftHandEnabled)
                {
                    Debug.Log("Right hand calibrated, waith for left");
                    uiController.PrintText("Right Hand calibrated successfully. Now let's calibrate the left one");
                    TextToSpeech.Instance.StartSpeaking("Right Hand calibrated successfully. Now let's calibrate the left one");
                }
                else if (!calibrationController.IsRightHand() && flowController.rightHandEnabled)
                {
                    Debug.Log("Left hand calibrated, waith for right");
                    uiController.PrintText("Left Hand calibrated successfully. Now let's calibrate the right one");
                    TextToSpeech.Instance.StartSpeaking("Left Hand calibrated successfully. Now let's calibrate the right one");
                }
                else
                {
                    Debug.Log("Calibration finished for the enabled hand(s)");
                    HandCalibrationMode = false;
                    flowController.CalibrationFinished();
                }
                calibrationController = null;
            }

            handObject.SetActive(false);
            trackingHand = new HandStruct(false);
            
        }
    }

    private void InteractionManager_InteractionSourcePressed(InteractionSourcePressedEventArgs args)
    {
        if (args.state.source.kind == InteractionSourceKind.Hand)
        {
            if (trackingHand.enabled == true && !ObjectTouched)
            {
                handObject.SetActive(false);
                trackingHand = new HandStruct(false);
            }
            EventManager.TriggerEvent("tap");
        }
    }

    public void EnableHandCalibration()
    {
        HandCalibrationMode = true;
        ManipulationEnabled = false;
    }

    public void EnableHandManipulation()
    {
        HandCalibrationMode = false;
        ManipulationEnabled = true;
    }

    public void DisableHandManipulation()
    {
        HandCalibrationMode = false;
        ManipulationEnabled = false;
    }

    public void EnableDataCollection()
    {
        DataCollectionMode = true;
    }

    public void DisableDataCollection()
    {
        DataCollectionMode = false;
    }

    public GameObject GetManipulatedObject()
    {
        return ManipulatedObject;
    }

    public void SetTurtorialMode(bool status)
    {
        TurtorialModeEnabled = status;
    }
}