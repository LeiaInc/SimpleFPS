
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Linq;
using LeiaLoft;
using UnityEngine.UI;

public class BlinkTrackingUnityPlugin : MonoBehaviour
{
    public LeiaDisplayParameter param;
    public LeiaHeadTracking.Engine.Result trackingResult = new LeiaHeadTracking.Engine.Result();
    private LeiaHeadTracking.Engine headTrackingEngine = null;
    LeiaCamera leiaCamera;
    public float predictedFaceX = 0, predictedFaceY = 0, predictedFaceZ = 600;
    public float faceX = 0, faceY = 0, faceZ = 600;
    float eyeDistanceThreshold = 200f;

    private double old_time_stamp = 0.0;
    private double time_delay = 88.0;
    public Text delayLabel;

    public Transform testHead;

    public void SetTimeDelay(float newTimeDelay)
    {
        Debug.Log("SetTimeDelay " + newTimeDelay);
        time_delay = newTimeDelay;
        if (delayLabel != null)
        {
            delayLabel.text = "Time Delay: " + time_delay;
        }
        else
        {
            Debug.LogError("delayLabel not set for time delay slider");
        }
    }

    public Text slidingScaleLabel;
    public Text cameraShiftYOffsetLabel;
    EyeTrackingCameraShift _shifter;
    public EyeTrackingCameraShift shifter
    {
        get
        {
            if (_shifter == null)
            {
                _shifter = FindObjectOfType<EyeTrackingCameraShift>();
            }
            return _shifter;
        }
    }

    public void SetSlidingScale(float newSlidingScale)
    {
        if (slidingScaleLabel != null)
        {
            slidingScaleLabel.text = "Sliding scale: " + shifter.slidingScale;
        }
        else
        {
            Debug.LogError("slidingScaleLabel not set for sliding scale slider");
        }
        if (shifter != null)
        {
            shifter.SetSlidingScale(newSlidingScale);
        }
    }

    LeiaDisplay _leiaDisplay;
    LeiaDisplay leiaDisplay
    {
        get
        {
            if (_leiaDisplay == null)
            {
                _leiaDisplay = FindObjectOfType<LeiaDisplay>();
            }

            return _leiaDisplay;
        }
    }

    public void SetCameraShiftYOffset(float newYOffset)
    {
        if (slidingScaleLabel != null)
        {
            cameraShiftYOffsetLabel.text = "Cam Shift Y-Offset: " + newYOffset;
        }
        else
        {
            Debug.LogError("slidingScaleLabel not set for sliding scale slider");
        }
        leiaDisplay.SetCameraShiftOffsetY(newYOffset);
    }

    private bool _cameraConnectedPrev;
    private bool _cameraConnected;
    public bool CameraConnected
    {
        get
        {
            return _cameraConnected;
        }
    }

    public Text debugLabel;

    float Z0;

    public float B_AT_Z0;
    public float B_AT_Z0_LF;
    public float B_AT_Z0_Stereo;

    public void SetB_AT_Z0(float value)
    {
        if (LeiaDisplay.Instance.DesiredRenderTechnique == LeiaDisplay.RenderTechnique.Stereo)
        {
            SetB_AT_Z0_Stereo(value);
        }
        else
        {
            SetB_AT_Z0_LF(value);
        }
    }

    public void SetB_AT_Z0_LF(float value)
    {
        B_AT_Z0_LF = value;
    }

    public void SetB_AT_Z0_Stereo(float value)
    {
        B_AT_Z0_Stereo = value;
    }

    LeiaVirtualDisplay leiaVirtualDisplay;
    FaceChooser faceChooser;
    void Awake()
    {
        faceChooser = new FaceChooser();
        if (slidingScaleLabel != null)
        {
            slidingScaleLabel.text = "Sliding scale: " + shifter.slidingScale;
        }
        if (delayLabel != null)
        {
            delayLabel.text = "Time Delay: " + time_delay;
        }
        // if (headTrackingEngine != null)
        // {
        //     headTrackingEngine.SetTrackedEye(false, true);
        // }
        leiaCamera = FindObjectOfType<LeiaCamera>();
        leiaVirtualDisplay = FindObjectOfType<LeiaVirtualDisplay>();

        param.centerViewNumX = 3.5f; //view offset
        param.centerViewNumY = 1.0f;
        param.convergeDimX = 30.0f; //spacing
        param.convergeDimY = 1000000.0f;
        param.convergeDist = 650.0f; //convergence distance
        param.displayOption = 0;
        param.viewSlant = -0.3217f;
        param.viewCountX = leiaCamera.GetViewCount();
        param.viewCountY = 1;

        Z0 = leiaCamera.ConvergenceDistance;

        B_AT_Z0 = leiaCamera.BaselineScaling;
        B_AT_Z0_LF = leiaCamera.BaselineScaling * .25f;
        B_AT_Z0_Stereo = leiaCamera.BaselineScaling * 2f;

        UpdateCameraConnectedStatus();
    }

    void UpdateCameraConnectedStatus()
    {
        _cameraConnectedPrev = _cameraConnected;
        _cameraConnected = false;
        WebCamDevice[] devices = WebCamTexture.devices;
        for (int i = 0; i < devices.Length; i++)
        {
#if UNITY_ANDROID
            // TODO: do we need to support realsense on Android?
            if (devices[i].name.Contains("Camera 1"))
#else
            if (devices[i].name.Contains("Intel(R) RealSense(TM)"))
#endif
            {
                _cameraConnected = true;
                break;
            }
        }

#if !UNITY_EDITOR || LEIA_HEADTRACKING_ENABLED_IN_EDITOR
        if (!_cameraConnected && _cameraConnectedPrev)
        {
            Debug.Log("Camera not connected! Terminating head tracking!");
            TerminateHeadTracking();
        }
        else
        if (_cameraConnected && !_cameraConnectedPrev)
        {
            InitHeadTracking();
        }

#endif

        Invoke("UpdateCameraConnectedStatus", 1f);
    }

    private void OnDisable()
    {
        //When eye tracking is disabled, reset face position to default
        faceX = 0;
        faceY = 0;
        faceZ = LeiaDisplay.Instance.GetDisplayConfig().ConvergenceDistance;
    }

    private void OnApplicationQuit()
    {
#if !UNITY_EDITOR || LEIA_HEADTRACKING_ENABLED_IN_EDITOR
        TerminateHeadTracking();
#endif
    }
    
    private void OnDestroy()
    {
#if !UNITY_EDITOR || LEIA_HEADTRACKING_ENABLED_IN_EDITOR
        TerminateHeadTracking();
#endif
    }
    
    void InitHeadTracking()
    {
        if (headTrackingEngine == null)
        {
            try
            {
                DisplayConfig displayConfig = LeiaDisplay.Instance.GetDisplayConfig();

                LeiaHeadTracking.Engine.InitArgs initArgs = new LeiaHeadTracking.Engine.InitArgs();
                initArgs.enablePolling = 1;
                initArgs.cameraWidth = displayConfig.CameraStreamParams.width;
                initArgs.cameraHeight = displayConfig.CameraStreamParams.height;
                initArgs.cameraFps = displayConfig.CameraStreamParams.fps;
                initArgs.cameraBinningFactor = displayConfig.CameraStreamParams.binningFactor;
#if DEVELOPMENT_BUILD
                initArgs.logLevel = LeiaHeadTracking.Engine.LogLevel.Trace;
#endif
                initArgs.detectorMaxNumOfFaces = 1;
                headTrackingEngine = new LeiaHeadTracking.Engine(ref initArgs, null);

                LeiaHeadTracking.Vector3 cameraPosition;
                cameraPosition.x = displayConfig.cameraCenterX;
                cameraPosition.y = displayConfig.cameraCenterY;
                cameraPosition.z = displayConfig.cameraCenterZ;
                LeiaHeadTracking.Vector3 cameraRotation;
                cameraRotation.x = displayConfig.cameraThetaX;
                cameraRotation.y = displayConfig.cameraThetaY;
                cameraRotation.z = displayConfig.cameraThetaZ;
                headTrackingEngine.SetCameraTransform(cameraPosition, cameraRotation);
                headTrackingEngine.StartTracking();
            }
            catch (LeiaHeadTracking.Engine.NativeCallFailedException e)
            {
                Debug.LogError("Failed to init head tracking: " + e.ToString());
                headTrackingEngine = null;
            }
        }
    }

    void TerminateHeadTracking()
    {
#if !UNITY_EDITOR || LEIA_HEADTRACKING_ENABLED_IN_EDITOR
        if (headTrackingEngine != null)
        {
            Debug.Log("Terminating head tracking engine");
            headTrackingEngine.Dispose();
            headTrackingEngine = null;
        }
#endif
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LeiaDisplayParameter
    {
        public int viewCountX;                         // horizontal view count for display 
        public int viewCountY;                         // vertical view count for display
        public double centerViewNumX;      //view offset -8 to +8           // center view number X
        public double centerViewNumY;                  // center view number Y
        public double convergeDimX;       //spacing 10-50            // x-dimension of eyeboxes in converge plane (unit:mm)
        public double convergeDimY;                    // y-dimension of eyeboxes in converge plane (unit:mm)
        public double convergeDist;       //convergence 500 - 1000mm             // distance of convergence plane from display plane (unit:mm)
        public double viewSlant;                       // radient of view slant 
        public int displayOption;                      // display option for view number query
    }

    enum TrackingState { FaceTracking, NotFaceTracking };

    TrackingState priorRequestedState = TrackingState.NotFaceTracking;
    TrackingState currentState = TrackingState.NotFaceTracking;
    TrackingState requestedState = TrackingState.NotFaceTracking;
    int numInRow = 0;

    void AddTestFace(float x = 0, float y = 0, float z = 800) //A useful method for adding a virtual test face, which can be used for multi-face testing when you don't have other people available to help you test
    {
        if (trackingResult.numDetectedFaces < LeiaHeadTracking.Engine.MAX_NUM_FACES)
        {
            LeiaHeadTracking.DetectedFace face = new LeiaHeadTracking.DetectedFace();
            face.pos.x = 0;
            face.pos.y = 0;
            face.pos.z = 800;
            face.vel.x = 0;
            face.vel.y = 0;
            face.vel.z = 0;
            trackingResult.detectedFaces[trackingResult.numDetectedFaces] = face;
            trackingResult.numDetectedFaces++;
        }
    }

    public enum FaceTransitionState { NoFace, FaceLocked, ReducingBaseline, SlidingCameras, IncreasingBaseline };

    public FaceTransitionState faceTransitionState = FaceTransitionState.NoFace;

    int chosenFaceIndex;
    int chosenFaceIndexPrev;
    public void UpdateFacePosition()
    {
        if (_cameraConnected)
        {
            // TODO: use timestamp to check if we received a new tracking result or the old one
            trackingResult.numDetectedFaces = 0;
            trackingResult.timestamp.ms = -1.0;
#if !UNITY_EDITOR || LEIA_HEADTRACKING_ENABLED_IN_EDITOR
            if (headTrackingEngine != null)
            {
                headTrackingEngine.GetTrackingResult(out trackingResult);
            }
#endif
            //AddTestFace();

            if (trackingResult.numDetectedFaces > 0)
            {
                requestedState = TrackingState.FaceTracking;

                DisplayConfig config = LeiaDisplay.Instance.GetDisplayConfig();
                float displayHeightMM = config.PanelResolution.y * config.DotPitchInMm.y;
                // TODO: delay should be calculated like that:
                //         delay = frame_on_display_presentation_time - trackingResult.timestamp
                double t = trackingResult.timestamp.ms;
                if (trackingResult.timestamp.ms < 0.0)
                {
                    t = Time.time * 1000;
                }
                float delay = (float)(t - old_time_stamp + time_delay);
                float Hv = Mathf.Sin(leiaCamera.FieldOfView / 2f * Mathf.Deg2Rad) * this.Z0;
                float ipd = 63;

                chosenFaceIndexPrev = chosenFaceIndex;
                chosenFaceIndex = -1;
                if (currentState == TrackingState.FaceTracking)
                {
                    chosenFaceIndex = faceChooser.ChosenFaceIndex(trackingResult);
                    LeiaHeadTracking.DetectedFace chosenFace = trackingResult.detectedFaces[chosenFaceIndex];

                    if (chosenFaceIndexPrev != chosenFaceIndex)
                    {
                        faceTransitionState = FaceTransitionState.ReducingBaseline;
                    }
                    if (faceTransitionState == FaceTransitionState.SlidingCameras
                        || faceTransitionState == FaceTransitionState.IncreasingBaseline)
                    {
                        Vector3 currentPos = new Vector3(
                            faceX,
                            faceY,
                            faceZ
                        );

                        Vector3 targetPos = new Vector3(
                            chosenFace.pos.x,
                            chosenFace.pos.y,
                            chosenFace.pos.z
                        );

                        faceX += (targetPos.x - currentPos.x) * Mathf.Min((Time.deltaTime * 5f), 1f);
                        faceY += (targetPos.y - currentPos.y) * Mathf.Min((Time.deltaTime * 5f), 1f);
                        faceZ += (targetPos.z - currentPos.z) * Mathf.Min((Time.deltaTime * 5f), 1f);
                        predictedFaceX = faceX;
                        predictedFaceY = faceY;
                        predictedFaceZ = faceZ;
                        if (faceTransitionState == FaceTransitionState.SlidingCameras
                            && Vector3.Distance(currentPos, targetPos) < 10f)
                        {
                            faceTransitionState = FaceTransitionState.IncreasingBaseline;
                        }
                    }

                    if (faceTransitionState == FaceTransitionState.FaceLocked)
                    {
                        faceX = chosenFace.pos.x;
                        faceY = chosenFace.pos.y;
                        faceZ = chosenFace.pos.z;
                        predictedFaceX = chosenFace.pos.x + chosenFace.vel.x * delay;
                        predictedFaceY = chosenFace.pos.y + chosenFace.vel.y * delay;
                        predictedFaceZ = chosenFace.pos.z + chosenFace.vel.z * delay;
                    }
                }

                // Store old timestamp
                old_time_stamp = t;

                float m = 0.01136f;
                float b = 2.72727f;

                float d = faceZ * (leiaVirtualDisplay.height) / displayHeightMM;

            }
            else
            {
                requestedState = TrackingState.NotFaceTracking;
            }

            if (currentState != requestedState)
            {
                if (requestedState == priorRequestedState)
                {
                    numInRow++;
                    if (numInRow > 20)
                    {
                        currentState = requestedState;
                        numInRow = 0;
                    }
                }
            }
            priorRequestedState = requestedState;
        }
    }

    void Update()
    {
        UpdateFacePosition();

        //debugLabel.text = "faceTransitionState = " + faceTransitionState.ToString();

        debugLabel.text =
            "faceX: " + faceX + "\n" +
            "faceY: " + faceY + "\n" +
            "faceZ: " + faceZ + "\n" +
            "chosenFaceIndex = " + chosenFaceIndex + "\n"
            + "numDetectedFaces = " + trackingResult.numDetectedFaces + "\n"
            + "baseline = " + leiaCamera.BaselineScaling + "\n"
            + "getPeelOffsetForShader = " + LeiaDisplay.Instance.getPeelOffsetForShader()+ "\n"
            + "getPeelOffsetForCameraShift = " + LeiaDisplay.Instance.getPeelOffsetForCameraShift()+ "\n"
            + "fps: " + 1.0f / Time.deltaTime + "\n";
        /*+
            "localPosition.z = " + shifter.transform.localPosition.z + "\n" +
            "config.PanelResolution.y = " + config.PanelResolution.y + "\n" +
            "config.DotPitchInMm.y = " + config.DotPitchInMm.y + "\n" +
            "d = " + d + "\n" +
            "d = " + d + "\n" +
            "displayHeightMM = " + displayHeightMM + "\n" +
            "Screen.height = " + Screen.height + "\n" +
            "FOV = " + leiaCamera.FieldOfView + "\n"
            ;
        */
    }

    public int NumFaces
    {
        get
        {
            return trackingResult.numDetectedFaces;
        }
    }
}
