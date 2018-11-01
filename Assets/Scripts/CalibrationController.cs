using System.Collections;
using UnityEngine;

public class CalibrationController
{
    private bool rightHand;
    private float headHandDistance;
    private float handHeight;
    private float startTime;
    private float finishTime;

    private int bufferCount = 50;
    private int currentCount = 0;
    private ArrayList distanceBuffer;
    private ArrayList handHeightBuffer;


    public CalibrationController(bool hand, float time)
    {
        rightHand = hand;
        distanceBuffer = new ArrayList();
        handHeightBuffer = new ArrayList();
        startTime = time;
    }

    public void addValue(float headDistance, float zDistance)
    {
        distanceBuffer.Add(headDistance);
        handHeightBuffer.Add(zDistance);
        currentCount++;
        updateCalibration();
    }

    public void resetSamples()
    {
        distanceBuffer.Clear();
        currentCount = 0;
    }

    public float getStandardDeviation(ArrayList List)
    {
        float s = 0;
        float medean = 0;
        for (int i = 0; i < List.Count; i++)
            medean = medean + (float)List[i];

        medean = medean / List.Count;
        for (int i = 0; i < List.Count; i++)
            s = s + Mathf.Pow((medean - (float)List[i]), 2);
        s = s / (List.Count - 1);

        return Mathf.Sqrt(s);
    }

    public void updateCalibration()
    {
        if (currentCount < bufferCount)
            return;

        if (distanceBuffer.Count > bufferCount)
        {
            distanceBuffer.Sort();
            distanceBuffer.RemoveAt(bufferCount-1);
            distanceBuffer.RemoveAt(0);
            distanceBuffer.Sort();
            //
            handHeightBuffer.Sort();
            handHeightBuffer.RemoveAt(bufferCount - 1);
            handHeightBuffer.RemoveAt(0);
            handHeightBuffer.Sort();
            currentCount = -2;
        }
    }

    public float getDistance()
    {
        foreach (float i in distanceBuffer)
        {
            headHandDistance += i;
        }
        headHandDistance /= currentCount;
        return headHandDistance;
    }

    public float getHandHeight()
    {
        foreach (float i in handHeightBuffer)
        {
            handHeight += i;
        }
        handHeight /= currentCount;
        return handHeight;
    }

    public bool isRightHand()
    {
        return rightHand;
    }

    public void setFinishTime(float time)
    {
        finishTime = time;
    }
}
