/* TODO
 *  store value of height in DB
 *  check db if height value exists in DB
 *  otherwise calculateHeight()
 *  store data to DB
 */

using HoloToolkit.Unity;
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
    //
    private bool trainingMode = false;
    private GameObject calibrationPoint;
    private float userHeight;
    private float handHeight;
    private float handDistance;
    private List<CalibrationController> calibrationControllers;
    //Create timer variables
    public float waitTime;
    private float timer;
    private bool timerEnabled = false;
    //
    private bool handAboveBoxInProgress;
    //private bool manipulatedHandAboveBox;
    private float manipulationTimer;
    public float waitHandTime;
	// Use this for initialization
	void Start ()
    {
        calibrationControllers = new List<CalibrationController>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (timerEnabled && trainingMode)
            refreshTimer();
	}

    public void Prepare()
    {
        placer.CreateCablirationScene();
        //Create UI point
        calibrationPoint = GameObject.FindGameObjectWithTag("Calibration");
        calibrationPoint.AddComponent<DirectionIndicator>();
        //Set GazeCursor for height calculation
        gazeCursor.setCalculationMode();
        textToSpeechManager.SpeakSsml("Now we should begin the calibration process. First you should at the red calibration point on the floor, " +
            "so we calculate you height. Height is needed by the training program");
    }

    public void finishCalculateMode(float height)
    {
        textToSpeechManager.SpeakSsml("Your height has been calculated succesfully");
        //Store value
        userHeight = height;
        //Destroy calibrationPoint for height calculation
        Destroy(calibrationPoint);
        //UI/audio instructions
        DebugText.text = "Calculation Finished";
        //Set generic use for gaze
        gazeCursor.setGenericUse();
        //Start hand calibration
        handsTrackingController.enableHandCalibration();
    }

    public void calibrationFinished()
    {
        maxValue(calibrationControllers, out handDistance, out handHeight);
        //Create play scene
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
    }

    public int addCalibrationController(CalibrationController controller, float time)
    {
        controller.setFinishTime(time);
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

    private void refreshTimer()
    {
        timer += Time.deltaTime;
        if (timer > waitTime)
        {
            timer = 0f;
            finishGame();
        }
    }

    private void finishGame()
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

    private void enableTimer()
    {
        timerEnabled = true;
        refreshTimer();
    }

    private void disableTimer()
    {
        timerEnabled = false;
    }

    private void resetTimer()
    {
        timer = 0.0f;
    }

    private void maxValue(List<CalibrationController> list, out float maxD, out float maxH)
    {
        maxD = -1.0f;
        maxH = -1.0f;
        foreach(CalibrationController i in list)
        {
            if (i.getDistance() > maxD)
                maxD = i.getDistance();
            if (i.getHandHeight() > maxH)
                maxH = i.getHandHeight();
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

    }

    public void userSaidNo()
    {

    }
}
