using System;
using System.Collections.Generic;
using UnityEngine;

public struct InteractionData
{
    //Booleans
    public bool interactionBegin;
    public bool objectTouched;
    public bool objectHold;
    public bool objectManipulationStarted;
    public bool objectManipulationInProgress;
    public bool objectManipulationFinished;
    public bool interactionFinished;
    public bool objectInTheBox;
    // Time variables
    public float timeStartInteraction;
    public float timeToTouch; //Start of interaction
    public float timeToHold; //Touch->Hold
    public float timeToStartManipulation; //Touch->StartManipulate
    public float timeToEndManipualtion;
    //Data variables
    public float tremor;
    public int failedTries;
    public int preTouchedTries;
    public List<Vector3> tremorList;

    public InteractionData(float time)
    {   
        interactionBegin = true;
        timeStartInteraction = time;
        failedTries = 0;
        preTouchedTries = 0;
        tremorList = new List<Vector3>();
        //
        objectTouched = false;
        objectHold = false;
        objectManipulationStarted = false;
        objectManipulationInProgress = false;
        objectManipulationFinished = false;
        interactionFinished = false;
        timeToTouch = .0f;
        timeToHold = .0f;
        timeToStartManipulation = .0f;
        timeToEndManipualtion = .0f;
        tremor = .0f;
        objectInTheBox = false;
    }
}

public struct SessionData
{
    public String sessionName;
    public DateTime date;
    public float duartion;
    public int interactions;
    public List<InteractionData> interactionList;

    public SessionData(string sessionName, DateTime date, float duartion, int interactions, List<InteractionData> interactionList)
    {
        this.sessionName = sessionName;
        this.date = date;
        this.duartion = duartion;
        this.interactions = interactions;
        this.interactionList = interactionList;
    }
}

public class InteractionAnalysis : MonoBehaviour
{
    private bool interactionInProgress;
    private bool recordData;
    private int interactions = 0;
    private InteractionData interaction;
    private List<InteractionData> interactionList;
    private List<Vector3> tremorValueList = new List<Vector3>();
    private float startSession, endSession;
    private DateTime localDate;

    void Start()
    {
        interactionInProgress = false;
        recordData = false;
        interactionList = new List<InteractionData>();
        EventManager.StartListening("box_collision", boxObjectCollision);
        EventManager.StartListening("floor_collision", floorObjectCollision);
    }

    void Update()
    {

    }

    private void floorObjectCollision()
    {
        objectCollision(false);
    }

    private void boxObjectCollision()
    {
        objectCollision(true);
    }

    public void sessionEnded()
    {
        //Store interactionList to DB

    }

    public void interactionDetected()
    {
        if (interactions == 0)
        {
            localDate = DateTime.Now;
            startSession = Time.time;
        }
        //Reset
        interaction =  new InteractionData(Time.time);
        tremorValueList.Clear();
    }

    public void interactionTouched()
    {
        if(!interaction.objectTouched && interaction.interactionBegin)
        {
            interaction.objectTouched = true;
            interaction.timeToTouch = Time.time;
        }
    }

    public void interactionNotTouched()
    {
        if (interaction.objectTouched && interaction.interactionBegin)
        {
            interaction.objectTouched = false;
            interaction.preTouchedTries++;
        }
    }

    public void interactionHold()
    {
        if(interaction.objectTouched)
        {
            interaction.objectHold = true;
            interaction.timeToHold = Time.time;
        }
        else
        {
            interaction.failedTries++;
        }   
    }

    public void manipulationStarted()
    {
        if(interaction.objectTouched)
        {
            interaction.objectManipulationStarted = true;
            interaction.objectManipulationInProgress = true;
            interaction.timeToStartManipulation = Time.time;
        }
        else
        {
            interaction.failedTries++;
        }
    }

    public void manipulationFinished()
    {
        if(interaction.objectManipulationInProgress)
        {
            interaction.objectManipulationFinished =true;
            interaction.objectManipulationInProgress = false;
            interaction.timeToEndManipualtion = Time.time;
        }
    }
    
    public void manipulationCanceled()
    {
        manipulationFinished();
    }

    public void objectCollision(bool inTheBox)
    {
        interaction.objectInTheBox = inTheBox;
        interaction.interactionFinished = true;
        //End interaction
        interactionEnded();
    }

    public void interactionEnded()
    {
        if (recordData)
        {
            interaction.tremorList = tremorValueList;
            interactionList.Add(interaction);
            interactions++;
        }
    }

    public void addTremorValue(Vector3 value)
    {
        tremorValueList.Add(value);
    }

    public void enableRecording()
    {
        recordData = true;
    }

    public void disableRecording()
    {
        recordData = false;
    }
}