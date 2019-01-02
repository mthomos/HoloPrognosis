using System;
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
    public GazeCursor cursor; // Cusrsor used for defining FocusedObject
    public TextMesh DebugText; // Text for debugging (for now)
    public GameObject TrackingObject; // GameObject representing the hand in holographic space
    public FlowController flowController;
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
    private DataScript dataScript;
    private CalibrationController calibrationController;
    //Training
    private int manipulationCounter;
    public float offset;
    public float bodyOffset;
    private float timerForRightPose;
    private float startTime;
    private bool RightPoseInProgress;
    private bool HighPoseInProgress;

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
        //Enable Data Collection
        dataScript = GameObject.Find("Data").GetComponent<DataScript>();
    }

    void Update()
    {
        if(TrainingMode)
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
        {
            ManipulationForOneHand();
        }
    }

    private void ManipulationForOneHand()
    {
        Vector3 focusPos = FocusedObject.transform.position; //The player gazes at the item which will catch
        Vector3 handPos = trackingHand.hand.transform.position; //The position of user's hand in the Holospace
        if (FocusedObject.GetComponent<Renderer>() == null)
            return;

        Bounds focusedObjectBounds = FocusedObject.GetComponent<Renderer>().bounds; //The graphical bounds of the focused objects
        focusedObjectBounds.Expand(.10f);
        if (focusedObjectBounds.Contains(handPos))
        {
            if (!ObjectManipulationInProgress)
            {
                UtilitiesScript.Instance.ChangeObjectColor(trackingHand.hand, TouchedColor);
                ObjectTouched = true;
                TouchedObject = FocusedObject;
                //Refresh Outline
                if (!ColorOutlineChanged)
                {
                    UtilitiesScript.Instance.EnableOutline(TouchedObject, null);
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
            /*
            if ((getDistanceObjects(trackingHand.hand.transform , Camera.main.transform) < flowController.getHeadDistanceUpperLimit(trackingHand.hand) + offset) && // Check head-hand distance
                (getDistanceObjects(trackingHand.hand.transform, Camera.main.transform) < flowController.getHeadDisatnceLowerLimit(trackingHand.hand) - 3*offset)) //&&
            
            (Camera.main.transform.position.x - initUserPos.x) > bodyOffset &&  // Check head/body position
            (Camera.main.transform.position.x - initUserPos.x) < bodyOffset &&
            (Camera.main.transform.position.z - initUserPos.z) > bodyOffset &&
            (Camera.main.transform.position.z - initUserPos.z) < bodyOffset))
            */

            //Move hand
            if(ManipulatedObject != null)
                ManipulatedObject.transform.position = trackingHand.hand.transform.position;
            if (TrainingMode)
                flowController.checkIfAboveBox(ManipulatedObject.transform.position);
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
        //if (trackingHand != null && ObjectManipulationInProgress) WIP until 2018.4
        {
            manipulationCounter++;
            UtilitiesScript.Instance.DisableOutline(ManipulatedObject);
            UtilitiesScript.Instance.EnableGravity(ManipulatedObject);

            ObjectManipulationInProgress = false;
            ManipulatedObject = null;
            TouchedObject = null;

            //Data
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

            bool rightHand = UtilitiesScript.Instance.isRightObject(pos);
            trackingHand = new HandStruct(hand, obj.state.source.id, rightHand);   
            if (HandCalibrationMode)
            {
                calibrationController = new CalibrationController(rightHand);
                beginCalibrationRightPose();
            }
            if (DataCollectionMode)
            {
                float height = Mathf.Abs(pos.y - Camera.main.transform.position.y);
                dataScript.addValue(pos, height);
            }
        }
    }

    private void beginCalibrationRightPose()
    {
        timerForRightPose = flowController.timerForRightPose;
        startTime = Time.time;
        RightPoseInProgress = true;
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
                float height = pos.y;
                if(RightPoseInProgress)
                {
                    calibrationController.addValue(dist, height);
                    if (Time.time - startTime > timerForRightPose) //Start max pose calibration
                    {
                        calibrationController.finishRightPose();
                        flowController.calibrationMaxPose();
                        RightPoseInProgress = false;
                        HighPoseInProgress = true;
                    }
                }
        
                if (HighPoseInProgress)
                {
                    calibrationController.addValue(dist, height);
                }
            }
            if (DataCollectionMode)
            {
                float height = pos.y;
                dataScript.addValue(pos, height);
            }
        }
    }   

    private void InteractionManager_InteractionSourceLost(InteractionSourceLostEventArgs args)
    {
        uint id = args.state.source.id;
        if (trackingHand.interactionDetected == true && args.state.source.kind == InteractionSourceKind.Hand)
        {
            if (HandCalibrationMode && calibrationController != null)
            {
                bool finishCalibration = flowController.addCalibrationController(calibrationController);
                if (finishCalibration)
                {
                    HandCalibrationMode = false;
                    HighPoseInProgress = false;
                    calibrationController = null;
                    flowController.calibrationFinished();
                }
                else
                {
                    if (calibrationController.isRightHand())
                        DebugText.text = "Right Hand calibrated successfully. Now let's calibrate the left one";
                    else
                        DebugText.text = "Left Hand calibrated successfully. Now let's calibrate the right one";
                    calibrationController = null;
                    HighPoseInProgress = false;
                }
            }
            Destroy(trackingHand.hand);
            trackingHand.interactionDetected = false;
        }
    }

    private void InteractionManager_InteractionSourceReleased(InteractionSourceReleasedEventArgs args)
    {
        uint id = args.state.source.id;
        if (trackingHand.interactionDetected == true && args.state.source.kind == InteractionSourceKind.Hand)
        {
            if (HandCalibrationMode && calibrationController != null)
            {
                bool continueCalibration = flowController.addCalibrationController(calibrationController);
                if (continueCalibration)
                {
                    HandCalibrationMode = false;
                    HighPoseInProgress = false;
                    calibrationController = null;
                    flowController.calibrationFinished();
                }
                else
                {
                    if (calibrationController.isRightHand())
                        DebugText.text = "Right Hand calibrated successfully. Now let's calibrate the left one";
                    else
                        DebugText.text = "Left Hand calibrated successfully. Now let's calibrate the right one";
                    calibrationController = null;
                    HighPoseInProgress = false;
                }
            }
            Destroy(trackingHand.hand);
            trackingHand.interactionDetected = false;
        }
    }

    public void enableHandCalibration()
    {
        HandCalibrationMode = true;
        TrainingMode = false;
    }

    public void enableHandManipulation()
    {
        HandCalibrationMode = false;
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