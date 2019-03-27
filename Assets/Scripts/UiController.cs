using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UiController : MonoBehaviour
{
    // Back Button
    private string en_BackButton = "Back";
    private string el_BackButton = "Πίσω";
    //About object
    private string en_AboutText = "About text";
    private string el_AboutText = "Περί text";
    //Ask for turtorial object
    private string en_AskText = "Do you want to enter to turtorial again ?";
    private string en_AskSkipButton = "Procceed" + "\n" + "to game";
    private string en_AskEnableButton = "Enable" + "\n" + "Tutorial";
    private string en_AskExitButton = "Exit";
    private string el_AskText = "Θέλετε να εκκινήσετε την επίδειξη;";
    private string el_AskSkipButton = "Προχωρήστε" + "\n" + "στο παιχνίδι";
    private string el_AskEnableButton = "Εκκίνση" + "\n" + "επίδειξης";
    private string el_AskExitButton = "Έξοδος";
    // Play object
    private string en_PlayText = "Game in progress";
    private string en_PlayExitButton = "End" + "\n" + "game";
    private string el_PlayText = "Παιχνίδι σε εξέλιξη";
    private string el_PlayExitButton = "Λήξη" + "\n" + "παιχνιδιού";
    // Results object
    private string en_ResultsSuccessText = "Successes";
    private string en_ResultsFailuresText = "Failures";
    private string en_ResultsRightImpvText = "Right hand improved by";
    private string en_ResultsLeftImpvText = "Left hand improved by";
    private string el_ResultsSuccessText = "Επιτυχίες";
    private string el_ResultsFailuresText = "Αποτυχίες";
    private string el_ResultsRightImpvText = "Δεξί χέρι βελτιώθηκε κατά";
    private string el_ResultsLeftImpvText = "Αριστερό χέρι βελτιώθηκε κατά";
    // Menu object
    private string en_MenuStartButton = "Start";
    private string en_MenuSettingsButton = "Settings";
    private string en_MenuAboutButton = "About";
    private string en_MenuLangButton = "Change language to Greek";
    private string el_MenuStartButton = "Έναρξη";
    private string el_MenuSettingsButton = "Ρυθμίσεις";
    private string el_MenuAboutButton = "Περί";
    private string el_MenuLangButton = "Αλλαγή γλώσσας σε Αγγλικά";
    // Settings object
    private string en_SettingsAudioButton = "Audio Feedback";
    private string en_SettingsClickerButton = "Clicker Enabled";
    private string el_SettingsAudioButton = "Φωνητικές οδηγίες";
    private string el_SettingsClickerButton = "Χρήση Clicker";
    // Start object
    private string en_StartPlayButton = "Play";
    private string en_StartRightButton = "Right Hand";
    private string en_StartLeftButton = "Left Hand";
    private string el_StartPlayButton = "Παίξτε";
    private string el_StartRightButton = "Δεξί χέρι";
    private string el_StartLeftButton = "Αριστερό χέρι";
    // Τurtorial object
    private string en_TurtorialSkipButton = "Procceed to" + "\n" + "game";
    private string en_TurtorialEnableGateButton = "Enable" + "\n" + "gate";
    private string en_TurtorialDisableGateButton = "Disable" + "\n" + "gate";
    private string en_TurtorialExitButton = "Exit";
    private string el_TurtorialSkipButton = "Προχωρήστε" + "\n" + "στο παιχνίδι";
    private string el_TurtorialEnableGateButton = "Ενερ/στε" + "\n" + "την πύλη";
    private string el_TurtorialDisableGateButton = "Απεν/στε" + "\n" + "την πύλη";
    private string el_TurtorialExitButton = "Έξοδος";
    // Texts
    private string en_InitiateCalibration = "Place your hand in right angle pose for 2 seconds ";
    private string el_InitiateCalibration = "Τοποθεστείτε το χέρι σας, τεντομένο σε ορθή γωνία για 2 δευτερόλεπτα";
    // Audio Clips
    public AudioClip mainMenuClip;
    public AudioClip SpatialStartClip;
    public AudioClip SpatialFinshClip;
    public AudioClip CalibrationInitClip;
    public AudioClip GateClip;
    public AudioClip rightManClip;
    public AudioClip leftManClip;
    public AudioClip rightAlreadyCalibClip;
    public AudioClip leftAlreadyCalibClip;
    public AudioClip highPoseClip;
    public AudioClip doRightCalibClip;
    public AudioClip doLeftCalibClip;
    public AudioClip allCalibFinishedClip;
    public AudioClip finishAudioClip;
    public AudioClip TutorialBeginClip;
    public AudioClip askTutorialClip;
    //public AudioClip
    //public AudioClip
    //public AudioClip

    //Public Variables-For Editor
    public GameObject menuPrefab;
    public GameObject settingsPrefab;
    public GameObject startPrefab;
    public GameObject resultsPrefab;
    public GameObject aboutPrefab;
    public GameObject askForTurtorialPrefab;
    public GameObject playPrefab;
    public GameObject TurtorialPrefab;
    public GameObject RedPointPrefab;
    //
    public AudioSource audioSource;
    public ObjectPlacer placer;
    public GazeCursor gazeCursor;
    public TextMesh UserText;
    public HandsTrackingController handsTrackingController;
    public FlowController flowController;
    public TurtorialController turtorialController;
    public float menuDistance;

    // Settings
    public bool audioFeedbackEnabled = false;
    public bool clickerEnabled = false;
    public bool firstTurtorial = false;
    public bool greekEnabled = true;
    private string appPath;

    // Menu
    private GameObject menuScreen;
    private GameObject settingsScreen;
    private GameObject startScreen;
    private GameObject resultsScreen;
    private GameObject aboutScreen;
    private GameObject askTurtorialScreen;
    private GameObject turtorialScreen;
    private GameObject playScreen;
    private int inMenu = -1; // Menu index
    private GameObject currentMenu;
    private Vector3 menuPosition = Vector3.zero;
    private Quaternion menuRotation = new Quaternion();
    private List<GameObject> guidanceList = new List<GameObject>();
    private GameObject child;
    /*
     * Menu Index
     * -1 : Play Scene
     * +0 : Main Menu
     * +1 : Settings Menu
     * +2 : Start Menu
     * +3 : Results Menu
     * +4 : About Menu
     * +5 : Ask Turtorial Menu
     * +6 : Turtorial Scene
     */

    private void Start()
    {
        //Load Settings
        appPath = Application.persistentDataPath;
        if (File.Exists(Path.Combine(appPath, "settings.txt")))
        {
            string[] newSettings = File.ReadAllLines(Path.Combine(appPath, "settings.txt"));
            foreach (string setting in newSettings)
            {
                string[] parsed = setting.Split(':');
                if (parsed[0] == "firstTurtorial")
                {
                    if ((int)char.GetNumericValue(parsed[1].ToCharArray()[0]) > 0)
                        firstTurtorial = true;
                }
                else if (parsed[0] == "audioFeedbackEnabled")
                {
                    if ((int)char.GetNumericValue(parsed[1].ToCharArray()[0]) > 0)
                        audioFeedbackEnabled = true;
                }
                else if (parsed[0] == "clickerEnabled")
                {
                    if ((int)char.GetNumericValue(parsed[1].ToCharArray()[0]) > 0)
                        clickerEnabled = true;
                }
                else if (parsed[0] == "greekEnabled")
                {
                    if ((int)char.GetNumericValue(parsed[1].ToCharArray()[0]) > 0)
                        greekEnabled = true;
                }
            }
        }
        else
        {
            // Create settings File
            string settings = "firstTurtorial:1\n" +
                              "audioFeedbackEnabled:1\n" +
                              "clickerEnabled:0" +
                              "greekEnabled:0";
            File.WriteAllBytes(Path.Combine(appPath, "settings.txt"), 
                    System.Text.Encoding.UTF8.GetBytes(settings));
        }
    }

    private void TapUiReceived()
    {
        //For UI navigation
        GameObject tappedObj = gazeCursor.GetFocusedObject();
        if (tappedObj == null)
            return;

        if (tappedObj.CompareTag("UI"))
        {
            if (inMenu == -1)
            {
                if (tappedObj.name == "ExitButton")
                {
                    flowController.FinishGame();
                }
            }
            else if (inMenu == 0) //Main Menu
            {
                if (tappedObj.name == "StartButton")
                {
                    MoveToStartScreen();
                }
                else if (tappedObj.name == "SettingsButton")
                {
                    MoveToSettingsScreen();
                }
                else if (tappedObj.name == "AboutButton")
                {
                    MoveToAboutScreen();
                }
                else if (tappedObj.name == "LangButton")
                {
                    Debug.Log ("LangButton pressed");
                    greekEnabled = !greekEnabled;
                    for (int i = 0; i < menuScreen.transform.childCount; i++)
                    {
                        child = menuScreen.transform.GetChild(i).gameObject;
                        if (child.name == "AboutButton")
                            child.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_MenuAboutButton : en_MenuAboutButton;
                        else if (child.name == "SettingsButton")
                            child.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_MenuSettingsButton : en_MenuSettingsButton;
                        else if (child.name == "StartButton")
                            child.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_MenuStartButton : en_MenuStartButton;
                        else if (child.name == "LangButton")
                            child.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_MenuLangButton : en_MenuLangButton;
                    }
                }
            }

            else if (inMenu == 1) //Setting Menu
            {
                if (tappedObj.name == "AudioFeedbackButton")
                {
                    audioFeedbackEnabled = (!audioFeedbackEnabled);
                    string status = audioFeedbackEnabled ? "On" : "Off";
                    string newText = greekEnabled ? el_SettingsAudioButton : en_SettingsAudioButton;
                    tappedObj.GetComponentInChildren<TextMesh>().text = newText + ":\n" + status;
                }
                else if (tappedObj.name == "ClickerButton")
                {
                    clickerEnabled = !(clickerEnabled);
                    string status = clickerEnabled ? "On" : "Off";
                    string newText = greekEnabled ? el_SettingsClickerButton : en_SettingsClickerButton;
                    tappedObj.GetComponentInChildren<TextMesh>().text = newText + ":\n" + status;
                }
                else if (tappedObj.name == "BackButton")
                {
                    ReturnToStartMenu();
                }
            }

            else if (inMenu == 2) // Start Menu
            {
                if (tappedObj.name == "PlayButton")
                {
                    //Prepare UI
                    UtilitiesScript.Instance.DisableObject(currentMenu);
                    //Ask user for turtorial
                    StartTurtorial(false);
                }
                else if (tappedObj.name == "RightHandButton")
                {
                    flowController.rightHandEnabled = (!flowController.rightHandEnabled);
                    string status = flowController.rightHandEnabled ? "On" : "Off";
                    string newText = greekEnabled ? el_StartRightButton : en_StartRightButton;
                    tappedObj.GetComponentInChildren<TextMesh>().text = newText + ":\n" + status;
                }
                else if (tappedObj.name == "LeftHandButton")
                {
                    flowController.leftHandEnabled = (!flowController.leftHandEnabled);
                    string status = flowController.leftHandEnabled ? "On" : "Off";
                    string newText = greekEnabled ? el_StartLeftButton : en_StartLeftButton;
                    tappedObj.GetComponentInChildren<TextMesh>().text = newText + ":\n" + status;
                }
                else if (tappedObj.name == "BackButton")
                {
                    ReturnToStartMenu();
                }
            }

            else if (inMenu == 3) // Results Menu
            {
                if (tappedObj.name == "BackButton")
                {
                    ReturnToStartMenu();
                }
            }

            else if (inMenu == 4) // About Menu
            {
                if (tappedObj.name == "BackButton")
                {
                    ReturnToStartMenu();
                }
            }

            else if (inMenu == 5) //  Ask Turtorial Menu
            {
                if (tappedObj.name == "SkipButton")
                {
                    UtilitiesScript.Instance.DisableObject(currentMenu);
                    InititateCalbration();
                }
                else if (tappedObj.name == "EnableButton")
                {
                    StartTurtorial(true);
                }
                else if (tappedObj.name == "ExitButton")
                {
                    ReturnToStartMenu();
                }
                
            }

            else if (inMenu == 6) //  Turtorial Scene
            {
                if (tappedObj.name == "SkipButton")
                {
                    ExitTurtorial();
                    InititateCalbration();
                }
                else if (tappedObj.name == "GateButton")
                {
                    if (turtorialController.turtorialStep == 1)
                    {
                        turtorialController.PrepareSecondTurtorial();
                        tappedObj.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_TurtorialDisableGateButton : en_TurtorialDisableGateButton;
                    }
                    else if (turtorialController.turtorialStep == 2)
                    {
                        turtorialController.PrepareFirstTurtorial();
                        tappedObj.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_TurtorialEnableGateButton : en_TurtorialEnableGateButton;
                    }
                }
                else if (tappedObj.name == "ExitButton")
                {
                    ExitTurtorial();
                    ReturnToStartMenu();
                }
                else if (tappedObj.name == "Video")
                {
                    UnityEngine.Video.VideoPlayer videoPlayer = tappedObj.GetComponent<UnityEngine.Video.VideoPlayer>();
                    if (videoPlayer == null)
                        return;

                    if (videoPlayer.isPlaying)
                        videoPlayer.Stop();
                    else
                        videoPlayer.Play();
                }
            }
        }
    }

    public void ExitTurtorial()
    {
        TextToSpeech.Instance.StopSpeaking();
        flowController.DisableTurtorialMode();
        handsTrackingController.DisableHandManipulation();
        handsTrackingController.SetTurtorialMode(false);
        turtorialController.FinishTurtorial();
    }

    private void StartTurtorial(bool foreceEnable)
    {
        if (foreceEnable || firstTurtorial)
        {
            if (firstTurtorial)
            {
                firstTurtorial = false;
                StoreSettings();
            }

            flowController.EnableTurtorialMode();
            turtorialController.PrepareFirstTurtorial();
            // Enable manipulation with hands
            handsTrackingController.EnableHandManipulation();
            handsTrackingController.DisableDataCollection();
            handsTrackingController.SetTurtorialMode(true);
            // Appear Menu
            if (turtorialScreen != null) //Create Play menu
                UtilitiesScript.Instance.EnableObject(turtorialScreen);
            else
            {
                turtorialScreen = Instantiate(TurtorialPrefab);
                turtorialScreen.transform.position = currentMenu.transform.position;
                turtorialScreen.transform.rotation = currentMenu.transform.rotation;
            }

            for (int i = 0; i < turtorialScreen.transform.childCount; i++)
            {
                child = turtorialScreen.transform.GetChild(i).gameObject;
                if (child.name =="SkipButton")
                    child.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_TurtorialSkipButton : en_TurtorialSkipButton;
                else if (child.name == "GateButton")
                    child.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_TurtorialEnableGateButton : en_TurtorialEnableGateButton;
                else if (child.name == "ExitButton")
                    child.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_TurtorialExitButton : en_TurtorialExitButton;
            }

            UtilitiesScript.Instance.DisableObject(currentMenu);
            currentMenu = null;
            inMenu = 6;

            if (greekEnabled)
            {
                audioSource.Stop();
                audioSource.clip = TutorialBeginClip;
                audioSource.Play();
            }
            else
            {
                TextToSpeech.Instance.StopSpeaking();
                TextToSpeech.Instance.StartSpeaking("Turtorial Mode has been loaded. In the wall there is a menu" +
                    "where you can see three buttons. One for skippping for the turtorial, another one for enabling or disabling gate turtorial" +
                    "and the last one to return to start menu. Above buttons there is a video turtorial so you can see how hand recognision" +
                    "and object manipulation works. Click on it to play or stop this video.");
            }
        }
        else
        {
            //Ask user for turtorial -- Create this menu
            if (askTurtorialScreen == null)
                askTurtorialScreen = Instantiate(askForTurtorialPrefab, currentMenu.transform.position, currentMenu.transform.rotation);
            else
            {
                UtilitiesScript.Instance.EnableObject(askTurtorialScreen);
                askTurtorialScreen.transform.position = currentMenu.transform.position;
            }

            for (int i = 0; i < askTurtorialScreen.transform.childCount; i++)
            {
                child = askTurtorialScreen.transform.GetChild(i).gameObject;
                if (child.name == "AskText")
                    child.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_AskText : en_AskText;
                else if (child.name == "SkipButton")
                    child.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_AskSkipButton : en_AskSkipButton;
                else if (child.name == "EnableButton")
                    child.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_AskEnableButton : en_AskEnableButton;
                else if (child.name == "ExitButton")
                    child.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_AskExitButton : en_AskExitButton;
            }
            if (greekEnabled)
            {
                audioSource.Stop();
                audioSource.clip = askTutorialClip;
                audioSource.Play();
            }
            else
            {
                TextToSpeech.Instance.StopSpeaking();
                TextToSpeech.Instance.StartSpeaking("Do you want to enter tutorial");
            }

            UtilitiesScript.Instance.DisableObject(currentMenu);
            currentMenu = askTurtorialScreen;
            inMenu = 5;
            if (menuPosition != Vector3.zero && menuRotation != new Quaternion())
            {
                currentMenu.transform.position = menuPosition;
                currentMenu.transform.rotation = menuRotation;
            }
        }
    }

    private void InititateCalbration()
    {
        UtilitiesScript.Instance.DisableObject(turtorialScreen);
        UserText.text = greekEnabled ? el_InitiateCalibration : en_InitiateCalibration;
        // Prepare Logic
        flowController.StartCalibration();
        inMenu = -1;
        if (!audioFeedbackEnabled)
            return;
        if (greekEnabled)
        {
            audioSource.Stop();
            audioSource.clip = CalibrationInitClip;
            audioSource.Play();
        }
        else
        {
            TextToSpeech.Instance.StopSpeaking();
            TextToSpeech.Instance.StartSpeaking(en_InitiateCalibration);
        }
    }

    private void MoveToAboutScreen()
    {
        if (aboutScreen == null) //Create Play menu
            aboutScreen = Instantiate(aboutPrefab, currentMenu.transform.position, currentMenu.transform.rotation);
        else
            UtilitiesScript.Instance.EnableObject(aboutScreen);
        UtilitiesScript.Instance.DisableObject(currentMenu);

        for (int i = 0; i < aboutScreen.transform.childCount; i++)
        {
            child = aboutScreen.transform.GetChild(i).gameObject;
            if (child.name == "AboutText")
                child.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_AboutText : en_AboutText;
            else if (child.name == "BackButton")
                child.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_BackButton : en_BackButton;
        }
        currentMenu = aboutScreen;
        inMenu = 4;

        currentMenu.transform.position = menuPosition;
        currentMenu.transform.rotation = menuRotation;
    }

    public void MoveToPlayScreen()
    {
        if (playScreen == null) //Create Play menu
            playScreen = Instantiate(playPrefab);
        else
            UtilitiesScript.Instance.EnableObject(playScreen);

        for (int i = 0; i < playScreen.transform.childCount; i++)
        {
            child = playScreen.transform.GetChild(i).gameObject;
            if (child.name == "PlayText")
                child.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_PlayText : en_PlayText;
            else if (child.name == "ExitButton")
                child.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_PlayExitButton : en_PlayExitButton;
        }
        UtilitiesScript.Instance.DisableObject(currentMenu);
        currentMenu = playScreen;
        inMenu = -1;
        currentMenu.transform.position = menuPosition;
        currentMenu.transform.rotation = menuRotation;
    }

    public void MoveToResultsScreen()
    {
        //Reset UI
        ObjectCollectionManager.Instance.ClearScene();
        UtilitiesScript.Instance.EnableObject(currentMenu);
        if (resultsScreen == null) //Create Results menu
            resultsScreen = Instantiate(resultsPrefab, currentMenu.transform.position, currentMenu.transform.rotation);
        else
            UtilitiesScript.Instance.EnableObject(resultsScreen);

        UtilitiesScript.Instance.DisableObject(currentMenu);
        currentMenu = resultsScreen;
        inMenu = 3;
        currentMenu.transform.position = menuPosition;
        currentMenu.transform.rotation = menuRotation;

        for (int i = 0; i < resultsScreen.transform.childCount; i++)
        {
            child = resultsScreen.transform.GetChild(i).gameObject;
            if (child.name == "BackButton")
                child.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_BackButton : en_BackButton;
            else if (child.name == "Successes")
                child.GetComponentInChildren<TextMesh>().text = (greekEnabled ? el_ResultsSuccessText : en_ResultsSuccessText) + " : " + flowController.success;
            else if (child.name == "Failures")
                child.GetComponentInChildren<TextMesh>().text = (greekEnabled ? el_ResultsFailuresText : en_ResultsFailuresText) + " : " + flowController.fail;
            else if (child.name == "Right_Impv")
            {
                if (!flowController.rightHandEnabled)
                    child.GetComponentInChildren<TextMesh>().text = "Not used";
                else
                {
                    float impv_per = ((flowController.maxHeightRightHand / flowController.GetRightCalibrationController().GetHighestPoseHandHeight()) - 1) * 100f;

                    if (impv_per < 0 || flowController.maxHeightRightHand == 0)
                        impv_per = 0;

                    child.GetComponentInChildren<TextMesh>().text = (greekEnabled ? el_ResultsRightImpvText : en_ResultsRightImpvText) + " " + impv_per + " % ";
                }
            }
            else if (child.name == "Left_Impv")
            {
                if (!flowController.leftHandEnabled)
                    child.GetComponentInChildren<TextMesh>().text = "Not used";
                else
                {
                    float impv_per = ((flowController.maxHeightLeftHand/ flowController.GetLeftCalibrationController().GetHighestPoseHandHeight()) - 1) * 100f;

                    if (impv_per < 0 || flowController.maxHeightLeftHand == 0)
                        impv_per = 0;

                    child.GetComponentInChildren<TextMesh>().text = (greekEnabled ? el_ResultsLeftImpvText : en_ResultsLeftImpvText) + " " + impv_per + " % ";
                }
            }
        }
    }

    private void MoveToStartScreen()
    {
        if (startScreen == null) //Create Play menu
            startScreen = Instantiate(startPrefab, currentMenu.transform.position, currentMenu.transform.rotation);
        else
            UtilitiesScript.Instance.EnableObject(startScreen);

        UtilitiesScript.Instance.DisableObject(currentMenu);
        currentMenu = startScreen;
        inMenu = 2;

        for (int i = 0; i < startScreen.transform.childCount; i++)
        {
            child = startScreen.transform.GetChild(i).gameObject;
            if (child.name == "PlayButton")
                child.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_StartPlayButton : en_StartPlayButton;
            else if (child.name == "RightHandButton")
            {
                string status = flowController.rightHandEnabled ? "On" : "Off";
                string newText = greekEnabled ? el_StartRightButton : en_StartRightButton;
                child.GetComponentInChildren<TextMesh>().text = newText + ":\n" + status;
            }
            else if (child.name == "LeftHandButton")
            {
                string status = flowController.leftHandEnabled ? "On" : "Off";
                string newText = greekEnabled ? el_StartLeftButton : en_StartLeftButton;
                child.GetComponentInChildren<TextMesh>().text = newText + ":\n" + status;
            }
            else if (child.name == "BackButton")
                child.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_BackButton : en_BackButton;
        }
        if (menuPosition != Vector3.zero && menuRotation != new Quaternion())
        {
            currentMenu.transform.position = menuPosition;
            currentMenu.transform.rotation = menuRotation;
        }
    }

    private void MoveToSettingsScreen()
    {
        if (settingsScreen == null) //Create Settings menu
            settingsScreen = Instantiate(settingsPrefab, currentMenu.transform.position, currentMenu.transform.rotation);
        else
            UtilitiesScript.Instance.EnableObject(settingsScreen);

        UtilitiesScript.Instance.DisableObject(currentMenu);
        currentMenu = settingsScreen;
        inMenu = 1;

        for (int i = 0; i < settingsScreen.transform.childCount; i++)
        {
            child = settingsScreen.transform.GetChild(i).gameObject;
            if (child.name == "BackButton")
                child.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_BackButton : en_BackButton;
            else if (child.name == "AudioFeedbackButton")
            {
                string status = audioFeedbackEnabled? "On" : "Off";
                string newText = greekEnabled ? el_SettingsAudioButton : en_SettingsAudioButton;
                child.GetComponentInChildren<TextMesh>().text = newText + ":\n" + status;
            }
            else if (child.name == "ClickerButton")
            {
                string status = clickerEnabled ? "On" : "Off";
                string newText = greekEnabled ? el_SettingsClickerButton : en_SettingsClickerButton;
                child.GetComponentInChildren<TextMesh>().text = newText + ":\n" + status;
            }
        }
        currentMenu.transform.position = menuPosition;
        currentMenu.transform.rotation = menuRotation;
    }

    private void StoreSettings()
    {
        firstTurtorial = false;
        // Update settings File
        int ft_value = firstTurtorial ? 1 : 0;
        int af_value = audioFeedbackEnabled ? 1 : 0;
        int cl_value = clickerEnabled ? 1 : 0;
        int gr_value = greekEnabled ? 1 : 0;
        string settings = "firstTurtorial:"+ ft_value + "\n" +
                          "audioFeedbackEnabled:"+ af_value + "\n" +
                          "clickerEnabled:" + cl_value +
                          "greekEnabled:" + gr_value;
        File.WriteAllBytes(Path.Combine(appPath, "settings.txt"),
                System.Text.Encoding.UTF8.GetBytes(settings));
    }

    public void ReturnToStartMenu()
    {
        if (currentMenu != null)
            UtilitiesScript.Instance.DisableObject(currentMenu);

        currentMenu = menuScreen;
        inMenu = 0;
        UtilitiesScript.Instance.EnableObject(currentMenu);
        currentMenu.transform.position = menuPosition;
        currentMenu.transform.rotation = menuRotation;
    }

    public void CreateUI()
    {
        EventManager.StartListening("tap", TapUiReceived);
        placer.CreateMenu();
        inMenu = 0;
    }

    public void PrintText(string text)
    {
        UserText.text = text;
    }

    public void SetMenu(GameObject obj)
    {
        menuPosition = obj.transform.position;
        menuRotation = obj.transform.rotation;
        menuScreen = obj;
        currentMenu = obj;
        inMenu = 0;

        for (int i = 0; i < menuScreen.transform.childCount; i++)
        {
            child = menuScreen.transform.GetChild(i).gameObject;
            if (child.name == "AboutButton")
                child.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_MenuAboutButton : en_MenuAboutButton;
            else if (child.name == "SettingsButton")
                child.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_MenuSettingsButton : en_MenuSettingsButton;
            else if (child.name == "StartButton")
                child.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_MenuStartButton : en_MenuStartButton;
            else if (child.name == "LangButton")
                child.GetComponentInChildren<TextMesh>().text = greekEnabled ? el_MenuLangButton : en_MenuLangButton;
        }

        // Create Guidance for user
        Vector3 center = Vector3.Lerp(Camera.main.transform.position, menuPosition, 0.5f);
        Vector3 useAngles = Camera.main.transform.eulerAngles;
        Vector3 dx = Camera.main.transform.position - menuPosition;
        float radius = new Vector2(dx.x, dx.z).magnitude / 2;
        Vector3 position = new Vector3(0, center.y, 0);
        //
        Vector2 cameraPos = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z);
        Vector2 menuPos = new Vector2(menuPosition.x, menuPosition.z);
        Vector2 customPos = new Vector2(cameraPos.x, menuPos.y);
        float d1_mag = (customPos - cameraPos).magnitude;
        float d2_mag = (menuPos - cameraPos).magnitude;
        float d_angle = Mathf.Acos(d1_mag / d2_mag) * Mathf.Rad2Deg;

        if (UtilitiesScript.Instance.IsRightFromHead(obj.transform.position))
        {
            for (float angle = 0f; angle < d_angle; angle += 8)
            {
                position.x = Camera.main.transform.position.x + Mathf.Sin((useAngles.y + angle) * Mathf.Deg2Rad) * radius;
                position.z = Camera.main.transform.position.z + Mathf.Cos((useAngles.y + angle) * Mathf.Deg2Rad) * radius;
                guidanceList.Add(Instantiate(RedPointPrefab, position, Quaternion.LookRotation(Vector3.up)));
            }
        }
        else
        {
            for (float angle = -d_angle; angle < 1f; angle += 9)
            {
                position.x = Camera.main.transform.position.x + Mathf.Sin((useAngles.y + angle) * Mathf.Deg2Rad) * radius;
                position.z = Camera.main.transform.position.z + Mathf.Cos((useAngles.y + angle) * Mathf.Deg2Rad) * radius;
                guidanceList.Add(Instantiate(RedPointPrefab, position, Quaternion.LookRotation(Vector3.up)));
            }
        }
        EventManager.StartListening("gaze_ui", DestroyGuidance);
        if (!audioFeedbackEnabled)
            return;
        if (greekEnabled)
        {
            audioSource.Stop();
            audioSource.clip = mainMenuClip;
            audioSource.Play();
        }
        else
        {
            TextToSpeech.Instance.StopSpeaking();
            TextToSpeech.Instance.StartSpeaking("Follow the green points to see the menu");
        }
    }

    private void DestroyGuidance()
    {
        EventManager.StopListening("gaze_ui", DestroyGuidance);
        if (greekEnabled)
            audioSource.Stop();
        else
            TextToSpeech.Instance.StopSpeaking();

        foreach (GameObject point in guidanceList)
            Destroy(point);
        guidanceList.Clear();
    }
}
