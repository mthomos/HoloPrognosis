using System;
using System.Collections.Generic;
using UnityEngine;

public class FlowController : MonoBehaviour
{
    //Public Variables-For Editor
    //
    public GameObject menuPrefab;
    public GameObject settingsPrefab;
    public GameObject playPrefab;
    public GameObject resultsPrefab;
    public FileManager fileManager;
    public ObjectPlacer placer;
    public GazeCursor gazeCursor;
    public TextMesh DebugText;
    public HandsTrackingController handsTrackingController;
    public float menuDistance;
    // Settings
    private List<int> settings;
    private bool audioFeedbackEnabled = false;
    private bool clickerEnabled = false;
    // Training
    private bool trainingMode = false;
    private CalibrationController rightController, leftController;
    //Create timer variables
    private float timer;
    public float timerForGate;
    // Menu
    private GameObject menuScreen;
    private GameObject settingsScreen;
    private GameObject playScreen;
    private GameObject resultsScreen;
    private GameObject currentMenu;
    private int inMenu = -1; // Menu index
    private bool rightHandEnabled = true;
    private bool leftHandEnabled = true;
    private bool treeIsShort = true;
    //
    public float timerForRightPose = 3.0f;
    public float timerForHighPose = 3.0f;
    // Gate variables
    private GateScript gateScript;
    private bool objectInGateDetected;
    private bool freeToRelease;

    private int success, fail;
    private bool rightHandPlaying = false;

    private void Start ()
    {
        EventManager.StartListening("box_collision", successfulTry);
        EventManager.StartListening("floor_collision", failedTry);
        //Load Settings
        settings = fileManager.LoadSettings();
        for (int i=0; i< settings.Count; i++)
        {
            if(i==0)
            {
                if (settings[i] > 0)
                    audioFeedbackEnabled = true;
            }
            else if (i == 2)
            {
                if (settings[i] > 0)
                    clickerEnabled = true;
            }
        }
    }

    private void successfulTry()
    {
        if (freeToRelease)
        {
            success++;
            PrepareNextManipulation();
        }
        else
            failedTry();
    }

    private void failedTry()
    {
        fail++;
        PrepareNextManipulation();
    }

    public void PrepareNextManipulation()
    {
        if (!trainingMode)
            return;
        DebugText.text = "";
        rightHandPlaying = !rightHandPlaying;
        GameObject nowPlayingObject;
        if(rightHandPlaying)
        {
            nowPlayingObject = ObjectCollectionManager.Instance.getLowestFruit(rightController.getHighestPoseHandHeight());
            if (nowPlayingObject == null)
                nowPlayingObject = ObjectCollectionManager.Instance.getLowestFruit(leftController.getHighestPoseHandHeight());
        }
        else
        {
            nowPlayingObject = ObjectCollectionManager.Instance.getLowestFruit(leftController.getHighestPoseHandHeight());
            if (nowPlayingObject == null)
                nowPlayingObject = ObjectCollectionManager.Instance.getLowestFruit(rightController.getHighestPoseHandHeight());
        }
        if (nowPlayingObject == null)
            finishGame();
        else
        {
            UtilitiesScript.Instance.EnableOutline(nowPlayingObject, null);
            nowPlayingObject.tag = "User";
        }
    }

    private void Update ()
    {

        if (trainingMode)
        {
            // Calculate distance of manipulated object and gate
            if (gateScript == null)
                gateScript = ObjectCollectionManager.Instance.getCreatedGate().GetComponent<GateScript>();
            if (gateScript.objectInsideTree(handsTrackingController.getManipulatedObject()))
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

    private void tapUiReceived()
    {
        if (trainingMode)
        {
            finishGame();
            return;
        }
        
        GameObject tappedObj = gazeCursor.getFocusedObject();
        if (tappedObj.CompareTag("UI"))
        {
            if (inMenu == 0) //Start Menu
            {
                if (tappedObj.name == "StartButton")
                {
                    moveToPlayScreen();
                }
                else if (tappedObj.name == "SettingsButton")
                {
                    moveToSettingsScreen();
                }
                else if (tappedObj.name == "AboutButton")
                {
                    //nothing for now
                }
            }
            else if (inMenu == 1) //Setting Menu
            {
                if (tappedObj.name == "AudioFeedbackButton")
                {
                    audioFeedbackEnabled = (!audioFeedbackEnabled);
                    if (audioFeedbackEnabled)
                        tappedObj.GetComponentInChildren<TextMesh>().text = "Audio Feedback:" + "\n" + "On";
                    else
                        tappedObj.GetComponentInChildren<TextMesh>().text = "Audio Feedback:" + "\n" + "Off";
                }
                else if (tappedObj.name == "ClickerButton")
                {
                    clickerEnabled = !(clickerEnabled);
                    if (audioFeedbackEnabled)
                        tappedObj.GetComponentInChildren<TextMesh>().text = "Clicker Enabled:" + "\n" + "On";
                    else
                        tappedObj.GetComponentInChildren<TextMesh>().text = "Clicker Enabled:" + "\n" + "Off";
                }
                else if (tappedObj.name == "BackButton")
                {
                    returnToStartMenu();
                }
            }
            else if (inMenu == 2) // Play Menu
            {
                if (tappedObj.name == "PlayButton")
                    startPlaying();
                else if (tappedObj.name == "RightHandButton")
                {
                    rightHandEnabled = (!rightHandEnabled);
                    if (rightHandEnabled)
                        tappedObj.GetComponentInChildren<TextMesh>().text = "Right Hand:" + "\n" + "Yes";
                    else
                        tappedObj.GetComponentInChildren<TextMesh>().text = "Right Hand:" + "\n" + "No";
                }
                else if (tappedObj.name == "LeftHandButton")
                {
                    leftHandEnabled = (!leftHandEnabled);
                    if (rightHandEnabled)
                        tappedObj.GetComponentInChildren<TextMesh>().text = "Left Hand:" + "\n" + "Yes";
                    else
                        tappedObj.GetComponentInChildren<TextMesh>().text = "Left Hand:" + "\n" + "No";
                }

                else if (tappedObj.name == "SizeTreeButton")
                {
                    treeIsShort = (!treeIsShort);
                    if (treeIsShort)
                        tappedObj.GetComponentInChildren<TextMesh>().text = "Tree Height:" + "\n" + "Short";
                    else
                        tappedObj.GetComponentInChildren<TextMesh>().text = "Tree Height:" + "\n" + "Tall";
                }
                else if (tappedObj.name == "BackButton")
                {
                    returnToStartMenu();
                }
            }
            else if (inMenu == 3) // Results Menu
            {
                if (tappedObj.name == "BackButton")
                {
                    returnToStartMenu();
                }
            }
        }
    }

    private void moveToPlayScreen()
    {
        UtilitiesScript.Instance.disableObject(currentMenu);
        if (playScreen == null) //Create Play menu
            playScreen = Instantiate(playPrefab, currentMenu.transform.position, currentMenu.transform.rotation);
        else
             UtilitiesScript.Instance.enableObject(playScreen);
        currentMenu = playScreen;
        inMenu = 2;
    }

    private void moveToSettingsScreen()
    {
         UtilitiesScript.Instance.disableObject(currentMenu);
        if (settingsScreen == null) //Create Settings menu
            settingsScreen = Instantiate(settingsPrefab, currentMenu.transform.position, currentMenu.transform.rotation);
        else
             UtilitiesScript.Instance.enableObject(settingsScreen);
        currentMenu = settingsScreen;
        inMenu = 1;
    }

    private void returnToStartMenu()
    {
        UtilitiesScript.Instance.disableObject(currentMenu);
        currentMenu = menuScreen;
        UtilitiesScript.Instance.enableObject(currentMenu);
        inMenu = 0;
    }

    public void calibrationMaxPose()
    {
        DebugText.text = "Raise your hand as high as you can. When ready open your palm";
        TextToSpeech.Instance.StartSpeaking(DebugText.text);
    }

    //Public methods
    public void createUI()
    {
        //First listen for taps
        gazeCursor.setGenericUse();
        EventManager.StartListening("tap", tapUiReceived);
        //Appear the menu in front of user
        Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward * menuDistance;
        menuScreen = Instantiate(menuPrefab);
        menuScreen.transform.position = pos;
        Vector3 directionToTarget = Camera.main.transform.position - pos;
        directionToTarget.y = 0.0f;
        if (directionToTarget.sqrMagnitude > 0.005f)
            menuScreen.transform.rotation = Quaternion.LookRotation(-directionToTarget);

        inMenu = 0;
        currentMenu = menuScreen;
        placer.HideGridEnableOcclulsion();
    }

    public void startPlaying()
    {
        UtilitiesScript.Instance.disableObject(currentMenu);
        DebugText.text = "Place your hand in right angle pose for 2 seconds ";
        TextToSpeech.Instance.StartSpeaking(DebugText.text);
        //Set generic use for gaze
        gazeCursor.setGenericUse();
        //Start hand calibration
        handsTrackingController.enableHandCalibration();
    }

    public void calibrationFinished()
    {
        TextToSpeech.Instance.StartSpeaking("Now the tree will be appeared");
        placer.CreateScene();
        //Set training use for gaze
        gazeCursor.setTrainingMode();
        // Enable manipulation with hands
        handsTrackingController.enableHandManipulation();
        // Enable data collection
        handsTrackingController.enableDataCollection();
        // Enable Timer
            // enableTimer();
        trainingMode = true;
    }

    public void finishGame()
    {
        TextToSpeech.Instance.StartSpeaking("Training finished");
        trainingMode = false;
        ObjectCollectionManager.Instance.ClearScene();
        UtilitiesScript.Instance.enableObject(currentMenu);
        if (resultsScreen == null) //Create Results menu
            resultsScreen = Instantiate(resultsPrefab, currentMenu.transform.position, currentMenu.transform.rotation);
        else
             UtilitiesScript.Instance.enableObject(resultsScreen);
        UtilitiesScript.Instance.disableObject(currentMenu);
        currentMenu = resultsScreen;
        TextMesh suc =  currentMenu.transform.Find("Successes").gameObject.GetComponent<TextMesh>();
        TextMesh failures = currentMenu.transform.Find("Failures").gameObject.GetComponent<TextMesh>();
        suc.text = "Succeses : " + success;
        failures.text = "Failures: " + fail;
        inMenu = 3;
        //Reset
        gazeCursor.setGenericUse();
        trainingMode = false;
        success = 0;
        fail = 0;
        rightController = null;
        leftController = null;
    }

    public bool addCalibrationController(CalibrationController controller)
    {
        if (controller.isRightHand())
            rightController = controller;
        else
            leftController = controller;

        if (rightController != null && leftController != null)
            return true;
        else
            return false;
    }

    public float getHeadDistanceUpperLimit(bool hand)
    {
        if (hand)
        {
            if (rightController.getHighestPoseHeadHandDistance() > rightController.getRightPoseHeadHandDistance())
            {
                return rightController.getHighestPoseHeadHandDistance();
            }
            else
                return rightController.getRightPoseHeadHandDistance();
        }
        else
        {
            if (leftController.getHighestPoseHeadHandDistance() > leftController.getRightPoseHeadHandDistance())
            {
                return leftController.getHighestPoseHeadHandDistance();
            }
            else
                return leftController.getRightPoseHeadHandDistance();
        }
    }

    public float getHeadDisatnceLowerLimit(bool hand)
    {
        if (hand)
        {
            if (rightController.getHighestPoseHeadHandDistance() > rightController.getRightPoseHeadHandDistance())
            {
                return rightController.getRightPoseHeadHandDistance();
            }
            else
                return rightController.getHighestPoseHeadHandDistance();
        }
        else
        {
            if (leftController.getHighestPoseHeadHandDistance() > leftController.getRightPoseHeadHandDistance())
            {
                return leftController.getRightPoseHeadHandDistance();
            }
            else
                return leftController.getHighestPoseHeadHandDistance();
        }
    }

    public void checkIfAboveBox(Vector3 pos)
    {
        GameObject createdBox = ObjectCollectionManager.Instance.getCreatedBox();
        Rect box = new Rect(createdBox.transform.position.x, createdBox.transform.position.z, createdBox.GetComponent<Renderer>().bounds.size.x / 2, createdBox.GetComponent<Renderer>().bounds.size.z / 2);

        if (box.Contains(new Vector2(pos.x, pos.z)))
            UtilitiesScript.Instance.EnableOutline(handsTrackingController.getManipulatedObject(), Color.white);
        else
            UtilitiesScript.Instance.EnableOutline(handsTrackingController.getManipulatedObject(), Color.red);
    }

    public void printText(String text)
    {
        DebugText.text = text;
    }
}
