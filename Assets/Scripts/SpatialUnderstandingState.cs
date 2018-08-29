using System;
using UnityEngine;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;
using UnityEngine.Events;

public class SpatialUnderstandingState : Singleton<SpatialUnderstandingState>
{
    //Public Variables - For Editor
    //Scan surface variables, for ease -> public
    public float MinAreaForStats = 1.0f;
    public float MinAreaForComplete = 8.0f; // for floor
    public float MinHorizAreaForComplete = 5.0f; // for horizontal surfaces not only walls
    public float MinWallAreaForComplete = 0.0f; // for walls only
    public float TagalongDistance = 2.0f;
    public float PositionUpdateSpeed = 10f;
    public float SmoothingFactor = 0.6f;
    //Debug displays
    public TextMesh DebugDisplay;
    public TextMesh DebugSubDisplay;
    public ObjectPlacer Placer;
    public string SpaceQueryDescription;
    //Private Variables
    private bool _triggered;
    private bool HideText = false;
    private bool ready = false;
    private UnityAction TapListener;
    //
    protected Interpolator interpolator;
    //

    public bool DoesScanMeetMinBarForCompletion
    {
        get
        {
            // Only allow this when we are actually scanning
            if ((SpatialUnderstanding.Instance.ScanState != SpatialUnderstanding.ScanStates.Scanning) ||
                (!SpatialUnderstanding.Instance.AllowSpatialUnderstanding))
                return false;

            // Query the current playspace stats
            IntPtr statsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
            if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr) == 0)
                return false;

            SpatialUnderstandingDll.Imports.PlayspaceStats stats = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats();

            // Check our preset requirements
            if ((stats.TotalSurfaceArea > MinAreaForComplete) ||
                (stats.HorizSurfaceArea > MinHorizAreaForComplete) ||
                (stats.WallSurfaceArea > MinWallAreaForComplete))
                return true;

            return false;
        }
    }
    //OK
    public string PrimaryText
    {
        get
        {
            if (HideText)
                return string.Empty;

            // Display the space and object query results (has priority)
            if (!string.IsNullOrEmpty(SpaceQueryDescription))
            {
                return SpaceQueryDescription;
            }

            // Scan state
            if (SpatialUnderstanding.Instance.AllowSpatialUnderstanding)
            {
                switch (SpatialUnderstanding.Instance.ScanState)
                {
                    case SpatialUnderstanding.ScanStates.Scanning:
                        // Get the scan stats
                        IntPtr statsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
                        if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr) == 0)
                        {
                            return "playspace stats query failed";
                        }

                        // The stats tell us if we could potentially finish
                        if (DoesScanMeetMinBarForCompletion)
                        {
                            return "When ready, air tap to finalize your playspace";
                        }
                        return "Walk around and scan in your playspace";
                    case SpatialUnderstanding.ScanStates.Finishing:
                        return "Finalizing scan (please wait)";
                    case SpatialUnderstanding.ScanStates.Done:
                        return "Scan complete";
                    default:
                        return "ScanState = " + SpatialUnderstanding.Instance.ScanState;
                }
            }
            return string.Empty;
        }
    }
    //OK
    public Color PrimaryColor
    {
        get
        {
            ready = DoesScanMeetMinBarForCompletion;
            if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning)
            {
                return ready ? Color.yellow : Color.white;
            }

            // If we're looking at the menu, fade it out
            float alpha = 1.0f;

            // Special case processing & 
            return (!string.IsNullOrEmpty(SpaceQueryDescription)) ?
                (PrimaryText.Contains("processing") ? new Color(1.0f, 0.0f, 0.0f, 1.0f) : new Color(1.0f, 0.7f, 0.1f, alpha)) :
                new Color(1.0f, 1.0f, 1.0f, alpha);
        }
    }
    //OK
    public string DetailsText
    {
        get
        {
            if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.None)
            {
                return "";
            }

            // Scanning stats get second priority
            if ((SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning) &&
                (SpatialUnderstanding.Instance.AllowSpatialUnderstanding))
            {
                IntPtr statsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
                if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr) == 0)
                {
                    return "Playspace stats query failed";
                }
                SpatialUnderstandingDll.Imports.PlayspaceStats stats = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats();

                // Start showing the stats when they are no longer zero
                if (stats.TotalSurfaceArea > MinAreaForStats)
                {
                    SpatialMappingManager.Instance.DrawVisualMeshes = false;
                    string subDisplayText = string.Format("totalArea={0:0.0}, horiz={1:0.0}, wall={2:0.0}", stats.TotalSurfaceArea, stats.HorizSurfaceArea, stats.WallSurfaceArea);
                    subDisplayText += string.Format("\nnumFloorCells={0}, numCeilingCells={1}, numPlatformCells={2}", stats.NumFloor, stats.NumCeiling, stats.NumPlatform);
                    subDisplayText += string.Format("\npaintMode={0}, seenCells={1}, notSeen={2}", stats.CellCount_IsPaintMode, stats.CellCount_IsSeenQualtiy_Seen + stats.CellCount_IsSeenQualtiy_Good, stats.CellCount_IsSeenQualtiy_None);
                    return subDisplayText;
                }
                return "";
            }
            return "";
        }
    }

    private void Update_DebugDisplay()
    {
        // Basic checks
        if (DebugDisplay == null) return;
        // Update display text
        DebugDisplay.text = PrimaryText;
        DebugDisplay.color = PrimaryColor;
        DebugSubDisplay.text = DetailsText;
        //Update display rotation
        Update_DisplayRotation();
    }

    private void Start()
    {
        TapListener = new UnityAction(Tap_Triggered);
        EventManager.StartListening("tap", TapListener);
        //
        interpolator = gameObject.GetComponent<Interpolator>();
        interpolator.SmoothLerpToTarget = true;
        interpolator.SmoothPositionLerpRatio = SmoothingFactor;
    }

    private void Tap_Triggered()
    {
        if (ready &&
            (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning) &&
             !SpatialUnderstanding.Instance.ScanStatsReportStillWorking)
            SpatialUnderstanding.Instance.RequestFinishScan();
    }

    // Update is called once per frame
    private void Update()
    {
        // Updates
        Update_DebugDisplay();
        
        if (!_triggered && SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done)
        {
            _triggered = true;
            HideText = true;
            EventManager.StopListening("tap", TapListener);
            Placer.CreateScene();
        }
    }

    private void Update_DisplayRotation()
    {
        Vector3 tagalongTargetPosition;
        tagalongTargetPosition = Camera.main.transform.position + Camera.main.transform.forward * TagalongDistance;
        interpolator.PositionPerSecond = PositionUpdateSpeed;
        interpolator.SetTargetPosition(tagalongTargetPosition);

        Vector3 directionToTarget = Camera.main.transform.position - DebugDisplay.transform.position;

        directionToTarget.y = 0.0f;

        // If we are right next to the camera the rotation is undefined. 
        if (directionToTarget.sqrMagnitude < 0.005f)
            return;

        DebugDisplay.transform.rotation = Quaternion.LookRotation(-directionToTarget);
        DebugSubDisplay.transform.rotation = Quaternion.LookRotation(-directionToTarget);
    }
}