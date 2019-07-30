using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

/*
 * Data script
 * Script for storing data for each game session
 */

public class DataScript : MonoBehaviour
{
    private bool requsetFinalize = false;
    //Gather values for every hand movement
    private Dictionary<float, float> speed_right = new Dictionary<float, float>();
    private Dictionary<float, float> speed_left = new Dictionary<float, float>();
    private Dictionary<float, float> acceleration_right = new Dictionary<float, float>();
    private Dictionary<float, float> acceleration_left = new Dictionary<float, float>();
    private Dictionary<float, float> handHeight_right = new Dictionary<float, float>();
    private Dictionary<float, float> handHeight_left = new Dictionary<float, float>();
    private Dictionary<float, bool> dataList = new Dictionary<float, bool>();               // save data pointer with this sequence manipuStart, manipulationEnded in data list, also save hand
    private Dictionary<int, bool> manipulationResults = new Dictionary<int, bool>();        // save results of each manipulation, also save hand
    private Dictionary<int, bool> manipulationHand = new Dictionary<int, bool>();           // save  the hand manipulation sequence
    private Dictionary<float, bool> interactionTouchedList = new Dictionary<float, bool>(); // save the "moments" which user touched the objects, also save hand

    private Vector3 privPos = Vector3.zero;
    private float privSpeed, privTime, cacheTime, startTime;
    private int startHour, startMinute, startSecond, manipulations;
    private string appPath;

    void Start()
    {
        appPath = Application.persistentDataPath;
        startTime = Time.time;
        startHour = DateTime.Now.Hour;
        startMinute = DateTime.Now.Minute;
        startSecond = DateTime.Now.Second;
    }

    private void Update()
    {
        if (requsetFinalize)
            FinalizeSession();
    }

    private void SaveSession()
    {
        string filePath, fileName;
        string lines="";
        byte[] bytes;

        string dateName = DateTime.Now.Day.ToString() + "_"
                        + DateTime.Now.Month.ToString() + "_"
                        + DateTime.Now.Year.ToString() + "_" 
                        + DateTime.Now.Hour.ToString() + ":" 
                        + DateTime.Now.Minute.ToString() ;

        foreach (KeyValuePair<float, float> s in speed_right)
        {
            lines = lines + s.Key.ToString() + "_" + s.Value.ToString() + "\n";
        }
        fileName = "speed_right_" + dateName + ".txt";
        filePath = Path.Combine(appPath, fileName);
        bytes = System.Text.Encoding.UTF8.GetBytes(lines);
        File.WriteAllBytes(filePath, bytes);

        lines = "";
        foreach (KeyValuePair<float, float> s in speed_left)
        {
            lines = lines + s.Key.ToString() + "_" + s.Value.ToString() + "\n";
        }
        fileName = "speed_left_" + dateName + ".txt";
        filePath = Path.Combine(appPath, fileName);
        bytes = System.Text.Encoding.UTF8.GetBytes(lines);
        File.WriteAllBytes(filePath, bytes);

        lines = "";
        foreach (KeyValuePair<float, float> a in acceleration_right)
        {
            lines = lines + a.Key.ToString() + "_" + a.Value.ToString() + "\n";
        }
        fileName = "acceleration_right_" + dateName + ".txt";
        filePath = Path.Combine(appPath, fileName);
        bytes = System.Text.Encoding.UTF8.GetBytes(lines);
        File.WriteAllBytes(filePath, bytes);

        lines = "";
        foreach (KeyValuePair<float, float> a in acceleration_left)
        {
            lines = lines + a.Key.ToString() + "_" + a.Value.ToString() + "\n";
        }
        fileName = "acceleration_left_" + dateName + ".txt";
        filePath = Path.Combine(appPath, fileName);
        bytes = System.Text.Encoding.UTF8.GetBytes(lines);
        File.WriteAllBytes(filePath, bytes);

        lines = "";
        foreach (KeyValuePair<float, float> h in handHeight_right)
        {
            lines = lines + h.Key.ToString() + "_" + h.Value.ToString() + "\n";
        }
        fileName = "handHeight_right_" + dateName + ".txt";
        filePath = Path.Combine(appPath, fileName);
        bytes = System.Text.Encoding.UTF8.GetBytes(lines);
        File.WriteAllBytes(filePath, bytes);

        lines = "";
        foreach (KeyValuePair<float, float> h in handHeight_left)
        {
            lines = lines + h.Key.ToString() + "_" + h.Value.ToString() + "\n";
        }
        fileName = "handHeight_left_" + dateName + ".txt";
        filePath = Path.Combine(appPath, fileName);
        bytes = System.Text.Encoding.UTF8.GetBytes(lines);
        File.WriteAllBytes(filePath, bytes);

        lines = "";
        foreach (KeyValuePair<float, bool> d in dataList)
        {
            lines = lines + d.Key.ToString() + "_" + d.Value.ToString() + "\n";
        }
        fileName = "dataList_" + dateName + ".txt";
        filePath = Path.Combine(appPath, fileName);
        bytes = System.Text.Encoding.UTF8.GetBytes(lines);
        File.WriteAllBytes(filePath, bytes);

        lines = "";
        foreach (KeyValuePair<int, bool> m in manipulationResults)
        {
            lines = lines + m.Key.ToString() + "_" + m.Value.ToString() + "\n";
        }
        fileName = "manipulationResults_" + dateName + ".txt";
        filePath = Path.Combine(appPath, fileName);
        bytes = System.Text.Encoding.UTF8.GetBytes(lines);
        File.WriteAllBytes(filePath, bytes);

        lines = "";
        foreach (KeyValuePair<int, bool> m in manipulationHand)
        {
            lines = lines + m.Key.ToString() + "_" + m.Value.ToString() + "\n";
        }
        fileName = "manipulationHand_" + dateName + ".txt";
        filePath = Path.Combine(appPath, fileName);
        bytes = System.Text.Encoding.UTF8.GetBytes(lines);
        File.WriteAllBytes(filePath, bytes);

        lines = "";
        foreach (KeyValuePair<float, bool> i in interactionTouchedList)
        {
            lines = lines + i.Key.ToString() + "_" + i.Value.ToString() + "\n";
        }
        fileName = "interactionTouchedList_" + dateName + ".txt";
        filePath = Path.Combine(appPath, fileName);
        bytes = System.Text.Encoding.UTF8.GetBytes(lines);
        File.WriteAllBytes(filePath, bytes);
    }

    private void FinalizeSession()
    {
        SaveSession();
        speed_right.Clear();
        acceleration_right.Clear();
        handHeight_right.Clear();
        speed_left.Clear();
        acceleration_left.Clear();
        handHeight_left.Clear();
        dataList.Clear();
        manipulationResults.Clear();
        manipulationHand.Clear();
        interactionTouchedList.Clear();
    }

    public void FinishSession()
    {
        requsetFinalize = true;
    }

    public void InteractionTouched(bool rightHand)
    {
        interactionTouchedList.Add(Time.time, rightHand);
    }

    public void AddValue(Vector3 position, float height, bool rightHand)
    {
        cacheTime = Time.time;
        if (cacheTime == privTime)
            return;

        float dt = cacheTime - privTime;
        float t_speed = Vector3.Magnitude(position - privPos) / dt;
        float t_accel = (t_speed - privSpeed) / dt;
        if (rightHand)
        {
            speed_right.Add(cacheTime, t_speed);
            acceleration_right.Add(cacheTime, t_accel);
            handHeight_right.Add(cacheTime, height);
        }
        else
        {
            speed_left.Add(cacheTime, t_speed);
            acceleration_left.Add(cacheTime, t_accel);
            handHeight_left.Add(cacheTime, height);
        }
        //Save values
        privSpeed = t_speed;
        privPos = position;
        privTime = cacheTime;
    }

    public void ManipulationStarted(bool rightHand)
    {
        dataList.Add(Time.time, rightHand);
    }

    public void ManipulationEnded(bool rightHand)
    {
        dataList.Add(Time.time, rightHand);
    }

    public void AddManipulationResult(bool result, bool rightHand)
    {
        manipulationResults.Add(manipulations, result);
        manipulationHand.Add(manipulations, rightHand);
        manipulations++;
    }
}
