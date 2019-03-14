using System.IO;
using UnityEngine;

public class UiController : MonoBehaviour
{
    //Public Variables-For Editor
    public GameObject menuPrefab;
    public GameObject settingsPrefab;
    public GameObject playPrefab;
    public GameObject resultsPrefab;
    public GameObject aboutPrefab;
    public GameObject askForTurtorialPrefab;
    // Turtorial Prefab in CollectionManager due to Spatial
    //
    public ObjectPlacer placer;
    public GazeCursor gazeCursor;
    public TextMesh DebugText;
    public HandsTrackingController handsTrackingController;
    public FlowController flowController;
    public TurtorialController turtorialController;
    public float menuDistance;

    // Settings
    private bool audioFeedbackEnabled = false;
    private bool clickerEnabled = false;
    private bool firstTurtorial = false;
    private string appPath;

    // Menu
    private GameObject menuScreen;
    private GameObject settingsScreen;
    private GameObject playScreen;
    private GameObject resultsScreen;
    private GameObject aboutScreen;
    private GameObject askTurtorialScreen;
    private GameObject turtorialScreen;
    private int inMenu = -1; // Menu index
    private GameObject currentMenu;
    /*
     * Menu Index
     * -1 : Play Scene
     * +0 : Start Menu
     * +1 : Settings Menu
     * +2 : Play Menu
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
            }
        }
        else
        {
            // Create settings File
            string settings = "firstTurtorial:1\n" +
                              "audioFeedbackEnabled:1\n" +
                              "clickerEnabled:0";
            File.WriteAllBytes(Path.Combine(appPath, "settings.txt"), 
                    System.Text.Encoding.UTF8.GetBytes(settings));
        }
    }

    private void TapUiReceived()
    {
        /*
        if (inMenu == -1) // Play scene
        {
            moveToResultsScreen();
            return;
        }
        */
        //For UI navigation
        GameObject tappedObj = gazeCursor.getFocusedObject();
        if (tappedObj == null)
            return;

        if (tappedObj.CompareTag("UI"))
        {
            if (inMenu == 0) //Start Menu
            {
                if (tappedObj.name == "StartButton")
                {
                    MoveToPlayScreen();
                }
                else if (tappedObj.name == "SettingsButton")
                {
                    MoveToSettingsScreen();
                }
                else if (tappedObj.name == "AboutButton")
                {
                    MoveToAboutScreen();
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
                    ReturnToStartMenu();
                }
            }

            else if (inMenu == 2) // Play Menu
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
                    if (flowController.rightHandEnabled)
                        tappedObj.GetComponentInChildren<TextMesh>().text = "Right Hand:" + "\n" + "Yes";
                    else
                        tappedObj.GetComponentInChildren<TextMesh>().text = "Right Hand:" + "\n" + "No";
                }
                else if (tappedObj.name == "LeftHandButton")
                {
                    flowController.leftHandEnabled = (!flowController.leftHandEnabled);
                    if (flowController.rightHandEnabled)
                        tappedObj.GetComponentInChildren<TextMesh>().text = "Left Hand:" + "\n" + "Yes";
                    else
                        tappedObj.GetComponentInChildren<TextMesh>().text = "Left Hand:" + "\n" + "No";
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
                    InititateCalbration();
                }
                else if (tappedObj.name == "EnableButton")
                {
                    StartTurtorial(true);
                }
            }

            else if (inMenu == 6) //  Turtorial Scene
            {
                if (tappedObj.name == "SkipButton")
                {
                    InititateCalbration();
                }
                else if (tappedObj.name == "GateButton")
                {
                    if (turtorialController.turtorialStep == 1)
                    {
                        turtorialController.PrepareSecondTurtorial();
                        tappedObj.GetComponentInChildren<TextMesh>().text = "Disable Gate";
                    }
                    else if (turtorialController.turtorialStep == 2)
                    {
                        turtorialController.PrepareFirstTurtorial();
                        tappedObj.GetComponentInChildren<TextMesh>().text = "Enable Gate";
                    }
                }
                else if (tappedObj.name == "ExitButton")
                {
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

    private void StartTurtorial(bool foreceEnable)
    {
        if (foreceEnable || firstTurtorial)
        {
            if (firstTurtorial)
                FirstTurtorialDone();

            flowController.EnableTurtorialMode();
            turtorialController.PrepareFirstTurtorial();
            // Enable manipulation with hands
            handsTrackingController.EnableHandManipulation();
            // Appear Menu
            UtilitiesScript.Instance.DisableObject(currentMenu);
            placer.AddTurtorialMenu();
            if (turtorialScreen != null) //Create Play menu
                UtilitiesScript.Instance.EnableObject(turtorialScreen);
            currentMenu = null;
            inMenu = 6;
            if (TextToSpeech.Instance.IsSpeaking())
                TextToSpeech.Instance.StopSpeaking();

            TextToSpeech.Instance.StartSpeaking("Turtorial Mode has been loaded. In the wall there is a menu"+
                "where you can see three buttons. One for skippping for the turtorial, another one for enabling or disabling gate turtorial"+
                "and the last one to return to start menu. Above buttons there is a video turtorial so you can see how hand recognision"+
                "and object manipulation works. Click on it to play or stop this video.");
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
                if (TextToSpeech.Instance.IsSpeaking())
                    TextToSpeech.Instance.StopSpeaking();
                TextToSpeech.Instance.StartSpeaking("Do you want to enter turtorial");
            }

            UtilitiesScript.Instance.DisableObject(currentMenu);
            currentMenu = askTurtorialScreen;
            inMenu = 5;
        }
    }

    private void InititateCalbration()
    { 

        flowController.DisableTurtorialMode();
        turtorialController.FinishTurtorial();
        DebugText.text = "Place your hand in right angle pose for 2 seconds ";
        if (TextToSpeech.Instance.IsSpeaking())
            TextToSpeech.Instance.StopSpeaking();
        TextToSpeech.Instance.StartSpeaking(DebugText.text);
        // Prepare Logic
        flowController.StartPlaying();
        inMenu = -1;
    }

    private void MoveToAboutScreen()
    {
        UtilitiesScript.Instance.DisableObject(currentMenu);
        if (aboutScreen == null) //Create Play menu
            aboutScreen = Instantiate(aboutPrefab, currentMenu.transform.position, currentMenu.transform.rotation);
        else
            UtilitiesScript.Instance.EnableObject(aboutScreen);
        currentMenu = aboutScreen;
        inMenu = 4;
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

        TextMesh success = currentMenu.transform.Find("Successes").gameObject.GetComponent<TextMesh>();
        TextMesh failures = currentMenu.transform.Find("Failures").gameObject.GetComponent<TextMesh>();
        TextMesh rightImpv = currentMenu.transform.Find("Right_Impv").gameObject.GetComponent<TextMesh>();
        TextMesh left_Impv = currentMenu.transform.Find("Left_Impv").gameObject.GetComponent<TextMesh>();

        success.text = "Succeses : " + flowController.success;
        failures.text = "Failures: " + (flowController.fail);

        float impv_per = ((flowController.maxHeightRightHand / flowController.GetRightCalibrationController().GetHighestPoseHandHeight()) - 1) * 100f;

        if (impv_per < 0 || flowController.maxHeightRightHand == 0)
            impv_per = 0;

        rightImpv.text = flowController.rightHandEnabled ? "Right Hand improved by " + impv_per + "%" : "";

        impv_per = ((flowController.maxHeightLeftHand / flowController.GetLeftCalibrationController().GetHighestPoseHandHeight()) - 1) * 100f;
        if (impv_per < 0 || flowController.maxHeightLeftHand == 0)
            impv_per = 0;

        left_Impv.text = flowController.leftHandEnabled ? "Left Hand improved by " + impv_per + "%" : "";

        Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward * menuDistance;
        currentMenu.transform.position = pos;
        //Fix menu direction
        Vector3 directionToTarget = Camera.main.transform.position - pos;
        directionToTarget.y = 0.0f;
        if (directionToTarget.sqrMagnitude > 0.005f)
            currentMenu.transform.rotation = Quaternion.LookRotation(-directionToTarget);
    }

    private void MoveToPlayScreen()
    {
        UtilitiesScript.Instance.DisableObject(currentMenu);
        if (playScreen == null) //Create Play menu
            playScreen = Instantiate(playPrefab, currentMenu.transform.position, currentMenu.transform.rotation);
        else
            UtilitiesScript.Instance.EnableObject(playScreen);
        currentMenu = playScreen;
        inMenu = 2;
    }

    private void MoveToSettingsScreen()
    {
        UtilitiesScript.Instance.DisableObject(currentMenu);
        if (settingsScreen == null) //Create Settings menu
            settingsScreen = Instantiate(settingsPrefab, currentMenu.transform.position, currentMenu.transform.rotation);
        else
            UtilitiesScript.Instance.EnableObject(settingsScreen);
        currentMenu = settingsScreen;
        inMenu = 1;
    }

    private void FirstTurtorialDone()
    {
        firstTurtorial = false;
        // Update settings File
        int ft_value = firstTurtorial ? 1 : 0;
        int af_value = audioFeedbackEnabled ? 1 : 0;
        int cl_value = clickerEnabled ? 1 : 0;
        string settings = "firstTurtorial:"+ ft_value + "\n" +
                          "audioFeedbackEnabled:"+ af_value + "\n" +
                          "clickerEnabled:" + cl_value;
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
        if (inMenu == -1)
            UtilitiesScript.Instance.PlaceInFrontOfUser(currentMenu, menuDistance);
    }

    public void CreateUI()
    {
        EventManager.StartListening("tap", TapUiReceived);
        //Appear the menu in front of user
        menuScreen = Instantiate(menuPrefab);
        //Fix menu position
        UtilitiesScript.Instance.PlaceInFrontOfUser(menuScreen, menuDistance);
        currentMenu = menuScreen;
        inMenu = 0;
    }

    public void PrintText(string text)
    {
        DebugText.text = text;
    }

    public void SetTurtorialMenu(GameObject obj)
    {
        turtorialScreen = obj;
        if (currentMenu == null)
            currentMenu = obj;
    }
}
