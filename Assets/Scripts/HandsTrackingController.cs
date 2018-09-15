using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        public Color TouchedColor = Color.magenta;
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
            cursor = GameObject.Find("Cursor").GetComponent<GazeCursor>();

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
            if (FocusedObject != null)
            {
                Bounds focusedObjectBounds = FocusedObject.GetComponent<Renderer>().bounds; //The graphical bounds of the focused objects
                focusedObjectBounds.Expand(.15f);
                if (focusedObjectBounds.Contains(firstHand) && focusedObjectBounds.Contains(secondHand))
                {
                    ChangeObjectColor(trackingHands[trackingHands.ElementAt(0).Key], TouchedColor);
                    ChangeObjectColor(trackingHands[trackingHands.ElementAt(1).Key], TouchedColor);
                    ObjectTouched = true;
                    ManipulationWithTwoHands = true;
                    TouchedObject = FocusedObject;
                    EnableOutline(FocusedObject);
                }
                else
                {
                    ChangeObjectColor(trackingHands[trackingHands.ElementAt(0).Key], DefaultColor);
                    ChangeObjectColor(trackingHands[trackingHands.ElementAt(1).Key], DefaultColor);
                    ObjectTouched = false;
                    TouchedObject = null;
                    DisableOutline(FocusedObject);
                }
            }
        }

        private void ManipulationForOneHand()
        {
            Vector3 focusPos = FocusedObject.transform.position; //The player gazes at the item which will catch
            Vector3 handPos = trackingHands.ElementAt(0).Value.transform.position; //The position of user's hand in the Holospace
            Bounds focusedObjectBounds = FocusedObject.GetComponent<Renderer>().bounds; //The graphical bounds of the focused objects
            focusedObjectBounds.Expand(.15f);
            if (FocusedObject != null)
            {
                if (focusedObjectBounds.Contains(handPos))
                {
                    if (!objectManipulationInProgress)
                    {
                        ChangeObjectColor(trackingHands[trackingHands.ElementAt(0).Key], TouchedColor);
                        ObjectTouched = true;
                        ManipulationWithTwoHands = false;
                        TouchedObject = FocusedObject;
                        EnableOutline(TouchedObject);
                    }
                }
                else
                {
                    ChangeObjectColor(trackingHands[trackingHands.ElementAt(0).Key], DefaultColor);
                    ObjectTouched = false;
                    TouchedObject = null;
                    DisableOutline(FocusedObject);
                }
            }
        }

        private void ChangeColorOutline(GameObject focusedObject, Color color)
        {
            if (focusedObject != null)
            {
                var outline = focusedObject.GetComponentInChildren<Outline>();
                if (outline != null) outline.OutlineColor = color;
            }
        }

        private void DisableOutline(GameObject focusedObject)
        {
            if (focusedObject != null)
            {
                var outline = focusedObject.GetComponentInChildren<Outline>();
                if (outline != null) outline.enabled = false;
            }
        }

        private void EnableOutline(GameObject focusedObject)
        {
            if (focusedObject != null)
            {
                var outline = focusedObject.GetComponentInChildren<Outline>();
                if(outline == null)
                {
                    //Create Outline
                    outline = gameObject.AddComponent<Outline>();
                    outline.OutlineMode = Outline.Mode.OutlineAll;
                    outline.OutlineWidth = 5f;
                    if (!objectManipulationInProgress) outline.OutlineColor = Color.red;
                    outline.enabled = true;
                }
                else
                {
                    if (!objectManipulationInProgress) outline.OutlineColor = Color.red;
                    outline.enabled = true;
                }
            }
        }

        private void ChangeObjectColor(GameObject obj, Color color)
        {            
            var rend = obj.GetComponentInChildren<Renderer>();
            if (rend)
                rend.material.color = color;
        }

        private void GestureRecognizer_ManipulationStarted(ManipulationStartedEventArgs args)
        {
            uint id = args.source.id;
            if (trackingHands.ContainsKey(id) && ObjectTouched)
            {
                objectManipulationInProgress = true;
                ManipulatedObject = TouchedObject;
                ChangeColorOutline(ManipulatedObject, Color.green);
                if (!ManipulationWithTwoHands)
                {
                    GameObject currentHandObject;
                    trackingHands.TryGetValue(id, out currentHandObject);
                    ManipulatedObject.transform.position = currentHandObject.transform.position;
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
                if (!ManipulationWithTwoHands)
                {
                    GameObject currentHandObject;// OR = trackingHands[id];
                    trackingHands.TryGetValue(id, out currentHandObject);
                    GameObject currentManipulatedObject = handAndManipulatedObjectCombo[id];
                    currentManipulatedObject.transform.position = currentHandObject.transform.position;
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
                DisableOutline(ManipulatedObject);
                EnableGravity(ManipulatedObject);
                ChangeObjectColor(trackingHands[args.source.id], DefaultColor);
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
                ChangeObjectColor(trackingHands[args.source.id], DefaultColor);
                DisableOutline(ManipulatedObject);
                EnableGravity(ManipulatedObject);
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
            if (obj.state.source.kind == InteractionSourceKind.Hand)
            {

                var hand = Instantiate(TrackingObject) as GameObject;
                Vector3 pos;

                if (obj.state.sourcePose.TryGetPosition(out pos))
                    hand.transform.position = pos;

                trackingHands.Add(id, hand);
            }
        }

        private void InteractionManager_InteractionSourceUpdated(InteractionSourceUpdatedEventArgs args)
        {
            uint id = args.state.source.id;

            if (trackingHands.ContainsKey(id) && args.state.source.kind == InteractionSourceKind.Hand)
            {
                Vector3 pos;
                Quaternion rot;
                if (args.state.sourcePose.TryGetPosition(out pos))
                    trackingHands[id].transform.position = pos;

                if (args.state.sourcePose.TryGetRotation(out rot))
                    trackingHands[id].transform.rotation = rot;
            }
        }   

        private void InteractionManager_InteractionSourceLost(InteractionSourceLostEventArgs args)
        {
            uint id = args.state.source.id;

            if (trackingHands.ContainsKey(id) && args.state.source.kind == InteractionSourceKind.Hand)
            {
                var obj = trackingHands[id];
                trackingHands.Remove(id);
                Destroy(obj);
            }
        }

        private void EnableGravity(GameObject obj)
        {
            obj.GetComponent<Rigidbody>().useGravity = true;
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