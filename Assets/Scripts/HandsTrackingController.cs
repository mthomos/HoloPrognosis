using System.Collections.Generic;
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
    public TextMesh StatusText; // Text for debugging (for now)
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
    private bool HandManipulationMode = false;
    private bool ObjectTouched = false;
    private bool ObjectManipulationInProgress = false;
    private bool ManipulationWithTwoHands = false;
    // trackingHands: In this dictionary, hands which generate intaractions and scanned by Hololens are stored
    private Dictionary<uint, HandStruct> trackingHands = new Dictionary<uint, HandStruct>();
    //Checking Manipulation Objects
    private GameObject TouchedObject; // GameObject which user touched and is candidate for Manipulation
    private GameObject ManipulatedObject;// GameObject which is being Manipulated by the user
    private GameObject FocusedObject; // GameObject which the user gazes at
    private GestureRecognizer gestureRecognizer;
    private InteractionAnalysis interactionAnalysis;
    private CalibrationController calibrationController;
    //Experimental
    //
    private int clicker;
    private float lastClickTime = .0f;
    private bool clickTriggered;
    private bool doubleClickTriggered;
    //
    private Vector3 privPos = Vector3.zero;
    private float privTime = .0f;
    //
    private int manipulationCounter;
    private Vector3 initPos;
    void Awake()
    {
        InteractionManager.InteractionSourceDetected += InteractionManager_InteractionSourceDetected;
        InteractionManager.InteractionSourceUpdated += InteractionManager_InteractionSourceUpdated;
        InteractionManager.InteractionSourceLost += InteractionManager_InteractionSourceLost;
        InteractionManager.InteractionSourceReleased += InteractionManager_InteractionSourceReleased;
        InteractionManager.InteractionSourcePressed += InteractionManager_InteractionSourcePressed;
        gestureRecognizer = new GestureRecognizer();
        gestureRecognizer.SetRecognizableGestures(GestureSettings.Tap | GestureSettings.ManipulationTranslate | GestureSettings.Hold);
        gestureRecognizer.Tapped += GestureRecognizer_Tapped;
        gestureRecognizer.ManipulationStarted += GestureRecognizer_ManipulationStarted;
        gestureRecognizer.ManipulationCompleted += GestureRecognizer_ManipulationCompleted;
        gestureRecognizer.ManipulationCanceled += GestureRecognizer_ManipulationCanceled;
        gestureRecognizer.ManipulationUpdated += GestureRecognizer_ManipulationUpdated;
        gestureRecognizer.HoldStarted += GestureRecognizer_HoldStarted;
        gestureRecognizer.HoldCanceled += GestureRecognizer_HoldCanceled;
        gestureRecognizer.HoldCompleted += GestureRecognizer_HoldCompleted;
        gestureRecognizer.StartCapturingGestures();
        interactionAnalysis = GameObject.Find("Data").GetComponent<InteractionAnalysis>();
    }

    void Update()
    {
        if(HandManipulationMode)
            CheckForHands();
        // Check for clcker
        CheckForClicker();
    }

    private void CheckForHands()
    {
        FocusedObject = cursor.getFocusedObject();
        if (FocusedObject != null)
        {
            int HandsNeeded = ObjectCollectionManager.Instance.GetHandsNeededForManipulation(FocusedObject.GetInstanceID());
            if (HandsNeeded <= trackingHands.Count && HandsNeeded > 0)
            {
                switch (HandsNeeded)
                {
                    case 1:
                        ManipulationForOneHand();
                        break;
                    case 2:
                        ManipulationForTwoHands();
                        break;
                }
            }
        }
    }

    private void CheckForClicker()
    {
        if (doubleClickTriggered == true)
        {
            clickTriggered = false;
            doubleClickTriggered = false;
            EventManager.TriggerEvent("double_click");
        }
        if (doubleClickTriggered == false && clickTriggered == true)
        {
            if (Time.time - lastClickTime > .9f)  clickTriggered = false;
            EventManager.TriggerEvent("click");
        }
    }

    private void ManipulationForOneHand()
    {
        Vector3 focusPos = FocusedObject.transform.position; //The player gazes at the item which will catch
        Vector3 handPos = trackingHands.ElementAt(0).Value.hand.transform.position; //The position of user's hand in the Holospace
        Bounds focusedObjectBounds = FocusedObject.GetComponent<Renderer>().bounds; //The graphical bounds of the focused objects
        focusedObjectBounds.Expand(.15f);
        if (focusedObjectBounds.Contains(handPos))
        {
            if (!ObjectManipulationInProgress)
            {
                ChangeObjectColor(trackingHands[trackingHands.ElementAt(0).Key].hand, TouchedColor);
                ObjectTouched = true;
                ManipulationWithTwoHands = false;
                TouchedObject = FocusedObject;
                if (!ColorOutlineChanged)
                {
                    EnableOutline(TouchedObject);
                    ColorOutlineChanged = true;
                }
                //Data
                interactionAnalysis.interactionTouched();
            }
        }
        else
        {
            ChangeObjectColor(trackingHands[trackingHands.ElementAt(0).Key].hand, DefaultColor);
            ObjectTouched = false;
            TouchedObject = null;
            DisableOutline(FocusedObject);
            ColorOutlineChanged = false;
            //Data
            interactionAnalysis.interactionNotTouched();
        }
    }

    private void ManipulationForTwoHands()
    {
        Vector3 focusPos = FocusedObject.transform.position; //The player gazes at the item which will catch
        Vector3 firstHand = trackingHands.ElementAt(0).Value.hand.transform.position; //The position of user's hand in the Holospace
        Vector3 secondHand = trackingHands.ElementAt(1).Value.hand.transform.position; //The position of user's hand in the Holospace
        Bounds focusedObjectBounds = FocusedObject.GetComponent<Renderer>().bounds; //The graphical bounds of the focused objects
        focusedObjectBounds.Expand(.15f);
        if (focusedObjectBounds.Contains(firstHand) && focusedObjectBounds.Contains(secondHand))
        {
            if (!ObjectManipulationInProgress)
            {
                ChangeObjectColor(trackingHands, TouchedColor);
                ObjectTouched = true;
                ManipulationWithTwoHands = true;
                TouchedObject = FocusedObject;
                if (!ColorOutlineChanged)
                {
                    EnableOutline(TouchedObject);
                    ColorOutlineChanged = true;
                }
                //Data
                interactionAnalysis.interactionTouched();
            }
        }
        else
        {
            ChangeObjectColor(trackingHands, DefaultColor);
            ObjectTouched = false;
            TouchedObject = null;
            DisableOutline(FocusedObject);
            ColorOutlineChanged = false;
            //Data
            interactionAnalysis.interactionNotTouched();
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
                outline.OutlineMode = Outline.Mode.OutlineAll;
                outline.OutlineWidth = 5f;
            }
            outline.OutlineColor = OutlineDefaultColor;
            outline.enabled = true;
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
        var rend = obj.GetComponent<Renderer>();
        if (rend) rend.material.color = color;
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
            ManipulatedObject = TouchedObject;
            ChangeColorOutline(ManipulatedObject, OutlineManipulateColor);
            initPos = ManipulatedObject.transform.position;
            ManipulatedObject.GetComponent<AppleScript>().disableWind();
            if (!ManipulationWithTwoHands)
            {
                ManipulatedObject.transform.position = trackingHands[id].hand.transform.position;
            }
            else
            {
                GameObject firstHandObject = trackingHands.ElementAt(0).Value.hand;
                GameObject secondHandObject = trackingHands.ElementAt(1).Value.hand;
                ManipulatedObject.transform.position = Vector3.Lerp(firstHandObject.transform.position, secondHandObject.transform.position, .5f);
            }
            ObjectCollectionManager.Instance.appearBox(manipulationCounter, initPos);
            //Data
            interactionAnalysis.manipulationStarted();
        }
    }

    private void GestureRecognizer_ManipulationUpdated(ManipulationUpdatedEventArgs args)
    {
        if (trackingHands.ContainsKey(args.source.id) && ObjectManipulationInProgress)
        {
            if (!ManipulationWithTwoHands)
            {
                if (getDistanceObjects(trackingHands[args.source.id].hand.transform , Camera.main.transform) > flowController.getHeadDistanceLimit())
                {
                    ManipulatedObject.transform.position = trackingHands[args.source.id].hand.transform.position;
                }
                else
                {
                    ManipulatedObject.transform.position = initPos;
                    ObjectManipulationInProgress = false;
                    ManipulatedObject = null;
                }
            }
            else
            {
                //
                // ελεγχος αποσταστης απο κεφαλη και απο εδαφος , αν πληρει προχωραμε κανονικα διαφορετικα διαφορετικη θεση --WIP
                //
                GameObject firstHand = trackingHands.ElementAt(0).Value.hand;
                GameObject secondHand = trackingHands.ElementAt(1).Value.hand;
                ManipulatedObject.transform.position = Vector3.Lerp(firstHand.transform.position, secondHand.transform.position, .5f);
                /*
                Interpolator interpolator = ManipulatedObject.GetComponent<Interpolator>();
                interpolator.SetTargetPosition(Vector3.Lerp(firstHand.transform.position, secondHand.transform.position, .5f));
                */
            }
        }
    }

    private void GestureRecognizer_ManipulationCompleted(ManipulationCompletedEventArgs args)
    {
        if (trackingHands.ContainsKey(args.source.id) && ObjectManipulationInProgress)
        {
            manipulationCounter++;
            DisableOutline(ManipulatedObject);
            EnableGravity(ManipulatedObject);
            ChangeObjectColor(trackingHands[args.source.id].hand, DefaultColor);
            if (!ManipulationWithTwoHands)
            {
                ObjectManipulationInProgress = false;
                ManipulatedObject = null;
            }
            else
            {
                ManipulationWithTwoHands = false;
                ObjectManipulationInProgress = false;
                ManipulatedObject = null;
            }
            //Data
            interactionAnalysis.manipulationFinished();
        }
    }

    private void GestureRecognizer_ManipulationCanceled(ManipulationCanceledEventArgs args)
    {
        if (trackingHands.ContainsKey(args.source.id) && ObjectManipulationInProgress)
        {
            DisableOutline(ManipulatedObject);
            EnableGravity(ManipulatedObject);
            ChangeObjectColor(trackingHands[args.source.id].hand, DefaultColor);
            if (!ManipulationWithTwoHands)
            {
                ObjectManipulationInProgress = false;
                ManipulatedObject = null;
            }
            else
            {
                ManipulationWithTwoHands = false;
                ObjectManipulationInProgress = false;
                ManipulatedObject = null;
            }
            //Data
            interactionAnalysis.manipulationCanceled();
        }
    }

    private void GestureRecognizer_Tapped(TappedEventArgs args)
    {            
        uint id = args.source.id;
        if (trackingHands.ContainsKey(id))
        {
            ChangeObjectColor(trackingHands[id].hand, TapColor);
        }
        EventManager.TriggerEvent("tap");
    }

    private void GestureRecognizer_HoldStarted(HoldStartedEventArgs args)
    {
        if (TouchedObject != null)
        {
            ChangeColorOutline(TouchedObject, OutlineHoldColor);
            //Data
            interactionAnalysis.interactionHold();
        }
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
            StatusText.text = "New hand calib";
            if (dot <= 0) //left hand
            {
                trackingHands.Add(obj.state.source.id, new HandStruct(hand, obj.state.source.id, false));
                if (HandCalibrationMode)
                    calibrationController = new CalibrationController(false, Time.time);
            }
            else  //right hand
            {
                trackingHands.Add(obj.state.source.id, new HandStruct(hand, obj.state.source.id, true));
                if (HandCalibrationMode)
                    calibrationController = new CalibrationController(true, Time.time);
            }
            /*
            if (pos.x > 0)
                trackingHands.Add(obj.state.source.id, new HandStruct(hand, obj.state.source.id, true));
            else
                trackingHands.Add(obj.state.source.id, new HandStruct(hand, obj.state.source.id, false));
            */
            //Data
            if(DataCollectionMode)
                interactionAnalysis.interactionDetected();
        }
    }

    private void InteractionManager_InteractionSourceUpdated(InteractionSourceUpdatedEventArgs args)
    {
        uint id = args.state.source.id;
        if (trackingHands.ContainsKey(id) && args.state.source.kind == InteractionSourceKind.Hand)
        {
            Vector3 pos;
            if (args.state.sourcePose.TryGetPosition(out pos))
                trackingHands[id].hand.transform.position = pos;
            float time = Time.time;
            if (HandCalibrationMode)
            {
                float dist = Mathf.Sqrt(Mathf.Pow(Camera.main.transform.position.x - pos.x, 2) + Mathf.Pow(Camera.main.transform.position.z - pos.z, 2));
                float height = Mathf.Abs(Camera.main.transform.position.y - pos.y);
                calibrationController.addValue(dist, height);
            }
            if (DataCollectionMode)
            {
                if (privPos != Vector3.zero)
                {
                    Vector3 tremor = (pos - privPos) / (time - privTime);
                    interactionAnalysis.addTremorValue(tremor);
                }
                privTime = time;
                privPos = pos;
            }
        }
    }   

    private void InteractionManager_InteractionSourceLost(InteractionSourceLostEventArgs args)
    {
        uint id = args.state.source.id;
        if (trackingHands.ContainsKey(id) && args.state.source.kind == InteractionSourceKind.Hand)
        {
            if (HandCalibrationMode)
            {
                if (flowController.addCalibrationController(calibrationController, Time.time) > 1)
                {
                    HandCalibrationMode = false;
                    flowController.calibrationFinished();
                }
                else
                {
                    StatusText.text = "Second Hand Calib waiting";
                    calibrationController = null;
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
            if (HandCalibrationMode)
            {
                if (flowController.addCalibrationController(calibrationController, Time.time) > 1)
                {
                    HandCalibrationMode = false;
                    flowController.calibrationFinished();
                }
                else
                {
                    StatusText.text = "Second Hand Calib waiting";
                    calibrationController = null;
                }
            }
            Destroy(trackingHands[id].hand);
            trackingHands.Remove(id);
        }
    }

    private void InteractionManager_InteractionSourcePressed(InteractionSourcePressedEventArgs args)
    {
        if (args.state.source.kind == InteractionSourceKind.Controller)
        {
            if (FocusedObject != null)
            {
                var outline = FocusedObject.GetComponent<Outline>();
                if (outline != null)
                {
                    if (outline.enabled == false)
                        EnableOutline(FocusedObject);
                    else
                        DisableOutline(FocusedObject);
                }
            }
            if (!clickTriggered) //First click
            {
                clickTriggered = true;
                lastClickTime = Time.time;
            }
            else //Second click
            {
                doubleClickTriggered = true;
            }
        }
    }

    private void EnableGravity(GameObject obj)
    {
        obj.GetComponent<Rigidbody>().useGravity = true;
    }

    private void DisableGravity(GameObject obj)
    {
        obj.GetComponent<Rigidbody>().useGravity = false;
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
        gestureRecognizer.HoldCanceled -= GestureRecognizer_HoldCanceled;
        gestureRecognizer.HoldCompleted -= GestureRecognizer_HoldCompleted;
    }

    private float getDistanceObjects(Transform obj1, Transform obj2)
    {
        return Vector3.Magnitude(obj1.position - obj2.position); 
    }

    public void enableHandCalibration()
    {
        HandCalibrationMode = true;
        HandManipulationMode = false;
    }

    public void enableHandManipulation()
    {
        HandCalibrationMode = false;
        HandManipulationMode = true;
    }

    public void enableDataCollection()
    {
        DataCollectionMode = true;
    }

    public void disableDataCollection()
    {
        DataCollectionMode = false;
    }
}