using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UiController : MonoBehaviour
{
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
    public ObjectPlacer placer;
    public GazeCursor gazeCursor;
    public TextMesh UserText;
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
            else if (inMenu == 0) //Start Menu
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
                        tappedObj.GetComponentInChildren<TextMesh>().text = "Disable" + "\n" + "Gate";
                    }
                    else if (turtorialController.turtorialStep == 2)
                    {
                        turtorialController.PrepareFirstTurtorial();
                        tappedObj.GetComponentInChildren<TextMesh>().text = "Enable" + "\n" + "Gate";
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
                FirstTurtorialDone();

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
            UtilitiesScript.Instance.DisableObject(currentMenu);
            currentMenu = null;
            inMenu = 6;

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
            }
            TextToSpeech.Instance.StopSpeaking();
            TextToSpeech.Instance.StartSpeaking("Do you want to enter turtorial");

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
        UserText.text = "Place your hand in right angle pose for 2 seconds ";
        TextToSpeech.Instance.StopSpeaking();
        TextToSpeech.Instance.StartSpeaking(UserText.text);
        // Prepare Logic
        flowController.StartCalibration();
        inMenu = -1;
    }

    private void MoveToAboutScreen()
    {
        if (aboutScreen == null) //Create Play menu
            aboutScreen = Instantiate(aboutPrefab, currentMenu.transform.position, currentMenu.transform.rotation);
        else
            UtilitiesScript.Instance.EnableObject(aboutScreen);
        UtilitiesScript.Instance.DisableObject(currentMenu);
        currentMenu = aboutScreen;
        inMenu = 4;
        if (menuPosition != Vector3.zero && menuRotation != new Quaternion())
        {
            currentMenu.transform.position = menuPosition;
            currentMenu.transform.rotation = menuRotation;
        }
    }

    public void MoveToPlayScreen()
    {
        if (playScreen == null) //Create Play menu
            playScreen = Instantiate(playPrefab);
        else
            UtilitiesScript.Instance.EnableObject(playScreen);
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
        UtilitiesScript.Instance.PlaceInFrontOfUser(menuScreen, menuDistance);
        if (menuPosition != Vector3.zero && menuRotation != new Quaternion())
        {
            currentMenu.transform.position = menuPosition;
            currentMenu.transform.rotation = menuRotation;
        }

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
        if (menuPosition != Vector3.zero && menuRotation != new Quaternion())
        {
            currentMenu.transform.position = menuPosition;
            currentMenu.transform.rotation = menuRotation;
        }
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
        if (menuPosition != Vector3.zero && menuRotation != new Quaternion())
        {
            
            currentMenu.transform.position = menuPosition;
            currentMenu.transform.rotation = menuRotation;
        }
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
        TextToSpeech.Instance.StopSpeaking();
        TextToSpeech.Instance.StartSpeaking("Follow the green points to see the menu");
        if (currentMenu == null)
        {
            currentMenu = obj;
            inMenu = 0;
        }
    }

    private void DestroyGuidance()
    {
        EventManager.StopListening("gaze_ui", DestroyGuidance);
        TextToSpeech.Instance.StopSpeaking();
        foreach (GameObject point in guidanceList)
            Destroy(point);

        guidanceList.Clear();
    }
}
