using System;
using System.Collections.Generic;
using UnityEngine;

public class DataScript : MonoBehaviour
{
    private bool requsetFinalize = false;
    //Gather values for every hand movement
    private Dictionary<float, float> speed = new Dictionary<float, float>();
    private Dictionary<float, float> acceleration = new Dictionary<float, float>();
    private Dictionary<float, float> handHeight = new Dictionary<float, float>();
    private List<float> dataList = new List<float>();// save data pointer with this sequence manipuStart, manipulationEnded in data list
    private List<bool> manipulationResults = new List<bool>(); // save results of each manipulation
    private List<float> interactionTouchedList = new List<float>(); // save the "moments" which user touched the objects
    //
    private Vector3 privPos = Vector3.zero;
    private float privSpeed, privTime, cacheTime;
    private float startTime;
    private int startHour, startMinute, startSecond;

    void Start()
    {
        startTime = Time.time;
        startHour = DateTime.Now.Hour;
        startMinute = DateTime.Now.Minute;
        startSecond = DateTime.Now.Second;
    }

    private void Update()
    {
            if (requsetFinalize)
                finalizeSession();
    }

    private void saveSession()
    {
        string line="";
        string dateName = DateTime.Now.Day.ToString() + "_"
                        + DateTime.Now.Month.ToString() + "_"
                        + DateTime.Now.Year.ToString() + "_" 
                        + DateTime.Now.Hour.ToString() + ":" 
                        + DateTime.Now.Minute.ToString() ;

        string speedFile = "speed_" + dateName;
        string accelerationFile = "acceleration_" + dateName;
        string handHeightFile = "handHeight_" + dateName;
        string dataListFile = "dataList_" + dateName;
        string manipulationResultsFile = "manipulationResults_" + dateName;
        string interactionTouchedListsFile = "interactionTouchedLists_" + dateName;

        foreach (KeyValuePair<float, float> s in speed)
        {
            line = line + s.Key.ToString() + "_" + s.Value.ToString() + ",";
        }
        FileManager.Instance.addRequest(speedFile, line);

        line = "";
        foreach (KeyValuePair<float, float> a in acceleration)
        {
            line = line + a.Key.ToString() + "_" + a.Value.ToString() + ",";
        }
        FileManager.Instance.addRequest(accelerationFile, line);

        line = "";
        foreach (KeyValuePair<float, float> h in handHeight)
        {
            line = line + h.Key.ToString() + "_" + h.Value.ToString() + ",";
        }
        FileManager.Instance.addRequest(handHeightFile, line);

        line = "";
        foreach (float d in dataList)
        {
            line = line + d.ToString() + ",";
        }
        FileManager.Instance.addRequest(dataListFile, line);

        line = "";
        foreach (bool m in manipulationResults)
        {
            line = line + m.ToString() + ",";
        }
        FileManager.Instance.addRequest(manipulationResultsFile, line);

        line = "";
        foreach (float i in interactionTouchedList)
        {
            line = line + i.ToString() + ",";
        }
        FileManager.Instance.addRequest(interactionTouchedListsFile, line);
    }

    private void finalizeSession()
    {
        saveSession();
        speed.Clear();
        acceleration.Clear();
        handHeight.Clear();
        dataList.Clear();
        manipulationResults.Clear();
        interactionTouchedList.Clear();
    }

    public void finishSession()
    {
        requsetFinalize = true;
    }

    public void interactionTouched()
    {
        interactionTouchedList.Add(Time.time);
    }

    public void addValue(Vector3 position, float height)
    {
        cacheTime = Time.time;
        if (cacheTime == privTime)
            return;

        float dt = cacheTime - privTime;
        float t_speed = Vector3.Magnitude(position - privPos) / dt;
        float t_accel = (t_speed - privSpeed) / dt;
        speed.Add(cacheTime, t_speed);
        acceleration.Add(cacheTime, t_accel);
        handHeight.Add(cacheTime, height);
        //Save values
        privSpeed = t_speed;
        privPos = position;
        privTime = cacheTime;
    }

    public void manipulationStarted()
    {
        dataList.Add(Time.time);
    }

    public void manipulationEnded()
    {
        dataList.Add(Time.time);
    }

    public void addManipulationResult(bool result)
    {
        manipulationResults.Add(result);
    }
}
