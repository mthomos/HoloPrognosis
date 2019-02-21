using System;
using UnityEngine;
using HoloToolkit.Unity;
using UnityEngine.Events;

public class SpatialUnderstandingState : Singleton<SpatialUnderstandingState>
{
    //Public Variables - For Editor
    public SpatialUnderstandingCustomMesh SpatialUnderstandingMesh;
    public Material OccludedMaterial;
    public float MinAreaForStats = 2.0f; // both floor and wall surfaces
    public float MinAreaForComplete = 4.0f; // for floor
    public float MinHorizAreaForComplete = 1.0f; // for horizontal surfaces not only walls
    public float MinWallAreaForComplete = 0.0f; // for walls only
    //Debug displays
    public TextMesh DebugDisplay;
    public UiController uiController;
    public string SpaceQueryDescription;
    //Private Variables
    private bool _triggered, scanReady, feedback_triggered;
    private UnityAction TapListener;

    private bool DoesScanMeetMinBarForCompletion
    {
        get
        {
            if ((SpatialUnderstanding.Instance.ScanState != SpatialUnderstanding.ScanStates.Scanning) ||
                (!SpatialUnderstanding.Instance.AllowSpatialUnderstanding))
                return false;

            IntPtr statsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
            if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr) == 0)
                return false;

            SpatialUnderstandingDll.Imports.PlayspaceStats stats = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats();

            // Check our preset requirements
            if ((stats.TotalSurfaceArea > MinAreaForComplete) ||
                (stats.HorizSurfaceArea > MinHorizAreaForComplete) ||
                (stats.WallSurfaceArea > MinWallAreaForComplete))
                return true;
            else
                return false;
        }
    }
    //OK
    private string PrimaryText
    {
        get
        {
            // Display the space and object query results (has priority)
            if (!string.IsNullOrEmpty(SpaceQueryDescription))
                return SpaceQueryDescription;

            // Scan state
            if (SpatialUnderstanding.Instance.AllowSpatialUnderstanding)
            {
                switch (SpatialUnderstanding.Instance.ScanState)
                {
                    case SpatialUnderstanding.ScanStates.Scanning:
                        if (DoesScanMeetMinBarForCompletion)
                        {
                            if (!feedback_triggered)
                            {
                                feedback_triggered = true;
                                TextToSpeech.Instance.StartSpeaking("Space scanned, air tap to finalize your playspace");
                            }
                            return "Space scanned, air tap to finalize your playspace";
                        }
                        else
                        return "Walk around and scan in your playspace";
                    case SpatialUnderstanding.ScanStates.Finishing:
                        return "Finalizing scan";
                    case SpatialUnderstanding.ScanStates.Done:
                        return "";
                    default:
                        return "";
                }
            }
            else
                return string.Empty;
        }
    }

    public Color PrimaryColor
    {
        get
        {
            scanReady = DoesScanMeetMinBarForCompletion;
            if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning && scanReady)
                return Color.yellow;
            else
                return Color.white;
        }
    }

    private void Update_DebugDisplay()
    {
        // Update display text
        DebugDisplay.text = PrimaryText;
        DebugDisplay.color = PrimaryColor;
    }

    private void Start()
    {
        TapListener = new UnityAction(Tap_Triggered);
        EventManager.StartListening("tap", TapListener);

        TextToSpeech.Instance.StartSpeaking("Walk around and scan in your playspace");
    }

    private void Tap_Triggered()
    {
        if (scanReady &&
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
            EventManager.StopListening("tap", TapListener);
            // Hide Spatial Mesh
            SpatialUnderstandingMesh.MeshMaterial = OccludedMaterial;
            //Create UI
            uiController.createUI();
        }
    }
}