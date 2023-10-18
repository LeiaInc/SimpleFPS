using SimulatedReality;
using SRUnity;
using LeiaLoft;
using System;
using UnityEngine;

public class RenderTrackingDevice: Singleton<RenderTrackingDevice>
{
    #region Core
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
    SimulatedRealityCamera SRCam;
#elif !UNITY_EDITOR && PLATFORM_ANDROID
    private class CNSDKHolder
    {
        private static bool _isInitialized = false;
        private static Leia.SDK _cnsdk = null;
        private static Leia.Interlacer _interlacer = null;
        public static Leia.SDK Get()
        {
            return _cnsdk;
        }
        public static Leia.Interlacer GetInterlacer()
        {
            return _interlacer;
        }
        public static void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            Debug.Log("CNSDKHolder::Initialize()");
            _isInitialized = true;
            Debug.Log("CNSDKHolder::Initialize::Creating new Leia.SDKConfig");
            Leia.SDKConfig cnsdkConfig = new Leia.SDKConfig();
            Debug.Log("CNSDKHolder::Initialize::Setting Log Level to Off");
            cnsdkConfig.SetPlatformLogLevel(Leia.LogLevel.Off); // Leia.LogLevel.Off
            Debug.Log("CNSDKHolder::Initialize::Setting FaceTracking To true");
            cnsdkConfig.SetFaceTrackingEnable(true);
            Debug.Log("CNSDKHolder::Initialize::Setting new Leia.SDK");
            _cnsdk = new Leia.SDK(cnsdkConfig);
            Debug.Log("CNSDKHolder::Initialize::Disposing cnsdkConfig");
            cnsdkConfig.Dispose();
            Debug.Log("CNSDKHolder::Initialize::Wait for LeiaSDK Initialization");
            // Wait for LeiaSDK Initialization
            // TODO: convert to coroutine
            //yield return new WaitUntil(() => _cnsdk.IsInitialized());
            while (!_cnsdk.IsInitialized()) {}
            Debug.Log("CNSDKHolder::Initialize::_cnsdk is initialized");
            try
            {
                Debug.Log("CNSDKHolder::Initialize::setting new Leia.Interlacer(_cnsdk)");
                _interlacer = new Leia.Interlacer(_cnsdk);
                Leia.Interlacer.Config interlacerConfig = _interlacer.GetConfig();
                interlacerConfig.showGui = false;
                _interlacer.SetConfig(interlacerConfig);
                Debug.Log("CNSDKHolder::Initialize::_interlacer: " + _interlacer);
            }
            catch (Exception e)
            {
                Debug.Log("Interlacer error: " + e.ToString());
            }
        }
    }
    public Leia.SDK CNSDK { get { return CNSDKHolder.Get(); } }
    public Leia.Interlacer Interlacer { get { return CNSDKHolder.GetInterlacer(); } }
    public Leia.SDK.ConfigHolder sdkConfig;
    private Texture[] inputViews = new Texture[2];
#else
#endif

    public void Initialize()
    {
        Debug.Log("LeiaRenderDevice::Initialize()");
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        string srCameraObjectName = "SRCamera";
        GameObject srCameraObject = GameObject.Find(srCameraObjectName);
        if(srCameraObject == null)
        {
            srCameraObject = new GameObject(srCameraObjectName);
            srCameraObject.transform.position = Vector3.zero;
        }
        if (srCameraObject.GetComponent<SimulatedRealityCamera>() == null)
        {
            SRCam = srCameraObject.gameObject.AddComponent<SimulatedRealityCamera>();
        }
#elif !UNITY_EDITOR && PLATFORM_ANDROID
        Debug.Log("LeiaRenderDevice::Initialize::Calling CNSDKHolder.Initialize..");
        CNSDKHolder.Initialize();
        Debug.Log("LeiaRenderDevice::Initialize::Applying EyeTracking");
        EyeTrackingAndroid.Instance = GetComponent<EyeTrackingAndroid>();
        if(EyeTrackingAndroid.Instance == null)
        {
            EyeTrackingAndroid.Instance = gameObject.AddComponent<EyeTrackingAndroid>();
        }
        EyeTrackingAndroid.Instance.enabled = true;
#else
#endif
    }

    #endregion
    #region Render

    public void Render(LeiaDisplay leiaDisplay, ref RenderTexture outputTexture)
    {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
#elif !UNITY_EDITOR && PLATFORM_ANDROID
        Leia.Interlacer interlacer = RenderTrackingDevice.Instance.Interlacer;
        if (interlacer != null && leiaDisplay.GetViewCount() == 2)
        {
            interlacer.SetLayerCount(1);
            inputViews[0] = leiaDisplay.GetEyeCamera(0).targetTexture;
            inputViews[1] = leiaDisplay.GetEyeCamera(1).targetTexture;
            interlacer.SetInputViews(inputViews, 0);
            interlacer.SetOutput(outputTexture);
            interlacer.Render();
        }
        else if (leiaDisplay.GetViewCount() == 1)
        {
            Graphics.Blit(leiaDisplay.GetEyeCamera(0).targetTexture, outputTexture);
        }
#else
#endif
    }

    #endregion
    #region Tracking

    public int NumFaces
    {
        get
        {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
            return SRUnity.SrRenderModeHint.ShouldRender3D() ? 1 : 0;
#elif !UNITY_EDITOR && PLATFORM_ANDROID
            return EyeTrackingAndroid.Instance.NumFaces;
#else
#endif
            return 0;
        }
    }
    public void SetTrackerEnabled(bool trackerEnabled)
    {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        SRUnity.SrRenderModeHint renderHint = new SrRenderModeHint();
        if (trackerEnabled)
        {
            renderHint.Prefer3D();
        }
        else
        {
            renderHint.Prefer2D();
        }
#elif !UNITY_EDITOR && PLATFORM_ANDROID
        EyeTrackingAndroid.Instance.enabled = trackerEnabled;
#else
#endif
    }
    public void UpdateFacePosition()
    {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
#elif !UNITY_EDITOR && PLATFORM_ANDROID
        EyeTrackingAndroid.Instance.UpdateFacePosition();
#else
#endif
    }
    public Vector3 GetPredictedFacePosition()
    {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        return SRUnity.SRHead.Instance.GetHeadPosition(ISRSettingsInterface.GetProjectSettings(null));
#elif !UNITY_EDITOR && PLATFORM_ANDROID
        return EyeTrackingAndroid.Instance.GetPredictedFacePosition();
#else
        return Vector3.zero;
#endif
    }
    public Vector3 GetNonPredictedFacePosition()
    {

#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        return SRUnity.SRHead.Instance.GetHeadPosition(ISRSettingsInterface.GetProjectSettings(null));
#elif !UNITY_EDITOR && PLATFORM_ANDROID
        return EyeTrackingAndroid.Instance.GetNonPredictedFacePosition();
#else
        return Vector3.zero;
#endif
    }

    #endregion
    #region 2D3D

    public void Set2D3DMode(bool is3D)
    {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        SRUnity.SrRenderModeHint renderHint = new SrRenderModeHint();
        if (is3D)
        {
            renderHint.BecomeIndifferent();
        }
        else
        {
            renderHint.Force2D();
        }
#elif !UNITY_EDITOR && PLATFORM_ANDROID
        if (RenderTrackingDevice.Instance.CNSDK != null)
        {
            RenderTrackingDevice.Instance.CNSDK.Set2D3D(is3D);
        }
#else
#endif
    }
    public bool Get2D3DMode()
    {
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        return SRUnity.SrRenderModeHint.ShouldRender3D();
#elif !UNITY_EDITOR && PLATFORM_ANDROID
        bool is2D3DOn = true;
        RenderTrackingDevice.Instance.CNSDK.Get2D3D(out is2D3DOn);
        return is2D3DOn;
#else
#endif
        return true;
    }
    #endregion
    #region DisplayConfig

    public Vector2Int GetDevicePanelResolution()
    {
        Vector2Int panelResolution = new Vector2Int(2560, 1600);  //LP2 defaults
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        panelResolution = SRUnity.SRCore.Instance.getResolution();
#elif !UNITY_EDITOR && PLATFORM_ANDROID

        if (sdkConfig != null)
        {
            panelResolution.x = sdkConfig.config.panelResolution[0];
            panelResolution.y = sdkConfig.config.panelResolution[1];
        }
#else
#endif
        return panelResolution;
    }

    public Vector2Int GetDeviceViewResolution()
    {
        Vector2Int viewResolution = new Vector2Int(1280, 800);  //LP2 defaults
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        viewResolution = SRUnity.SRCore.Instance.getResolution();
#elif !UNITY_EDITOR && PLATFORM_ANDROID

        if (sdkConfig != null)
        {
            viewResolution.x = sdkConfig.config.viewResolution[0];
            viewResolution.y = sdkConfig.config.viewResolution[1];
        }
#else
#endif
        return viewResolution;
    }
    public float GetDeviceSystemDisparityPixels()
    {
        float systemDisparityPixels = 4.0f;
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        //ToDo: find SR disparity pixels
#elif !UNITY_EDITOR && PLATFORM_ANDROID
        if (sdkConfig != null)
        {
            systemDisparityPixels = sdkConfig.config.systemDisparityPixels;
        }
#else
#endif
        return systemDisparityPixels;
    }
    public Vector2 GetDeviceDotPitchInMM()
    {
        Vector2 dotPitchInMM = new Vector2(0.10389f, 0.104375f); //LP2 defaults
#if !UNITY_EDITOR && PLATFORM_STANDALONE_WIN
        float dotPitch = SRUnity.SRCore.Instance.getDotPitch();
        dotPitchInMM = new Vector2(dotPitch,dotPitch);
#elif !UNITY_EDITOR && PLATFORM_ANDROID
        if (sdkConfig != null)
        {
            dotPitchInMM.x = sdkConfig.config.dotPitchInMM[0];
            dotPitchInMM.y = sdkConfig.config.dotPitchInMM[1];
        }
#else
#endif
        return dotPitchInMM;
    }

    #endregion

}
