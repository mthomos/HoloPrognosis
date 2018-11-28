using HoloToolkit.Unity;
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
    //
    public SaveLoad saveLoadScript;
    public ObjectPlacer placer;
    public GazeCursor gazeCursor;
    public TextMesh DebugText;
    public HandsTrackingController handsTrackingController;
    public TextToSpeech textToSpeechManager;
    public float menuDistance;
    // Settings
    private List<int> settings;
    private bool audioFeedbackEnabled = false;
    private bool graphicalFeedbackEnabled = false;
    private bool clickerEnabled = false;
    // Training
    private bool trainingMode = false;
    private float userHeight;
    private CalibrationController rightController, leftController;
    //Create timer variables
    private float timer;
    private bool timerEnabled = false;
    //
    private bool handAboveBoxInProgress;
    private float manipulationTimer;
    // Menu
    private GameObject menuScreen;
    private GameObject settingsScreen;
    private GameObject playScreen;
    private GameObject currentMenu;
    private int inMenu = -1;
    //
    private bool rightHandEnabled = true;
    private bool leftHandEnabled = true;
    //
    public float timerForRightPose = 2.0f;
    public float timerForHighPose = 3.0f;
    public float gameTimeMinutes;
    public float waitHandTime;

    private void Start ()
    {
        //Load Settings
        settings = saveLoadScript.LoadSettings();
        for (int i=0; i< settings.Count; i++)
        {
            if(i==0)
            {
                if (settings[i] > 0)
                    audioFeedbackEnabled = true;
            }
            else if (i == 1)
            {
                if (settings[i] > 0)
                    graphicalFeedbackEnabled = true;
            }
            else if (i == 2)
            {
                if (settings[i] > 0)
                    clickerEnabled = true;
            }
        }
    }

    private void Update ()
    {
        if (timerEnabled && trainingMode)
            refreshTimer();
	}

    private void tapUiReceived()
    {
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
                else if (tappedObj.name == "BackButton")
                {
                    returnToStartMenu();
                }
            }
        }
    }

    private void moveToPlayScreen()
    {
        disableObject(currentMenu);
        if (playScreen == null) //Create Play menu
            playScreen = Instantiate(playPrefab, currentMenu.transform.position, currentMenu.transform.rotation);
        else
            enableObject(playScreen);
        currentMenu = playScreen;
        inMenu = 2;
    }

    private void moveToSettingsScreen()
    {
        disableObject(currentMenu);
        if (settingsScreen == null) //Create Settings menu
            settingsScreen = Instantiate(settingsPrefab, currentMenu.transform.position, currentMenu.transform.rotation);
        else
            enableObject(settingsScreen);
        currentMenu = settingsScreen;
        inMenu = 1;
    }

    private void returnToStartMenu()
    {
        disableObject(currentMenu);
        currentMenu = menuScreen;
        enableObject(currentMenu);
        inMenu = 0;
    }

    private void enableObject(GameObject obj)
    {
        obj.SetActive(true);
    }

    private void disableObject(GameObject obj)
    {
        obj.SetActive(false);
    }

    private void refreshTimer()
    {
        timer += Time.deltaTime;
        if (timer > gameTimeMinutes * 60)
        {
            timer = 0f;
            finishGame();
        }
    }

    public void calibrationMaxPose()
    {
        // Tell user to raise the hand for 2s
        textToSpeechManager.SpeakSsml("Now Raise your hand as high as you can for two seconds"); 
    }
    private void enableTimer()
    {
        timerEnabled = true;
        timer = 0.0f;
    }

    private void disableTimer()
    {
        timerEnabled = false;
    }

    //Public methods
    public void createUI()
    {
        //First listen for taps
        gazeCursor.setGenericUse();
        EventManager.StartListening("tap", tapUiReceived);
        //Appear the menu in front of user
        Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward * menuDistance;
        //Vector3 newPos = placer.create_possible_pos(ObjectCollectionManager.Instance.menuSize, ObjectType.Menu);
        menuScreen = Instantiate(menuPrefab);
        menuScreen.transform.position = pos;
        //menuScreen.AddComponent<MenuScript>();
        inMenu = 0;
        currentMenu = menuScreen;
        placer.HideGridEnableOcclulsion();
    }

    public void startPlaying()
    {
        disableObject(currentMenu);
        EventManager.StopListening("tap", tapUiReceived);
        //Preapare Calibration Scene
        //placer.CreateCablirationScene();
        enableCalibrationMode();
    }

    public void CalculateUserHeightProcess()
    {
        //Create UI point
        GameObject calibrationPoint = GameObject.FindGameObjectWithTag("Calibration");
        calibrationPoint.AddComponent<DirectionIndicator>();
        calibrationPoint.GetComponent<DirectionIndicator>().setCursor(gazeCursor);
        //Set GazeCursor for height calculation
        gazeCursor.setCalculationMode();
        textToSpeechManager.SpeakSsml("Now we should begin the calibration process. First you should at the red calibration point on the floor, " +
            "so we calculate your height. Height is needed by the training program");
    }

    public void enableCalibrationMode()
    {
        //textToSpeechManager.SpeakSsml("Your height has been calculated succesfully");
        //userHeight = height; 
        //Destroy calibrationPoint for height calculation
        //ObjectCollectionManager.Instance.ClearScene();

        DebugText.text = "Place your hand in right angle pose for 2 seconds ";
        //Set generic use for gaze
        gazeCursor.setGenericUse();
        //Start hand calibration
        handsTrackingController.enableHandCalibration();
    }

    public void calibrationFinished()
    {
        placer.CreateScene();
        //UI/audio instructions
        DebugText.text = "Calibration Finished. Play scene is loading";
        //Set training use for gaze
        gazeCursor.setTrainingMode();
        // Enable manipulation with hands
        handsTrackingController.enableHandManipulation();
        // Enable data collection
        //handsTrackingController.enableDataCollection();
        // Enable Timer
        enableTimer();
        trainingMode = true;
    }

    public void finishGame()
    {
        ObjectCollectionManager.Instance.ClearScene();
        //Evaluate Hands
        gazeCursor.setGenericUse();
        //Retrieve Data
        handsTrackingController.enableDataCollection();
        //Process Data

        //Store Data

        //Present Summary of Session
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
        Rect box = new Rect(createdBox.transform.position.x, createdBox.transform.position.z, createdBox.GetComponent<BoxCollider>().size.x / 2, createdBox.GetComponent<BoxCollider>().size.z / 2);
        if (handAboveBoxInProgress)
        {
            if (box.Contains(pos))
            {
                if(Time.time - manipulationTimer > waitHandTime)
                {
                    // Audio - UI feedback
                    handsTrackingController.freeToRelease();
                }
            }
            else
            {
                //Reset Timer and bools
                handAboveBoxInProgress = false;
                handsTrackingController.resetManipulation();
            }
        }
        else
        {
            if (box.Contains(pos))
            {
                handAboveBoxInProgress = true;
                manipulationTimer = Time.time;
            }
        }
    }

    public void userSaidYes()
    {
        throw new NotImplementedException();
    }

    public void userSaidNo()
    {
        throw new NotImplementedException();
    }

    public void userRequestedFinish()
    {
        throw new NotImplementedException();
    }
}
