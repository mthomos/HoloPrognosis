using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.WSA.Input;

namespace HoloPrognosis
{
    /// <summary>
    /// HandsTrackingController : TO_DO
    /// </summary>
    public class HandsTrackingController : MonoBehaviour
    {
        //Public Variables - For Editor
        //GameObjects
        private GazeCursor cursor; // Cusrsor used for defining FocusedObject
        public TextMesh StatusText; // Text for debugging (for now)
        public GameObject TrackingObject; // GameObject representing the hand in holographic space
        //Colors
        public Color DefaultColor = Color.green;
        public Color TapColor = Color.white;
        public Color HoldStartedColor = Color.red;
        public Color HoldCanceledColor = Color.magenta;
        public Color HoldCompletedColor = Color.yellow;
        public Color ManipulateColor = Color.cyan;
        //Private Variables
        //Booleans
        private bool ObjectTouched = false;
        private bool objectManipulationInProgress = false;
        private bool ManipulationWithTwoHands = false;
        // trackingHands: In this dictionary, hands which generate intaractions and scanned by Hololens are stored
        private Dictionary<uint, GameObject> trackingHands = new Dictionary<uint, GameObject>();
        /* 
        Summary: handAndManipulatedObjectCombo
        In this dictionary we store the the hand that manipulate a particular object, so we can know for every
        hand (interraction input) which Object manipulates
        Key : Hand uid , Value: Object which is being manipulated
        */
        private Dictionary<uint, GameObject> handAndManipulatedObjectCombo = new Dictionary<uint, GameObject>();
        //Checking Manipulation Objects
        private GameObject TouchedObject; // GameObject which user touched and is candidate for Manipulation
        private GameObject ManipulatedObject;// GameObject which is being Manipulated by the user
        private GameObject FocusedObject; // GameObject which the user gazes at
        private GestureRecognizer gestureRecognizer;
        private Outline outlineComponent; //Outline for TouchedObject

        void Awake()
        {
            InteractionManager.InteractionSourceDetected += InteractionManager_InteractionSourceDetected;
            InteractionManager.InteractionSourceUpdated += InteractionManager_InteractionSourceUpdated;
            InteractionManager.InteractionSourceLost += InteractionManager_InteractionSourceLost;
            gestureRecognizer = new GestureRecognizer();
            gestureRecognizer.SetRecognizableGestures(GestureSettings.Tap | GestureSettings.ManipulationTranslate);
            gestureRecognizer.Tapped += GestureRecognizer_Tapped;
            gestureRecognizer.ManipulationStarted += GestureRecognizer_ManipulationStarted;
            gestureRecognizer.ManipulationCompleted += GestureRecognizer_ManipulationCompleted;
            gestureRecognizer.ManipulationCanceled += GestureRecognizer_ManipulationCanceled;
            gestureRecognizer.ManipulationUpdated += GestureRecognizer_ManipulationUpdated; 
            gestureRecognizer.StartCapturingGestures();
            cursor = GameObject.Find("Gaze").GetComponent<GazeCursor>();
        }

        void Update()
        {
            FocusedObject = cursor.getFocusedObject();
            if (FocusedObject != null)
            {
                int HandsNeeded = ObjectCollectionManager.Instance.GetHandsNeededForManipulation(FocusedObject.GetInstanceID());
                if (HandsNeeded <= trackingHands.Count && HandsNeeded > 0 && FocusedObject != null)
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

        private void ManipulationForTwoHands()
        {
            Vector3 focusPos = FocusedObject.transform.position; //The player gazes at the item which will catch
            Vector3 firstHand = trackingHands.ElementAt(0).Value.transform.position; //The position of user's hand in the Holospace
            Vector3 secondHand = trackingHands.ElementAt(1).Value.transform.position; //The position of user's hand in the Holospace
            Bounds focusedObjectBounds = FocusedObject.GetComponent<Renderer>().bounds; //The graphical bounds of the focused objects
            focusedObjectBounds.Expand(.1f);
            if (focusedObjectBounds.Contains(firstHand) && focusedObjectBounds.Contains(secondHand))
            {
                ChangeObjectColor(trackingHands[trackingHands.ElementAt(0).Key], Color.magenta);
                ChangeObjectColor(trackingHands[trackingHands.ElementAt(1).Key], Color.magenta);
                StatusText.text = "Focus Object Touched";
                ObjectTouched = true;
                ManipulationWithTwoHands = true;
                TouchedObject = FocusedObject;
                EnableOutline(FocusedObject);
            }
            else
            {
                ObjectTouched = false;
                TouchedObject = null;
                DisableOutline(FocusedObject);
            }
        }

        private void ManipulationForOneHand()
        {
            Vector3 focusPos = FocusedObject.transform.position; //The player gazes at the item which will catch
            Vector3 handPos = trackingHands.ElementAt(0).Value.transform.position; //The position of user's hand in the Holospace
            //First approach : Use euclidean distance
            /*
            float dx = Mathf.Abs(handPos.x - focusPos.x);
            float dy = Mathf.Abs(handPos.y - focusPos.y);
            float dz = Mathf.Abs(handPos.z - focusPos.z);
            if (dx < 0.05 && dy < 0.05 && dz < 0.05)
            */
            // Another approach : Using graphical bounds of the object and expand them
            Bounds focusedObjectBounds = FocusedObject.GetComponent<Renderer>().bounds; //The graphical bounds of the focused objects
            focusedObjectBounds.Expand(.1f);
            if (focusedObjectBounds.Contains(handPos))
            {
                ChangeObjectColor(trackingHands[trackingHands.ElementAt(0).Key], Color.magenta);
                StatusText.text = "Focus Object Touched";
                ObjectTouched = true;
                ManipulationWithTwoHands = false;
                TouchedObject = FocusedObject;
                EnableOutline(FocusedObject);
            }
            else
            {
                ObjectTouched = false;
                TouchedObject = null;
                DisableOutline(FocusedObject);
            }
        }

        private void DisableOutline(GameObject focusedObject)
        {
            if (focusedObject == null)
            {
                if (outlineComponent != null)
                    outlineComponent.enabled = false;
            }
        }

        private void EnableOutline(GameObject focusedObject)
        {
            if (focusedObject == null)
            {
                if (outlineComponent != null)
                    outlineComponent.enabled = false;
                outlineComponent = focusedObject.GetComponent<Outline>();
                if (outlineComponent != null)
                    outlineComponent.enabled = true;
            }
        }

        private void ChangeObjectColor(GameObject obj, Color color)
        {            
            var rend = obj.GetComponentInChildren<Renderer>();
            if (rend)
            {
                rend.material.color = color;
                Debug.LogFormat("Color Change: {0}", color.ToString());
            }
        }

        private void GestureRecognizer_ManipulationStarted(ManipulationStartedEventArgs args)
        {
            uint id = args.source.id;
            if (trackingHands.ContainsKey(id) && ObjectTouched)
            {
                objectManipulationInProgress = true;
                ManipulatedObject = TouchedObject;
                if (!ManipulationWithTwoHands)
                {
                    GameObject currentHandObject;
                    trackingHands.TryGetValue(id, out currentHandObject);
                    ManipulatedObject.transform.position = currentHandObject.transform.position;
                    //ManipulatedObject.transform.rotation = currentHandObject.transform.rotation;
                    handAndManipulatedObjectCombo.Add(id, ManipulatedObject);
                }
                else
                {
                    GameObject firstHand = trackingHands.ElementAt(0).Value;
                    GameObject secondHand = trackingHands.ElementAt(1).Value;
                    ManipulatedObject.transform.position = Vector3.Lerp(firstHand.transform.position, secondHand.transform.position, .5f);
                    handAndManipulatedObjectCombo.Add(id, ManipulatedObject);
                }
            }
        }

        private void GestureRecognizer_ManipulationUpdated(ManipulationUpdatedEventArgs args)
        {
            uint id = args.source.id;
            if (trackingHands.ContainsKey(args.source.id) && objectManipulationInProgress)
            {
                StatusText.text = "Manipulation Updated";
                if (!ManipulationWithTwoHands)
                {
                    GameObject currentHandObject;// OR = trackingHands[id];
                    trackingHands.TryGetValue(id, out currentHandObject);
                    GameObject currentManipulatedObject = handAndManipulatedObjectCombo[id];
                    currentManipulatedObject.transform.position = currentHandObject.transform.position;
                    //currentManipulatedObject.transform.rotation = currentHandObject.transform.rotation;
                }
                else
                {
                    GameObject firstHand = trackingHands.ElementAt(0).Value;
                    GameObject secondHand = trackingHands.ElementAt(1).Value;
                    ManipulatedObject.transform.position = Vector3.Lerp(firstHand.transform.position, secondHand.transform.position, .5f);
                }
            }
        }

        void GestureRecognizer_ManipulationCompleted(ManipulationCompletedEventArgs args)
        {
            if (trackingHands.ContainsKey(args.source.id) && objectManipulationInProgress)
            {
                StatusText.text = "Manipulation Completed";
                if (!ManipulationWithTwoHands)
                {
                    objectManipulationInProgress = false;
                    ManipulatedObject = null;
                    handAndManipulatedObjectCombo.Remove(args.source.id);
                }
                else
                {
                    ManipulationWithTwoHands = false;
                    objectManipulationInProgress = false;
                    ManipulatedObject = null;
                    handAndManipulatedObjectCombo.Clear();
                }
            }
        }

        private void GestureRecognizer_ManipulationCanceled(ManipulationCanceledEventArgs args)
        {
            if (trackingHands.ContainsKey(args.source.id) && objectManipulationInProgress)
            {
                StatusText.text = "Manipulation Canceled";
                if (!ManipulationWithTwoHands)
                {
                    objectManipulationInProgress = false;
                    ManipulatedObject = null;
                    handAndManipulatedObjectCombo.Remove(args.source.id);
                }
                else
                {
                    ManipulationWithTwoHands = false;
                    objectManipulationInProgress = false;
                    ManipulatedObject = null;
                    handAndManipulatedObjectCombo.Clear();
                }
            }
        }

        private void GestureRecognizer_Tapped(TappedEventArgs args)
        {            
            uint id = args.source.id;
            if (trackingHands.ContainsKey(id))
            {
                ChangeObjectColor(trackingHands[id], TapColor);
            }
            EventManager.TriggerEvent("tap");
        }

        private void InteractionManager_InteractionSourceDetected(InteractionSourceDetectedEventArgs obj)
        {
            uint id = obj.state.source.id;
            // Check to see that the source is a hand.
            if (obj.state.source.kind != InteractionSourceKind.Hand)
                return;
           
            var hand = Instantiate(TrackingObject) as GameObject;
            Vector3 pos;

            if (obj.state.sourcePose.TryGetPosition(out pos))
                hand.transform.position = pos;

            trackingHands.Add(id, hand);
        }

        void InteractionManager_InteractionSourceUpdated(InteractionSourceUpdatedEventArgs args)
        {
            uint id = args.state.source.id;

            if (args.state.source.kind == InteractionSourceKind.Hand)
            {
                StatusText.text = " Hand Interaction updated";
                if (trackingHands.ContainsKey(id))
                {
                    Vector3 pos;
                    Quaternion rot;
                    if (args.state.sourcePose.TryGetPosition(out pos))
                        trackingHands[id].transform.position = pos;

                    if (args.state.sourcePose.TryGetRotation(out rot))
                        trackingHands[id].transform.rotation = rot;
                }
            }
        }   

        private void InteractionManager_InteractionSourceLost(InteractionSourceLostEventArgs args)
        {
            uint id = args.state.source.id;
            if (args.state.source.kind != InteractionSourceKind.Hand)
                return;
            StatusText.text = " Hand Interaction lost";
            if (trackingHands.ContainsKey(id))
            {
                var obj = trackingHands[id];
                trackingHands.Remove(id);
                Destroy(obj);
            }
        }

        void OnDestroy()
        {                        
            InteractionManager.InteractionSourceDetected -= InteractionManager_InteractionSourceDetected;
            InteractionManager.InteractionSourceUpdated -= InteractionManager_InteractionSourceUpdated;
            InteractionManager.InteractionSourceLost -= InteractionManager_InteractionSourceLost;
            gestureRecognizer.Tapped -= GestureRecognizer_Tapped;
            gestureRecognizer.ManipulationStarted -= GestureRecognizer_ManipulationStarted;
            gestureRecognizer.ManipulationCompleted -= GestureRecognizer_ManipulationCompleted;
            gestureRecognizer.ManipulationCanceled -= GestureRecognizer_ManipulationCanceled;
            gestureRecognizer.ManipulationUpdated -= GestureRecognizer_ManipulationUpdated;
            gestureRecognizer.StopCapturingGestures();
        }
    }
}