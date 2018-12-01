﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

public struct HandStruct
{
    public GameObject hand;
    public uint id;
    public bool rightHand;

    public HandStruct(GameObject hand, uint id, bool rightHand)
    {
        this.hand = hand;
        this.id = id;
        this.rightHand = rightHand;
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
    private bool ManipulationWithOneHand = false;
    // trackingHands: In this dictionary, hands which generate intaractions and scanned by Hololens are stored
    private Dictionary<uint, HandStruct> trackingHands = new Dictionary<uint, HandStruct>();
    //Checking Manipulation Objects
    private GameObject TouchedObject; // GameObject which user touched and is candidate for Manipulation
    private GameObject ManipulatedObject;// GameObject which is being Manipulated by the user
    private GameObject FocusedObject; // GameObject which the user gazes at
    private GestureRecognizer gestureRecognizer;
    private DataScript dataScript;
    private CalibrationController calibrationController;
    //Training
    private int manipulationCounter;
    private Vector3 initHandPos;
    private Vector3 initUserPos;
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
        //gestureRecognizer.HoldStarted += GestureRecognizer_HoldStarted;
        //gestureRecognizer.HoldCanceled += GestureRecognizer_HoldCanceled;
        //gestureRecognizer.HoldCompleted += GestureRecognizer_HoldCompleted;
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
        //gestureRecognizer.HoldStarted -= GestureRecognizer_HoldStarted;
        //gestureRecognizer.HoldCanceled -= GestureRecognizer_HoldCanceled;
        //gestureRecognizer.HoldCompleted -= GestureRecognizer_HoldCompleted;
    }

    private void CheckForHands()
    {
        FocusedObject = cursor.getFocusedObject();
        if (FocusedObject != null && trackingHands.Count > 0)
        {
            ManipulationForOneHand();
        }
    }

    private void ManipulationForOneHand()
    {
        Vector3 focusPos = FocusedObject.transform.position; //The player gazes at the item which will catch
        Vector3 handPos = trackingHands.ElementAt(0).Value.hand.transform.position; //The position of user's hand in the Holospace
        if (FocusedObject.GetComponent<Renderer>() == null)
            return;

        Bounds focusedObjectBounds = FocusedObject.GetComponent<Renderer>().bounds; //The graphical bounds of the focused objects
        focusedObjectBounds.Expand(.10f);
        if (focusedObjectBounds.Contains(handPos))
        {
            if (!ObjectManipulationInProgress)
            {
                ChangeObjectColor(trackingHands[trackingHands.ElementAt(0).Key].hand, TouchedColor);
                ObjectTouched = true;
                ManipulationWithOneHand = true;
                TouchedObject = FocusedObject;
                //Refresh Outline
                if (!ColorOutlineChanged)
                {
                    EnableOutline(TouchedObject);
                    ColorOutlineChanged = true;
                }
                //Gather data
                if (DataCollectionMode)
                    dataScript.interactionTouched();
            }
        }
        else
        {
            ChangeObjectColor(trackingHands[trackingHands.ElementAt(0).Key].hand, DefaultColor);
            ObjectTouched = false;
            TouchedObject = null;
            //DisableOutline(FocusedObject);
            ColorOutlineChanged = false;
        }
    }

    private void EnableOutline(GameObject focusedObject)
    {
        if (focusedObject != null)
        {
            var outline = focusedObject.GetComponent<Outline>();
            if(outline == null)
            {
                //Create Outline
                outline = gameObject.AddComponent<Outline>();
                if (outline != null)
                {
                    outline.OutlineMode = Outline.Mode.OutlineAll;
                    outline.OutlineWidth = 5f;
                    outline.OutlineColor = OutlineDefaultColor;
                    outline.enabled = true;
                }
            }
            else
            {
                outline.OutlineColor = OutlineDefaultColor;
                outline.enabled = true;
            }
        }
    }

    private void ChangeColorOutline(GameObject focusedObject, Color color)
    {
        if (focusedObject != null)
        {
            var outline = focusedObject.GetComponent<Outline>();
            if (outline != null)
            {
                if (outline.enabled == false) outline.enabled = true;
                outline.OutlineColor = color;
            }
        }
    }

    private void DisableOutline(GameObject focusedObject)
    {
        if (focusedObject != null)
        {
            var outline = focusedObject.GetComponent<Outline>();
            if (outline != null) outline.enabled = false;
        }
    }

    private void ChangeObjectColor(GameObject obj, Color color)
    {
        obj.GetComponent<Renderer>().material.color = color;
    }

    private void ChangeObjectColor(Dictionary<uint, HandStruct> dictionary, Color color)
    {
        foreach (KeyValuePair<uint, HandStruct> entry in dictionary)
        {
            ChangeObjectColor(entry.Value.hand, DefaultColor);
        }
    }

    private void GestureRecognizer_ManipulationStarted(ManipulationStartedEventArgs args)
    {
        uint id = args.source.id;
        if (trackingHands.ContainsKey(id) && ObjectTouched)
        {
            ObjectManipulationInProgress = true;
            //Reset Gravity
            ManipulatedObject = TouchedObject;
            //Viusal feedback
            ChangeColorOutline(ManipulatedObject, OutlineManipulateColor);
            //Store initial position of hand and head
            initHandPos = ManipulatedObject.transform.position;
            initUserPos = Camera.main.transform.position;
            /*
            //Disable Wind
            if(ManipulatedObject.GetComponent<AppleScript>() != null)
                ManipulatedObject.GetComponent<AppleScript>().disableWind();
            */
            ManipulatedObject.transform.position = trackingHands[id].hand.transform.position;
            if (TrainingMode)
            {
                ObjectCollectionManager.Instance.appearBox(manipulationCounter, initHandPos);
            }
            //Gather data
            if (DataCollectionMode)
                dataScript.manipulationStarted();
        }
        EventManager.TriggerEvent("manipulation_started");
    }

    private void GestureRecognizer_ManipulationUpdated(ManipulationUpdatedEventArgs args)
    {
        if (trackingHands.ContainsKey(args.source.id) && ObjectManipulationInProgress)
        {
            /*
            if ((getDistanceObjects(trackingHands[args.source.id].hand.transform , Camera.main.transform) < flowController.getHeadDistanceUpperLimit(trackingHands[args.source.id].hand) + offset) && // Check head-hand distance
                (getDistanceObjects(trackingHands[args.source.id].hand.transform, Camera.main.transform) < flowController.getHeadDisatnceLowerLimit(trackingHands[args.source.id].hand) - 3*offset)) //&&
            
            (Camera.main.transform.position.x - initUserPos.x) > bodyOffset &&  // Check head/body position
            (Camera.main.transform.position.x - initUserPos.x) < bodyOffset &&
            (Camera.main.transform.position.z - initUserPos.z) > bodyOffset &&
            (Camera.main.transform.position.z - initUserPos.z) < bodyOffset))
            */

            //Move hand
            if(ManipulatedObject != null)
                ManipulatedObject.transform.position = trackingHands[args.source.id].hand.transform.position;
            if (TrainingMode)
                flowController.checkIfAboveBox(ManipulatedObject.transform.position);
        }
            
        EventManager.TriggerEvent("manipulation_updated");
    }

    private void GestureRecognizer_ManipulationCompleted(ManipulationCompletedEventArgs args)
    {
        if (ObjectManipulationInProgress)
        //if (trackingHands.ContainsKey(args.source.id) && ObjectManipulationInProgress)
        {
            manipulationCounter++;
            DisableOutline(ManipulatedObject);
            EnableGravity(ManipulatedObject);

            ObjectManipulationInProgress = false;
            ManipulatedObject = null;
            TouchedObject = null;
            ManipulationWithOneHand = true;

            //Data
            if (DataCollectionMode)
                dataScript.manipulationEnded();
        }
        EventManager.TriggerEvent("manipulation_completed");
    }

    private void GestureRecognizer_ManipulationCanceled(ManipulationCanceledEventArgs args)
    {
        if (ObjectManipulationInProgress)
        //if (trackingHands.ContainsKey(args.source.id) && ObjectManipulationInProgress)
        {
            manipulationCounter++;
            DisableOutline(ManipulatedObject);
            EnableGravity(ManipulatedObject);
            //ChangeObjectColor(trackingHands[args.source.id].hand, DefaultColor);

            ObjectManipulationInProgress = false;
            ManipulatedObject = null;
            TouchedObject = null;
            ManipulationWithOneHand = true;

            //Data
            if (DataCollectionMode)
                dataScript.manipulationEnded();
        }
        EventManager.TriggerEvent("manipulation_canceled");
    }

    private void GestureRecognizer_Tapped(TappedEventArgs args)
    {            
        uint id = args.source.id;
        EventManager.TriggerEvent("tap");
    }

    private void GestureRecognizer_HoldStarted(HoldStartedEventArgs args)
    {
        if (TouchedObject != null)
            ChangeColorOutline(TouchedObject, OutlineHoldColor);
    }

    private void GestureRecognizer_HoldCanceled(HoldCanceledEventArgs args)
    {
        if (TouchedObject != null)
            ChangeColorOutline(TouchedObject, HoldFinished);
    }

    private void GestureRecognizer_HoldCompleted(HoldCompletedEventArgs args)
    {
        if (TouchedObject != null)
            ChangeColorOutline(TouchedObject, HoldFinished);
    }

    private void InteractionManager_InteractionSourceDetected(InteractionSourceDetectedEventArgs obj)
    {
        if (obj.state.source.kind == InteractionSourceKind.Hand)
        {
            var hand = Instantiate(TrackingObject) as GameObject;
            Vector3 pos;
            if (obj.state.sourcePose.TryGetPosition(out pos))
                hand.transform.position = pos;
            // Define if it is right/left hand
            Vector3 heading = pos - Camera.main.transform.position;
            Vector3 perp = Vector3.Cross(Camera.main.transform.forward, heading);
            float dot = Vector3.Dot(perp, Camera.main.transform.up);
            bool rightHand;
            if (dot <= 0) //left hand
                rightHand =false;
            else
                rightHand = true;

            trackingHands.Add(obj.state.source.id, new HandStruct(hand, obj.state.source.id, rightHand));
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
        if (trackingHands.ContainsKey(id) && args.state.source.kind == InteractionSourceKind.Hand)
        {
            Vector3 pos;
            if (args.state.sourcePose.TryGetPosition(out pos))
                trackingHands[id].hand.transform.position = pos;
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
        if (trackingHands.ContainsKey(id) && args.state.source.kind == InteractionSourceKind.Hand)
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
            Destroy(trackingHands[id].hand);
            trackingHands.Remove(id);
        }
    }

    private void InteractionManager_InteractionSourceReleased(InteractionSourceReleasedEventArgs args)
    {
        uint id = args.state.source.id;
        if (trackingHands.ContainsKey(id) && args.state.source.kind == InteractionSourceKind.Hand)
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
            Destroy(trackingHands[id].hand);
            trackingHands.Remove(id);
        }
    }

    private void EnableGravity(GameObject obj)
    {
        if (obj.GetComponent<Rigidbody>() == null)
        {
            obj.AddComponent<Rigidbody>();
            obj.GetComponent<Rigidbody>().useGravity = true;
        }
        else
        {
            obj.GetComponent<Rigidbody>().useGravity = true;
        }
    }

    private float getDistanceObjects(Transform obj1, Transform obj2)
    {
        return Vector3.Magnitude(obj1.position - obj2.position); 
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
        //DataCollectionMode = true;
    }

    public void disableDataCollection()
    {
        DataCollectionMode = false;
    }

    public void enableOutlineToManipulated(Color color)
    {
        ChangeColorOutline(ManipulatedObject, color);
    }
}