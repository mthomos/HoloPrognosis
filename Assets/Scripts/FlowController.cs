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
    //
    private bool heightCalculationDone = false;
    private bool calibrationDone = false;
    private bool scanDone = false;
    //
    private GameObject calibrationPoint;
    private float userHeight;
    private List<CalibrationController> calibrationControllers;
    float maxCalibrationDistance;
    //Create timer variables
    public float waitTime;
    private float timer;
    private bool timerEnabled = false;

	// Use this for initialization
	void Start ()
    {
        calibrationControllers = new List<CalibrationController>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (timerEnabled)
            refreshTimer();
	}

    public void Prepare()
    {
        scanDone = true;
        placer.CreateCablirationScene();
        //Create UI point
        calibrationPoint = GameObject.FindGameObjectWithTag("Calibration");
        calibrationPoint.AddComponent<DirectionIndicator>();
        //Set GazeCursor for height calculation
        gazeCursor.setCalculationMode();
    }

    public void finishCalculateMode(float height)
    {
        heightCalculationDone = true;
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

        calibrationDone = true;
        maxCalibrationDistance = maxValue(calibrationControllers);
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
    }

    public int addCalibrationController(CalibrationController controller, float time)
    {
        controller.setFinishTime(time);
        calibrationControllers.Add(controller);
        return calibrationControllers.Count;
    }

    public float getHeadDistanceLimit()
    {
        return maxCalibrationDistance;
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

    private float maxValue(List<CalibrationController> list)
    {
        float max = -1.0f;
        foreach(CalibrationController i in list)
        {
            if (i.getDistance() > max)
                max = i.getDistance();
        }
        //reduce value by 2cm
        max -= 0.03f;
        return max;
    }
}
