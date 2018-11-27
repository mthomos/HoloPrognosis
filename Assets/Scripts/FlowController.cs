using HoloToolkit.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

public class FlowController : MonoBehaviour
{
    //Public Variables-For Editor
    public ObjectPlacer placer;
    public GazeCursor gazeCursor;
    public TextMesh DebugText;
    public HandsTrackingController handsTrackingController;
    public TextToSpeech textToSpeechManager;
    public GameObject menuPrefab;
    public GameObject settingsPrefab;
    public GameObject playPrefab;
    public float menuDistance;
    public float waitTimeMinutes;
    public float waitHandTime;
    // Settings
    private List<int> settings;
    private bool audioFeedbackEnabled = false;
    private bool graphicalFeedbackEnabled = false;
    private bool clickerEnabled = false;
    // Training
    private bool trainingMode = false;
    private float userHeight;
    private float handHeight;
    private float handDistance;
    private List<CalibrationController> calibrationControllers = new List<CalibrationController>();
    //Create timer variables
    private float timer;
    private bool timerEnabled = false;
    //
    private bool handAboveBoxInProgress;
    private float manipulationTimer;
    //
    private GameObject menuScreen;
    private GameObject settingsScreen;
    private GameObject playScreen;
    private GameObject currentMenu;
    private int inMenu = -1;
    //
    private bool rightHandEnabled = true;
    private bool leftHandEnabled = false;

    //Private methods

	private void Start ()
    {
        //Load Settings
        settings = SaveLoad.Instance.LoadSettings();
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
        GameObject tappedObj = handsTrackingController.getTappedObject();
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
                        tappedObj.GetComponent<TextMesh>().text = "Audio Feedback:" + "\n" + "On";
                    else
                        tappedObj.GetComponent<TextMesh>().text = "Audio Feedback:" + "\n" + "Off";
                }
                else if (tappedObj.name == "ClickerButton")
                {
                    clickerEnabled = !(clickerEnabled);
                    if (audioFeedbackEnabled)
                        tappedObj.GetComponent<TextMesh>().text = "Clicker Enabled:" + "\n" + "On";
                    else
                        tappedObj.GetComponent<TextMesh>().text = "Clicker Enabled:" + "\n" + "Off";
                }
                else if (tappedObj.name == "AboutButton")
                {
                    returnToStartMenu();
                }
            }
            else if (inMenu == 2) // Play Menu
            {
                if (tappedObj.name == "StartButton")
                {
                    startPlaying();
                }
                else if (tappedObj.name == "RightHandButton")
                {
                    rightHandEnabled = (!rightHandEnabled);
                    if (rightHandEnabled)
                        tappedObj.GetComponent<TextMesh>().text = "Right Hand:" + "\n" + "On";
                    else
                        tappedObj.GetComponent<TextMesh>().text = "Right Hand:" + "\n" + "Off";
                }
                else if (tappedObj.name == "LeftHandButton")
                {
                    leftHandEnabled = (!leftHandEnabled);
                    if (rightHandEnabled)
                        tappedObj.GetComponent<TextMesh>().text = "Left Hand:" + "\n" + "On";
                    else
                        tappedObj.GetComponent<TextMesh>().text = "Left Hand:" + "\n" + "Off";
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
        currentMenu.SetActive(false);
        if (playScreen == null)
        {
            //Create Play menu
            Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward * 1.5f;
            Quaternion rot = Quaternion.LookRotation(pos - Camera.main.transform.position);
            playScreen = Instantiate(playPrefab, pos, rot);
        }
        currentMenu = playScreen;
        menuScreen.SetActive(true);
        inMenu = 0;
    }

    private void moveToSettingsScreen()
    {
        currentMenu.SetActive(false);
        if (settingsScreen == null)
        {
            //Create Settings menu
            Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward * 1.5f;
            Quaternion rot = Quaternion.LookRotation(pos - Camera.main.transform.position);
            settingsScreen = Instantiate(settingsPrefab, pos, rot);
        }
        currentMenu = settingsScreen;
        menuScreen.SetActive(true);
        inMenu = 1;
    }

    private void returnToStartMenu()
    {
        currentMenu.SetActive(false);
        currentMenu = menuScreen;
        menuScreen.SetActive(true);
        inMenu = 2;
    }

    private void startPlaying()
    {
        currentMenu.SetActive(false);
        EventManager.StopListening("tap", tapUiReceived);
        //Preapare Calibration Scene
        Prepare();
    }

    private void refreshTimer()
    {
        timer += Time.deltaTime;
        if (timer > waitTimeMinutes * 60)
        {
            timer = 0f;
            finishGame();
        }
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

    private void maxValue(List<CalibrationController> list, out float maxD, out float maxH)
    {
        maxD = -1.0f;
        maxH = -1.0f;
        foreach (CalibrationController i in list)
        {
            if (i.getDistance() > maxD)
                maxD = i.getDistance();
            if (i.getHandHeight() > maxH)
                maxH = i.getHandHeight();
        }
    }

    //Public methods

    public void createUI()
    {
        //First listen for taps
        EventManager.StartListening("tap", tapUiReceived);
        DebugText.text = "Menu Created";
        //Appear the menu in front of user
        Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward * menuDistance;
        menuScreen = Instantiate(menuPrefab, pos, Quaternion.LookRotation(pos - Camera.main.transform.position));
        inMenu = 0;
        currentMenu = menuScreen;
    }

    public void Prepare()
    {
        placer.CreateCablirationScene();
        //Create UI point
        GameObject calibrationPoint = GameObject.FindGameObjectWithTag("Calibration");
        calibrationPoint.AddComponent<DirectionIndicator>();
        calibrationPoint.GetComponent<DirectionIndicator>().setCursor(gazeCursor);
        //Set GazeCursor for height calculation
        gazeCursor.setCalculationMode();
        textToSpeechManager.SpeakSsml("Now we should begin the calibration process. First you should at the red calibration point on the floor, " +
            "so we calculate your height. Height is needed by the training program");
    }

    public void finishCalculateMode(float height)
    {
        textToSpeechManager.SpeakSsml("Your height has been calculated succesfully");
        //Store value
        userHeight = height;
        //Destroy calibrationPoint for height calculation
        ObjectCollectionManager.Instance.ClearScene();
        DebugText.text = "Calculation Finished";
        //Set generic use for gaze
        gazeCursor.setGenericUse();
        //Start hand calibration
        handsTrackingController.enableHandCalibration();
    }

    public void calibrationFinished()
    {
        maxValue(calibrationControllers, out handDistance, out handHeight);
        //Clear scene Create play scene
        ObjectCollectionManager.Instance.ClearScene();
        placer.CreateScene();
        //UI/audio instructions
        DebugText.text = "Calibration Finished";
        //Set generic use for gaze
        gazeCursor.setTrainingMode();
        //Enable manipulation with hands
        handsTrackingController.enableHandManipulation();
        handsTrackingController.enableDataCollection();
        //Enable Timer
        enableTimer();
        trainingMode = true;
        //EventManager.StartListening("manipulationStarted", manipulationStarted);
        //EventManager.StartListening("manipulationUpdated", manipulationUpdated);
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

    public int addCalibrationController(CalibrationController controller, float time)
    {
        calibrationControllers.Add(controller);
        return calibrationControllers.Count;
    }

    public float getHeadDistanceLimit()
    {
        return handDistance;
    }

    public float getHandHeight()
    {
        return handHeight;
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
