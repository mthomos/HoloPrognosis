using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.WSA.Input;
using HoloToolkit.Unity;

namespace HoloPrognosis
{
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
        public Color OutlineHoldColor = Color.blue;
        public Color OutlineDefaultColor = Color.red;
        public Color OutlineManipulateColor = Color.green;
        public Color HoldFinished = Color.white;
        //Private Variables
        //Booleans
        private bool ObjectTouched = false;
        private bool ObjectManipulationInProgress = false;
        private bool ManipulationWithTwoHands = false;
        // trackingHands: In this dictionary, hands which generate intaractions and scanned by Hololens are stored
        private Dictionary<uint, GameObject> trackingHands = new Dictionary<uint, GameObject>();
        //Checking Manipulation Objects
        private GameObject TouchedObject; // GameObject which user touched and is candidate for Manipulation
        private GameObject ManipulatedObject;// GameObject which is being Manipulated by the user
        private GameObject FocusedObject; // GameObject which the user gazes at
        private GestureRecognizer gestureRecognizer;
        //Experimental
        private bool colorOutlineChanged;
        //
        private int clicker;
        private float lastClickTime = .0f;
        private bool clickTriggered;
        private bool doubleClickTriggered;
        //
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
            cursor = GameObject.Find("Cursor").GetComponent<GazeCursor>();
        }

        void Update()
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
            //Check for clcker
            CheckForClicker();
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
                clickTriggered = false;
                EventManager.TriggerEvent("click");
            }
        }

        private void ManipulationForOneHand()
        {
            Vector3 focusPos = FocusedObject.transform.position; //The player gazes at the item which will catch
            Vector3 handPos = trackingHands.ElementAt(0).Value.transform.position; //The position of user's hand in the Holospace
            Bounds focusedObjectBounds = FocusedObject.GetComponent<Renderer>().bounds; //The graphical bounds of the focused objects
            focusedObjectBounds.Expand(.15f);
            if (focusedObjectBounds.Contains(handPos))
            {
                if (!ObjectManipulationInProgress)
                {
                    ChangeObjectColor(trackingHands[trackingHands.ElementAt(0).Key], TouchedColor);
                    ObjectTouched = true;
                    ManipulationWithTwoHands = false;
                    TouchedObject = FocusedObject;
                    if (!colorOutlineChanged)
                    {
                        EnableOutline(TouchedObject);
                        colorOutlineChanged = true;
                    }
                }
            }
            else
            {
                ChangeObjectColor(trackingHands[trackingHands.ElementAt(0).Key], DefaultColor);
                ObjectTouched = false;
                TouchedObject = null;
                DisableOutline(FocusedObject);
                colorOutlineChanged = false;
            }
        }

        private void ManipulationForTwoHands()
        {
            Vector3 focusPos = FocusedObject.transform.position; //The player gazes at the item which will catch
            Vector3 firstHand = trackingHands.ElementAt(0).Value.transform.position; //The position of user's hand in the Holospace
            Vector3 secondHand = trackingHands.ElementAt(1).Value.transform.position; //The position of user's hand in the Holospace
            Bounds focusedObjectBounds = FocusedObject.GetComponent<Renderer>().bounds; //The graphical bounds of the focused objects
            focusedObjectBounds.Expand(.15f);
            if (focusedObjectBounds.Contains(firstHand) && focusedObjectBounds.Contains(secondHand))
            {
                if (!ObjectManipulationInProgress)
                {
                    ChangeObjectColor(trackingHands[trackingHands.ElementAt(0).Key], TouchedColor);
                    ChangeObjectColor(trackingHands[trackingHands.ElementAt(1).Key], TouchedColor);
                    ObjectTouched = true;
                    ManipulationWithTwoHands = true;
                    TouchedObject = FocusedObject;
                    if (!colorOutlineChanged)
                    {
                        EnableOutline(TouchedObject);
                        colorOutlineChanged = true;
                    }
                }
            }
            else
            {
                ChangeObjectColor(trackingHands[trackingHands.ElementAt(0).Key], DefaultColor);
                ChangeObjectColor(trackingHands[trackingHands.ElementAt(1).Key], DefaultColor);
                ObjectTouched = false;
                TouchedObject = null;
                DisableOutline(FocusedObject);
                colorOutlineChanged = false;
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

        private void GestureRecognizer_ManipulationStarted(ManipulationStartedEventArgs args)
        {
            uint id = args.source.id;
            if (trackingHands.ContainsKey(id) && ObjectTouched)
            {
                ObjectManipulationInProgress = true;
                ManipulatedObject = TouchedObject;
                ChangeColorOutline(ManipulatedObject, OutlineManipulateColor);
                if (!ManipulationWithTwoHands)
                {
                    GameObject currentHandObject = trackingHands[id];
                    ManipulatedObject.transform.position = currentHandObject.transform.position;
                }
                else
                {
                    GameObject firstHandObject = trackingHands.ElementAt(0).Value;
                    GameObject secondHandObject = trackingHands.ElementAt(1).Value;
                    ManipulatedObject.transform.position = Vector3.Lerp(firstHandObject.transform.position, secondHandObject.transform.position, .5f);

                }
            }
        }

        private void GestureRecognizer_ManipulationUpdated(ManipulationUpdatedEventArgs args)
        {
            uint id = args.source.id;
            if (trackingHands.ContainsKey(args.source.id) && ObjectManipulationInProgress)
            {
                if (!ManipulationWithTwoHands)
                {
                    GameObject currentHandObject = trackingHands[id];
                    Interpolator interpolator = ManipulatedObject.GetComponent<Interpolator>();
                    interpolator.SetTargetPosition(currentHandObject.transform.position);
                    //ManipulatedObject.transform.position = currentHandObject.transform.position;
                }
                else
                {
                    GameObject firstHand = trackingHands.ElementAt(0).Value;
                    GameObject secondHand = trackingHands.ElementAt(1).Value;
                    //ManipulatedObject.transform.position = Vector3.Lerp(firstHand.transform.position, secondHand.transform.position, .5f);
                    Interpolator interpolator = ManipulatedObject.GetComponent<Interpolator>();
                    interpolator.SetTargetPosition(Vector3.Lerp(firstHand.transform.position, secondHand.transform.position, .5f));
                }
            }
        }

        void GestureRecognizer_ManipulationCompleted(ManipulationCompletedEventArgs args)
        {
            if (trackingHands.ContainsKey(args.source.id) && ObjectManipulationInProgress)
            {
                DisableOutline(ManipulatedObject);
                EnableGravity(ManipulatedObject);
                ChangeObjectColor(trackingHands[args.source.id], DefaultColor);
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
            }
        }

        private void GestureRecognizer_ManipulationCanceled(ManipulationCanceledEventArgs args)
        {
            if (trackingHands.ContainsKey(args.source.id) && ObjectManipulationInProgress)
            {
                DisableOutline(ManipulatedObject);
                EnableGravity(ManipulatedObject);
                ChangeObjectColor(trackingHands[args.source.id], DefaultColor);
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

        private void GestureRecognizer_HoldStarted(HoldStartedEventArgs args)
        {
            if (TouchedObject != null)
                ChangeColorOutline(TouchedObject, OutlineHoldColor);
            if (ManipulatedObject != null)
                ChangeColorOutline(ManipulatedObject, OutlineHoldColor);
            if (FocusedObject != null)
                ChangeColorOutline(FocusedObject, OutlineHoldColor);
        }

        private void GestureRecognizer_HoldCanceled(HoldCanceledEventArgs args)
        {
            if (TouchedObject != null)
                ChangeColorOutline(TouchedObject, HoldFinished);
            if (ManipulatedObject != null)
                ChangeColorOutline(ManipulatedObject, HoldFinished);
            if (FocusedObject != null)
                ChangeColorOutline(FocusedObject, HoldFinished);
        }

        private void GestureRecognizer_HoldCompleted(HoldCompletedEventArgs args)
        {
            if (TouchedObject != null)
                ChangeColorOutline(TouchedObject, HoldFinished);
            if (ManipulatedObject != null)
                ChangeColorOutline(ManipulatedObject, HoldFinished);
            if (FocusedObject != null)
                ChangeColorOutline(FocusedObject, HoldFinished);
        }

        private void InteractionManager_InteractionSourceDetected(InteractionSourceDetectedEventArgs obj)
        {
            if (obj.state.source.kind == InteractionSourceKind.Hand)
            {
                //Create hand object/cube
                var hand = Instantiate(TrackingObject) as GameObject;
                Vector3 pos;

                if (obj.state.sourcePose.TryGetPosition(out pos))
                    hand.transform.position = pos;
                //Add detected hand to hand dictionary
                trackingHands.Add(obj.state.source.id, hand);
            }
        }

        private void InteractionManager_InteractionSourceUpdated(InteractionSourceUpdatedEventArgs args)
        {
            uint id = args.state.source.id;

            if (trackingHands.ContainsKey(id) && args.state.source.kind == InteractionSourceKind.Hand)
            {
                Vector3 pos;
                if (args.state.sourcePose.TryGetPosition(out pos))
                    trackingHands[id].transform.position = pos;
            }
        }   

        private void InteractionManager_InteractionSourceLost(InteractionSourceLostEventArgs args)
        {
            uint id = args.state.source.id;

            if (trackingHands.ContainsKey(id) && args.state.source.kind == InteractionSourceKind.Hand)
            {
                Destroy(trackingHands[id]);
                trackingHands.Remove(id);
            }
        }

        private void InteractionManager_InteractionSourceReleased(InteractionSourceReleasedEventArgs args)
        {
            uint id = args.state.source.id;

            if (trackingHands.ContainsKey(id) && args.state.source.kind == InteractionSourceKind.Hand)
            {
                Destroy(trackingHands[id]);
                trackingHands.Remove(id);
            }
        }

        private void InteractionManager_InteractionSourcePressed(InteractionSourcePressedEventArgs args)
        {
            if (args.state.source.kind == InteractionSourceKind.Controller)
            {
                clicker++;
                StatusText.text = "Clicker clicked:" + clicker; ;
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
                if (!clickTriggered)
                {
                    clickTriggered = true;
                    lastClickTime = Time.time;
                }
                else
                {
                    if (Time.time - lastClickTime < 1.5f)
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
    }
}