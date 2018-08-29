using System.Collections;
using UnityEngine;

public class GazeStabilizer
{
    private int bufferCount = 20;
    private int currentCount = 0;
    private ArrayList GazeOriginSamples;
    private ArrayList RotationSamples;
    private const float PositionStandardDeviationReset = 0.2f;
    private const float StabalizedLerpBoost = 10.0f;
    private const float UnstabalizedLerpFactor = 0.3f;

    private Vector3 newStableGazePosition;
    private Quaternion newStableGazeRotation;

    public GazeStabilizer()
    {
        GazeOriginSamples = new ArrayList();
        RotationSamples = new ArrayList();
    }

    public GazeStabilizer(int count)
    {
        bufferCount = count;
        GazeOriginSamples = new ArrayList();
        RotationSamples = new ArrayList();
    }

    public void addPositionSample(Vector3 sample)
    {
        GazeOriginSamples.Add(sample);
        currentCount++;
    }

    public void addRotationSample(Vector3 sample)
    {
        RotationSamples.Add(sample);
        currentCount++;
    }

    public void resetSamples()
    {
        GazeOriginSamples.Clear();
        RotationSamples.Clear();
        currentCount = 0;
    }

    public float euclideanDistance(Vector3 point)
    {
        return Mathf.Sqrt(Mathf.Pow(point.x, 2) + Mathf.Pow(point.y, 2) + Mathf.Pow(point.z, 2));
    }

    public float getStandardDeviation(ArrayList List)
    {
        float s = 0;
        float medean = 0;
        for (int i = 0; i < List.Count; i++)
            medean = medean + euclideanDistance((Vector3)List[i]);

        medean = medean / List.Count;
        for (int i = 0; i < List.Count; i++)
            s = s + Mathf.Pow((medean - euclideanDistance((Vector3)List[i])), 2);
        s = s / (List.Count - 1);

        return Mathf.Sqrt(s);
    }

    private Vector3 getCentroidPoint(ArrayList List)
    {
        float x = 0, y = 0, z = 0;
        for (int i = 0; i < List.Count; i++)
        {
            x = x + ((Vector3)List[i]).x;
            y = y + ((Vector3)List[i]).y;
            z = z + ((Vector3)List[i]).z;
        }
        return new Vector3(x / List.Count, y / List.Count, z / List.Count);
    }

    public Vector3 getNewStableGazePosition()
    {
        return newStableGazePosition;
    }

    public Quaternion getNewStableGazeRotation()
    {
        return newStableGazeRotation;
    }

    public void UpdateStability(Vector3 gazePosition, Vector3 rotation)
    {
        float lerpPower = UnstabalizedLerpFactor;
        if (GazeOriginSamples.Count > bufferCount  && // we have enough samples and...
            (getStandardDeviation(GazeOriginSamples) > PositionStandardDeviationReset ))//|| // the standard deviation of positions is high or...
             //getStandardDeviation(RotationSamples) > DirectionStandardDeviationReset)) // the standard deviation of directions is high
        {
            resetSamples();
            newStableGazePosition = gazePosition;
            newStableGazeRotation = Quaternion.Euler(rotation);
        }
        else if (GazeOriginSamples.Count > bufferCount)
        {
            // We've detected that the user's gaze is fairly fixed, so start stabilizing.  The more fixed the gaze the less the cursor will move.
            lerpPower = StabalizedLerpBoost * ((getStandardDeviation(GazeOriginSamples) + (getStandardDeviation(RotationSamples))));
            newStableGazePosition = Vector3.Lerp(newStableGazePosition, gazePosition, lerpPower);
            newStableGazeRotation = Quaternion.LookRotation(Vector3.Lerp(newStableGazeRotation * Vector3.forward, rotation, lerpPower));
        }
        else
        {
            newStableGazePosition = gazePosition;
            newStableGazeRotation = Quaternion.Euler(rotation);
        }
    }
}

