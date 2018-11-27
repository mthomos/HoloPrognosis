using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

class DataScript : MonoBehaviour
{
    private bool interactionInProgress;
    private bool recordData;
    private bool requsetFinalize = false;

    private Dictionary<float, float> speed = new Dictionary<float, float>();
    private Dictionary<float, float> accelaration = new Dictionary<float, float>();
    private Dictionary<float, float> handHeight = new Dictionary<float, float>();
    // save data pointer with this sequence manipuStart, manipulationEnded
    private List<float> dataList = new List<float>(); // Save pointers of data
    private List<bool> manipulationResults = new List<bool>();
    private List<float> interactionTouchedList = new List<float>();
    //
    private Queue<Vector3> posBuffer = new Queue<Vector3>();
    private Queue<float> timeBuffer = new Queue<float>();
    private Vector3 privPos = Vector3.zero;
    private Vector3 currentPos;
    private float privSpeed = 0;
    private float privTime = 0;
    private float cacheTime;


    void Start()
    {
        interactionInProgress = false;
        recordData = false;
        EventManager.StartListening("box_collision", boxObjectCollision);
        EventManager.StartListening("floor_collision", floorObjectCollision);
    }

    private void Update()
    {
        if (posBuffer.Count > 0)
        {
            if (privPos != Vector3.zero)
            {
                currentPos = posBuffer.Dequeue();
                float t_time = timeBuffer.Dequeue();
                float t_speed = Vector3.Magnitude(currentPos - privPos) / t_time;
                float t_accel = (t_speed - privSpeed) / t_time;
                speed.Add(t_time, t_speed);
                accelaration.Add(t_time, t_accel);
                privSpeed = t_speed;
                privPos = currentPos;
            }
            else
            {
                privPos = posBuffer.Dequeue();
                timeBuffer.Dequeue();
            }
        }
        else
        {
            if (requsetFinalize)
                finalizeSession();
        }
    }

    private void floorObjectCollision()
    {
        objectCollision(false);
    }

    private void boxObjectCollision()
    {
        objectCollision(true);
    }

    private void objectCollision(bool inTheBox)
    {
        manipulationResults.Add(inTheBox);
    }

    public void objectTouched()
    {
        interactionTouchedList.Add(Time.time);
    }


    public void addValue(Vector3 value, float height)
    {
        cacheTime = Time.time;
        posBuffer.Enqueue(value);
        timeBuffer.Enqueue(cacheTime - privTime);
        handHeight.Add(cacheTime, height);
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

    public void saveSession()
    {
        string name = "session_" 
                    + DateTime.Now.Day.ToString() + "_"
                    + DateTime.Now.Month.ToString() + "_"
                    + DateTime.Now.Year.ToString() + "_" 
                    + DateTime.Now.Hour.ToString() + ":" 
                    + DateTime.Now.Minute.ToString() ;
        string path = Path.Combine(Application.dataPath + "/../", name);
        StreamWriter writer = new StreamWriter(new FileStream(path, FileMode.Create)); //Overwrite file
        string line = "";
        foreach(KeyValuePair<float, float> s in speed)
        {
            line = line + s.Key.ToString() + "_" + s.Value.ToString() + ",";
        }
        writer.WriteLine(line);
        line = "";
        foreach (KeyValuePair<float, float> a in accelaration)
        {
            line = line + a.Key.ToString() + "_" + a.Value.ToString() + ",";
        }
        writer.WriteLine(line);
        line = "";
        foreach (KeyValuePair<float, float> h in handHeight)
        {
            line = line + h.Key.ToString() + "_" + h.Value.ToString() + ",";
        }
        writer.WriteLine(line);
        line = "";
        foreach (float d in dataList)
        {
            line = line + d.ToString() + ",";
        }
        writer.WriteLine(line);
        line = "";
        foreach (bool m in manipulationResults)
        {
            line = line + m.ToString() + ",";
        }
        writer.WriteLine(line);
        line = "";
        foreach (float i in interactionTouchedList)
        {
            line = line + i.ToString() + ",";
        }
        writer.WriteLine(line);
        line = "";
        writer.Dispose();
    }

    public void interactionTouched()
    {
        interactionTouchedList.Add(Time.time);
    }

    private void finalizeSession()
    {
        speed.Clear();
        accelaration.Clear();
        handHeight.Clear();
        dataList.Clear();
        manipulationResults.Clear();
        interactionTouchedList.Clear();
    }

    public void finishSession()
    {
        requsetFinalize = true;
    }
}
