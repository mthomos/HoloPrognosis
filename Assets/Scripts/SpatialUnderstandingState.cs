using System;
using UnityEngine;
using HoloToolkit.Unity;
using UnityEngine.Events;

public class SpatialUnderstandingState : Singleton<SpatialUnderstandingState>
{
    //Public Variables - For Editor
    public AudioSource audioSource;
    public SpatialUnderstandingCustomMesh SpatialUnderstandingMesh;
    public Material OccludedMaterial;
    public float MinAreaForStats = 2.0f; // both floor and wall surfaces
    public float MinAreaForComplete = 4.0f; // for floor
    public float MinHorizAreaForComplete = 1.0f; // for horizontal surfaces not only walls
    public float MinWallAreaForComplete = 0.0f; // for walls only
    //Debug displays
    public TextMesh DebugDisplay;
    public UiController uiController;
    //Private Variables
    private bool triggered, scanReady, feedback_triggered;
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

    private string PrimaryText
    {
        get
        {
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
                                if (uiController.greekEnabled)
                                {
                                    audioSource.Stop();
                                    audioSource.clip = uiController.SpatialFinshClip;
                                    audioSource.Play();
                                }
                                else
                                {
                                    TextToSpeech.Instance.StopSpeaking();
                                    TextToSpeech.Instance.StartSpeaking("Space scanned, air tap to finalize your playspace");
                                }
                            }
                            return uiController.greekEnabled ? "Χώρος αποτυπώθηκε \n Kάντε κλικ για να τερματισετε το σκανάρισμα" : "Space scanned, air tap to finalize your playspace";
                        }
                        else
                        return uiController.greekEnabled ? "Σκανάρετε τον χώρο σας" : "Walk around and scan in your playspace";
                    case SpatialUnderstanding.ScanStates.Finishing:
                        return uiController.greekEnabled ? "Λήξη σκαναρίσματος" :  "Finalizing scan";
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
                return Color.green;
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
        if (uiController.greekEnabled)
        {
            audioSource.Stop();
            audioSource.clip = uiController.SpatialStartClip;
            audioSource.Play();
        }
        else
        {
            TextToSpeech.Instance.StopSpeaking();
            TextToSpeech.Instance.StartSpeaking("Walk around and scan in your space");
        }
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
        
        if (!triggered && SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done)
        {
            triggered = true;
            EventManager.StopListening("tap", TapListener);
            // Hide Spatial Mesh
            SpatialUnderstandingMesh.MeshMaterial = OccludedMaterial;
            //Create UI
            uiController.CreateUI();
        }
    }
}