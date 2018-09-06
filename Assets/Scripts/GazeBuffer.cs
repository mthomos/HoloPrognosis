using System.Collections;
using UnityEngine;

public class GazeBuffer
{
    private int bufferCount = 50;
    private int bufferLimit = 1000;
    private int currentCount = 0;
    private ArrayList GazeOriginSamples, GazeForwardSamples;
    private const float StandardDeviationReset = 0.2f;
    //Variables for optimization
    private ArrayList bufferDistancesOrigin, bufferDistancesForward;
    private float sumDistancesOrigin = 0, sumDistancesForward = 0;
    private Vector3 centralPointOrigin = new Vector3(0, 0, 0);
    private Vector3 centralPointForward = new Vector3(0, 0, 0);
    //Output variables for use
    private Vector3 stableGazeOrigin;
    private Vector3 stableGazeForward;
    //Variables for standard deviation
    private float standardDeviationOrigin;
    private float standardDeviationForward;

    public GazeBuffer()
    {
        GazeOriginSamples = new ArrayList();
        GazeForwardSamples = new ArrayList();
        bufferDistancesOrigin = new ArrayList();
        bufferDistancesForward = new ArrayList();
    }

    public GazeBuffer(int count)
    {
        bufferCount = count;
        GazeOriginSamples = new ArrayList();
        GazeForwardSamples = new ArrayList();
        bufferDistancesOrigin = new ArrayList();
        bufferDistancesForward = new ArrayList();
    }

    public void addSamples(Vector3 origin, Vector3 forward)
    {
        if (currentCount < bufferLimit)
        {
            //Update sums and central points
            sumDistancesOrigin = sumDistancesOrigin + euclideanDistance(origin);
            sumDistancesForward = sumDistancesForward + euclideanDistance(forward);
            centralPointOrigin.x = centralPointOrigin.x + origin.x;
            centralPointOrigin.y = centralPointOrigin.y + origin.y;
            centralPointOrigin.z = centralPointOrigin.z + origin.z;
            centralPointForward.x = centralPointForward.x + forward.x;
            centralPointForward.y = centralPointForward.y + forward.y;
            centralPointForward.z = centralPointForward.z + forward.z;
            //Update buffers
            GazeOriginSamples.Add(origin);
            GazeForwardSamples.Add(forward);
            bufferDistancesOrigin.Add(euclideanDistance(origin));
            bufferDistancesForward.Add(euclideanDistance(forward));
            currentCount++;
        }
        else
        {
            Vector3 oldOrigin = (Vector3)GazeOriginSamples[currentCount % bufferLimit];
            Vector3 oldForward = (Vector3)GazeForwardSamples[currentCount % bufferLimit];
            //Update sums and central points
            sumDistancesOrigin = sumDistancesOrigin + euclideanDistance(origin-oldOrigin);
            sumDistancesForward = sumDistancesForward + euclideanDistance(forward-oldForward);
            centralPointOrigin.x = centralPointOrigin.x + origin.x - oldOrigin.x;
            centralPointOrigin.y = centralPointOrigin.y + origin.y - oldOrigin.y;
            centralPointOrigin.z = centralPointOrigin.z + origin.z - oldOrigin.z;
            centralPointForward.x = centralPointForward.x + forward.x - oldForward.x;
            centralPointForward.y = centralPointForward.y + forward.y - oldForward.y;
            centralPointForward.z = centralPointForward.z + forward.z - oldForward.z;
            //Update buffers
            GazeOriginSamples.Insert(currentCount % bufferLimit, origin);
            GazeForwardSamples.Insert(currentCount % bufferLimit, forward);
            bufferDistancesOrigin.Insert(currentCount % bufferLimit, euclideanDistance(origin));
            bufferDistancesForward.Insert(currentCount % bufferLimit, euclideanDistance(forward));
            currentCount++;
        }
    }

    public void resetSamples()
    {
        GazeOriginSamples.Clear();
        GazeForwardSamples.Clear();
        currentCount = 0;
    }

    public float euclideanDistance(Vector3 point)
    {
        return Mathf.Sqrt(Mathf.Pow(point.x, 2) + Mathf.Pow(point.y, 2) + Mathf.Pow(point.z, 2));
    }

    private void UpdateStandardDeviation()
    {
        float s_origin = 0;
        float medean_origin = sumDistancesOrigin /bufferCount;
        float s_forward = 0;
        float medean_forward = sumDistancesForward / bufferCount;

        for (int i = 0; i < bufferDistancesOrigin.Count; i++)
        {
            s_origin = s_origin + Mathf.Pow((medean_origin - (float)bufferDistancesOrigin[i]), 2);
            s_forward = s_forward + Mathf.Pow((medean_forward - (float)bufferDistancesForward[i]), 2);
        }

        standardDeviationOrigin = s_origin / (bufferDistancesOrigin.Count - 1);
        standardDeviationForward = s_forward / (bufferDistancesOrigin.Count - 1);

    }
    private Vector3 getCentroidPointOrigin()
    {
        return new Vector3(centralPointOrigin.x/bufferCount, centralPointOrigin.y/bufferCount, centralPointOrigin.z/bufferCount);
    }

    private Vector3 getCentroidPointForward()
    {
        return new Vector3(centralPointForward.x / bufferCount, centralPointForward.y / bufferCount, centralPointForward.z / bufferCount);
    }

    public Vector3 getStableGazeOrigin()
    {
        return stableGazeOrigin;
    }

    public Vector3 getStableGazeForward()
    {
        return stableGazeForward;
    }

    public void UpdateStability(Vector3 gazeOrigin, Vector3 gazeForward)
    {
        stableGazeOrigin = gazeOrigin;
        stableGazeForward = gazeForward;
        UpdateStandardDeviation();
        if (GazeOriginSamples.Count > bufferCount && // we have enough samples and...
            (standardDeviationOrigin > StandardDeviationReset) &&
                (standardDeviationForward > StandardDeviationReset))
        {   
            resetSamples();
        }
        else if (GazeOriginSamples.Count > bufferCount &&
            (standardDeviationOrigin <= StandardDeviationReset) &&
                (standardDeviationForward <= StandardDeviationReset))
        {
            stableGazeOrigin = getCentroidPointOrigin();
            stableGazeForward = getCentroidPointForward();
        }
    }
}
