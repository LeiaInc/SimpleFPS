/****************************************************************
*
* Copyright 2019 © Leia Inc.  All rights reserved.
*
* NOTICE:  All information contained herein is, and remains
* the property of Leia Inc. and its suppliers, if any.  The
* intellectual and technical concepts contained herein are
* proprietary to Leia Inc. and its suppliers and may be covered
* by U.S. and Foreign Patents, patents in process, and are
* protected by trade secret or copyright law.  Dissemination of
* this information or reproduction of this materials strictly
* forbidden unless prior written permission is obtained from
* Leia Inc.
*
****************************************************************
*/
using System.Collections.Generic;
using UnityEngine;

namespace LeiaLoft
{
    [ExecuteInEditMode]
    public class LeiaDisplay : MonoBehaviour
    {
        #region Private_Variables

        private LightfieldMode _desiredLightfieldMode;
        private int _numViewsX = 2;
        private int _numViewsY = 1;

        private Vector3 viewerPositionNonPredicted = new Vector3(0, 0, 535.964f);
        private Vector3 viewerPositionPredicted = new Vector3(0, 0, 535.964f);
        private Vector2[] _initialViewOffsets;

        private RenderTexture _interlacedTexture;
        private Material _editorPreviewMaterial;
        private string _editorPreviewShaderName { get { return "EditorPreview"; } }

        private bool _initialized = false;

        #region Private_Variables_LeiaDisplayNew
        private bool CompletedFirstUpdate;
        #endregion
        #endregion
        #region Public_Variables

        public enum LightfieldMode { On, Off };
        public LightfieldMode DesiredLightfieldMode
        {
            get
            {
                return _desiredLightfieldMode;
            }
            set
            {
                if (_desiredLightfieldMode != value)
                {
                    _desiredLightfieldMode = value;
                    Request2D3DUpdate();
                }
            }
        }
        [SerializeField]
        public enum EditorPreviewMode { SideBySide, Interlaced };
        public EditorPreviewMode DesiredPreviewMode;

        public bool CameraShiftEnabled;
        public event System.Action StateChanged = delegate { };

        #region Public_Variables_LeiaDisplay_New

        //Variables Developer can Modify
        public float VirtualHeight = 10;

        //[Range(.1f, 1)]
        public float ParallaxFactor = 1.0f; //1 is realistic parallax

        [Range(.1f, 1)]
        public float DepthFactor = 1.0f; //1 is realistic depth

        public bool DrawCameraBounds;

        //Real Display Dimensions
        [HideInInspector]
        public float WidthMM = 266; //lumepad2
        [HideInInspector]
        public float HeightMM = 168;
        [HideInInspector]
        public float ViewingDistanceMM = 450; //this will be pulled from config

        [Range(.1f, 20)]
        public float MaxDisparity = 10f; //10% of screen width

        public bool UseCameraClippingPlanes;

        //Virtual Display Dimensions
        [HideInInspector]
        public float VirtualWidth
        {
            get
            {
                return VirtualHeight * (WidthMM / HeightMM);
            }
        }

        //[HideInInspector]
        public Camera DriverCamera;

        public float MMToVirtual
        {
            get
            {
                return VirtualHeight / HeightMM;
            }
        }

        public float VirtualToMM
        {
            get
            {
                return HeightMM / VirtualHeight;
            }
        }

        public enum ControlMode { DisplayDriven, CameraDriven };
        [HideInInspector]
        public ControlMode mode;

        public Head ViewersHead;
        public Camera HeadCamera
        {
            get
            {
                return ViewersHead.headcamera;
            }
        }

        public float ConvergenceDistance
        {
            get
            {
                if (mode == ControlMode.CameraDriven)
                {
                    return transform.localPosition.z;
                }
                else
                {
                    return ViewersHead.transform.localPosition.z;
                }
            }
            set
            {
                if (mode == ControlMode.CameraDriven)
                {
                    transform.localPosition = new Vector3(
                    transform.localPosition.x,
                    transform.localPosition.y,
                    value
                    );
                }
                else
                {
                    /*
                    ViewersHead.transform.localPosition = new Vector3(
                    ViewersHead.transform.localPosition.x,
                    ViewersHead.transform.localPosition.y,
                    value
                    );
                    */
                }
            }
        }
        #endregion
        #endregion
        #region Unity_Functions

        private void OnEnable()
        {
            if (!Application.isPlaying)
                return;

            Debug.Log("LeiaDisplay::OnEnable()");
            Debug.Log("LeiaDisplay::OnEnable::Checking is RenderTrackingDevice is attached");
            RenderTrackingDevice.Instance = GetComponent<RenderTrackingDevice>();
            if (RenderTrackingDevice.Instance == null)
            {
                Debug.Log("LeiaDisplay::OnEnable::RenderTrackingDevice is not attached. Adding a new RenderTrackingDevice component to LeiaDisplay");
                RenderTrackingDevice.Instance = gameObject.AddComponent<RenderTrackingDevice>();
            }
            Debug.Log("LeiaDisplay::OnEnable::Calling RenderTrackingDevice.Instance.Initialize..");
            RenderTrackingDevice.Instance.Initialize();
            if (!_initialized)
            {
                DontDestroyOnLoad(gameObject);
                Debug.Log("LeiaDisplay::OnEnable::Calling Update UpdateInitialViewOffsets()");
                UpdateInitialViewOffsets();
                _interlacedTexture = new RenderTexture(RenderTrackingDevice.Instance.GetDevicePanelResolution().x, RenderTrackingDevice.Instance.GetDevicePanelResolution().y, 0);
                _interlacedTexture.Create();
                _initialized = true;
            }

#if UNITY_EDITOR
            EnsureEditorPreivewMaterialInitialized();
#endif
            Request2D3DUpdate();
        }


        private void OnResume()
        {
#if !UNITY_EDITOR
            if(DesiredLightfieldMode == LightfieldMode.On)
            {
                Set2D3DMode(3);
            }
#endif
        }

        private void OnPause()
        {
#if !UNITY_EDITOR
            if(Get2D3DMode() != 2)
            {
                Set2D3DMode(2);
            }
#endif
        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                OnResume();
            }
            else
            {
                OnPause();
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
        }

        private void OnApplicationQuit()
        {
            Set2D3DMode(2);
        }
        #region Unity_Functions_LeiaDisplayNew
        public void Update()
        {
            if (Application.isPlaying)
            {
                UpdateViews(); // Joe: questioning the need for this here
            }

            if (!CompletedFirstUpdate)
            {
                OnComponentAddedToGameObject();
                CompletedFirstUpdate = true;
                return;
            }

            if (UseCameraClippingPlanes)
            {
                if (DriverCamera != null)
                {
                    ViewersHead.headcamera.nearClipPlane = this.DriverCamera.nearClipPlane;
                    ViewersHead.headcamera.farClipPlane = this.DriverCamera.farClipPlane;
                }
            }
            else
            {
                ViewersHead.headcamera.nearClipPlane = 1f / ((5f / 4f) * ParallaxFactor * VirtualToMM * (1f / ViewersHead.HeadPositionMM.z + 1f / 1000f));
                ViewersHead.headcamera.farClipPlane = 1f / ((5f / 6f) * ParallaxFactor * VirtualToMM * (1f / ViewersHead.HeadPositionMM.z - 1f / 1000f));
                ViewersHead.Update();
            }

            if (mode == ControlMode.CameraDriven)
            {
                UpdateVirtualDisplayFromCamera();
            }
            else
            {
                //Display centric

                UpdateCameraFromVirtualDisplay();
            }
        }
        private void OnDrawGizmos()
        {
            Vector3 TopLeftCorner = new Vector3(
                    VirtualWidth / 2f,
                    VirtualHeight / 2f,
                    0
                    );

            Vector3 TopRightCorner = new Vector3(
                    -VirtualWidth / 2f,
                    VirtualHeight / 2f,
                    0
                    );

            Vector3 BottomLeftCorner = new Vector3(
                    VirtualWidth / 2f,
                    -VirtualHeight / 2f,
                    0
                    );

            Vector3 BottomRightCorner = new Vector3(
                    -VirtualWidth / 2f,
                    -VirtualHeight / 2f,
                    0
                    );

            TopLeftCorner = transform.position + transform.rotation * TopLeftCorner;
            TopRightCorner = transform.position + transform.rotation * TopRightCorner;
            BottomLeftCorner = transform.position + transform.rotation * BottomLeftCorner;
            BottomRightCorner = transform.position + transform.rotation * BottomRightCorner;
            if (enabled)
            {
                Gizmos.DrawLine(TopRightCorner, TopLeftCorner);
                Gizmos.DrawLine(BottomLeftCorner, BottomRightCorner);
                Gizmos.DrawLine(TopRightCorner, BottomRightCorner);
                Gizmos.DrawLine(TopLeftCorner, BottomLeftCorner);
            }
        }

        private void OnDestroy()
        {
            if (ViewersHead != null)
            {
                DestroyImmediate(ViewersHead.gameObject);
            }
        }
        #endregion
        #endregion
        #region Init_Functions
        #region Init_Functions_LeiaDisplayNew

        public void InitLeiaDisplay(List<Vector2> ViewConfig)
        {
            GameObject newHeadGameObject = new GameObject("Head (Camera For 2D)");
            newHeadGameObject.transform.parent = transform;
            newHeadGameObject.transform.localPosition = new Vector3(
                0,
                0,
                ViewingDistanceMM * VirtualHeight / (HeightMM * ParallaxFactor)
                );
            if (mode == ControlMode.CameraDriven)
            {
                UpdateVirtualDisplayFromCamera();
            }
            ViewersHead = newHeadGameObject.AddComponent<Head>();
            ViewersHead.InitHead(ViewConfig, this);
        }
        void OnComponentAddedToGameObject()
        {
            DriverCamera = GetComponent<Camera>();
            Camera camParent = null;
            if (transform.parent != null)
            {
                camParent = transform.parent.GetComponent<Camera>();
            }

            if (DriverCamera != null) //If LeiaDisplay component was just added to a camera game object do this
            {
                Debug.Log("Gets here");
                mode = ControlMode.CameraDriven;
                ParallaxFactor = (ViewingDistanceMM * VirtualHeight) / (HeightMM * transform.localPosition.z);
                GameObject leiaDisplayGameObject = new GameObject("LeiaDisplay");
                leiaDisplayGameObject.transform.parent = transform;
                LeiaDisplay newLeiaDisplay = leiaDisplayGameObject.AddComponent<LeiaDisplay>();
                newLeiaDisplay.UseCameraClippingPlanes = true;
                leiaDisplayGameObject.transform.localPosition = new Vector3(
                    0,
                    0,
                    2f / (1f / DriverCamera.nearClipPlane + 1f / DriverCamera.farClipPlane)
                );

                DestroyImmediate(this);
            }
            else
            {
                if (camParent != null)
                {
                    DriverCamera = camParent;
                    List<Vector2> ViewConfig = new List<Vector2>
                {
                    //new Vector2(-1.5f, 0),
                    new Vector2(-.5f, 0),
                    new Vector2(.5f, 0)//,
                    //new Vector2(1.5f, 0)
                };
                    mode = ControlMode.CameraDriven;
                    InitLeiaDisplay(ViewConfig);
                }
                else //If leiadisplay added to a blank object do this
                {
                    MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();

                    if (meshRenderer != null)
                    {
                        // Get the bounds of the object
                        Bounds bounds = meshRenderer.bounds;

                        // Calculate and print the height of the object
                        float height = bounds.size.y;

                        this.VirtualHeight = height * 4f;
                    }

                    List<Vector2> ViewConfig = new List<Vector2>
                {
                    //new Vector2(-1.5f, 0),
                    new Vector2(-.5f, 0),
                    new Vector2(.5f, 0) //,
                    //new Vector2(1.5f, 0)
                };
                    mode = ControlMode.DisplayDriven;
                    InitLeiaDisplay(ViewConfig);
                }
            }
        }
        #endregion
        #endregion
        #region Head_Functions
        #region Head_Functions_LeiaDisplayNew

        public int GetViewCount()
        {
            return ViewersHead.ViewConfig.Count;
        }

        public Camera GetEyeCamera(int index)
        {
            return ViewersHead.eyes[index].eyecamera;
        }

        public Eye GetEye(int index)
        {
            return ViewersHead.eyes[index];
        }

        void UpdateCameraFromVirtualDisplay()
        {

        }

        void UpdateVirtualDisplayFromCamera()
        {
            VirtualHeight = 2f * Mathf.Tan(Mathf.Deg2Rad * (DriverCamera.fieldOfView / 2f)) * transform.localPosition.z;
            ParallaxFactor = (ViewingDistanceMM * VirtualHeight) / (HeightMM * transform.localPosition.z);
            if (ViewersHead != null)
            {
                this.ViewersHead.Update();
            }
        }

        public Vector3 RealToVirtualCenterFacePosition(Vector3 FacePositionMM)
        {
            return Vector3.Scale(FacePositionMM, new Vector3(1f, 1f, -1f / ParallaxFactor)) * VirtualHeight / HeightMM;
        }
        #endregion
        #endregion
        #region Render_Functions

        public void RenderImage()
        {
            RenderTrackingDevice.Instance.UpdateFacePosition();
            viewerPositionPredicted = RenderTrackingDevice.Instance.GetPredictedFacePosition();
            viewerPositionNonPredicted = RenderTrackingDevice.Instance.GetNonPredictedFacePosition();

            if (viewerPositionNonPredicted == Vector3.zero)
            {
                ViewersHead.HeadPositionMM = new Vector3(0, 0, ViewingDistanceMM);
            }
            else
            {
                ViewersHead.HeadPositionMM = viewerPositionNonPredicted;
            }

#if UNITY_EDITOR
            if (DesiredLightfieldMode == LightfieldMode.On && GetViewCount() == 2)
            {
                EnsureEditorPreivewMaterialInitialized();
                SetEditorPreviewProperties();
                Graphics.Blit(Texture2D.whiteTexture, _interlacedTexture, _editorPreviewMaterial);
            }
            else
            {
                Graphics.Blit(GetEyeCamera(0).targetTexture, _interlacedTexture);
            }
#elif UNITY_ANDROID && !UNITY_EDITOR
            RenderTrackingDevice.Instance.Render(this, ref _interlacedTexture);
#endif
            Graphics.Blit(_interlacedTexture, Camera.current.activeTexture);
        }

        public void UpdateViews()
        {
            int viewResolutionX = RenderTrackingDevice.Instance.GetDeviceViewResolution().x;
            int viewResolutionY = RenderTrackingDevice.Instance.GetDeviceViewResolution().y;

            for (int ix = 0; ix < GetViewCount(); ix++)
            {
                Eye view = GetEye(ix);

                string viewIdStr = string.Format("view_{0}_{1}", ix, 0);
                view.SetTextureParams(viewResolutionX, viewResolutionY, viewIdStr);
            }
        }

        private void UpdateInitialViewOffsets()
        {
            _initialViewOffsets = new Vector2[_numViewsX * _numViewsY];

            // Calculate initial offsets to center the view grid
            float baseOffsetX = -0.5f * (_numViewsX - 1.0f);
            float baseOffsetY = -0.5f * (_numViewsY - 1.0f);

            for (int viewY = 0; viewY < _numViewsY; viewY++)
            {
                for (int viewX = 0; viewX < _numViewsX; viewX++)
                {
                    // Calculate view offset based on base offsets and view indices
                    float viewOffsetX = baseOffsetX + viewX;
                    float viewOffsetY = baseOffsetY + viewY;

                    _initialViewOffsets[viewX + viewY * _numViewsX] = new Vector2(viewOffsetX, viewOffsetY);
                }
            }
        }

        private float GetInitialViewOffsetX(int viewX, int viewY)
        {
            return _initialViewOffsets[viewX + viewY * _numViewsX].x;
        }

        private float GetInitialViewOffsetY(int viewX, int viewY)
        {
            return _initialViewOffsets[viewX + viewY * _numViewsX].y;
        }
        public static float GetViewportAspectFor(Camera renderingCamera)
        {
            return renderingCamera.pixelRect.width * 1.0f / renderingCamera.pixelRect.height;
        }

        public static Matrix4x4 GetConvergedProjectionMatrixForPosition(Camera Camera, Vector3 convergencePoint)
        {
            Matrix4x4 m = Matrix4x4.zero;

            Vector3 cameraToConvergencePoint = convergencePoint - Camera.transform.position;

            float far = Camera.farClipPlane;
            float near = Camera.nearClipPlane;

            // posX and posY are the camera-axis-aligned translations off of "root camera" position
            float posX = -1 * Vector3.Dot(cameraToConvergencePoint, Camera.transform.right);
            float posY = -1 * Vector3.Dot(cameraToConvergencePoint, Camera.transform.up);

            // this is really posZ. it is better if posZ is positive-signed
            float ConvergenceDistance = Mathf.Max(Vector3.Dot(cameraToConvergencePoint, Camera.transform.forward), 1E-5f);

            if (Camera.orthographic)
            {
                // calculate the halfSizeX and halfSizeY values that we need for orthographic cameras

                float halfSizeX = Camera.orthographicSize * GetViewportAspectFor(Camera);
                float halfSizeY = Camera.orthographicSize;

                // orthographic

                // row 0
                m[0, 0] = 1.0f / halfSizeX;
                m[0, 1] = 0.0f;
                m[0, 2] = -posX / (halfSizeX * ConvergenceDistance);
                m[0, 3] = 0.0f;

                // row 1
                m[1, 0] = 0.0f;
                m[1, 1] = 1.0f / halfSizeY;
                m[1, 2] = -posY / (halfSizeY * ConvergenceDistance);
                m[1, 3] = 0.0f;

                // row 2
                m[2, 0] = 0.0f;
                m[2, 1] = 0.0f;
                m[2, 2] = -2.0f / (far - near);
                m[2, 3] = -(far + near) / (far - near);

                // row 3
                m[3, 0] = 0.0f;
                m[3, 1] = 0.0f;
                m[3, 2] = 0.0f;
                m[3, 3] = 1.0f;
            }
            else
            {
                // calculate the halfSizeX and halfSizeY values for perspective DriverCamera that we would have gotten if we had used new CameraCalculatedParams.
                // we don't need "f" (disparity per camera vertical pixel count) or EmissionRescalingFactor
                const float minAspect = 1E-5f;
                float aspect = Mathf.Max(GetViewportAspectFor(Camera), minAspect);
                float halfSizeY = ConvergenceDistance * Mathf.Tan(Camera.fieldOfView * Mathf.PI / 360.0f);
                float halfSizeX = aspect * halfSizeY;

                // perspective

                // row 0
                m[0, 0] = ConvergenceDistance / halfSizeX;
                m[0, 1] = 0.0f;
                m[0, 2] = -posX / halfSizeX;
                m[0, 3] = 0.0f;

                // row 1
                m[1, 0] = 0.0f;
                m[1, 1] = ConvergenceDistance / halfSizeY;
                m[1, 2] = -posY / halfSizeY;
                m[1, 3] = 0.0f;

                // row 2
                m[2, 0] = 0.0f;
                m[2, 1] = 0.0f;
                m[2, 2] = -(far + near) / (far - near);
                m[2, 3] = -2.0f * far * near / (far - near);

                // row 3
                m[3, 0] = 0.0f;
                m[3, 1] = 0.0f;
                m[3, 2] = -1.0f;
                m[3, 3] = 0.0f;
            }
            return m;
        }

        #region Render_Functions_LeiaDisplayNew

        public Matrix4x4 GetProjectionMatrixForCamera(Camera camera, Vector3 offset, bool isEye)
        {
            float W = VirtualWidth;
            float H = VirtualHeight;

            Vector3 cameraPositionRelativeToDisplay = camera.transform.InverseTransformPoint(transform.position);

            Vector3 cameraRelative = -cameraPositionRelativeToDisplay; //camera.transform.localPosition + offset;

            float xc = cameraRelative.x;
            float yc = cameraRelative.y;
            float zc = cameraRelative.z;

            float r = -(W / 2 - xc) / zc; //minus sign is to make zc positive
            float l = -(-W / 2 - xc) / zc;
            float t = -(H / 2 - yc) / zc;
            float b = -(-H / 2 - yc) / zc;

            float far = camera.farClipPlane;
            float near = camera.nearClipPlane;

            Matrix4x4 p = new Matrix4x4();

            p.m00 = 2 / (r - l);
            p.m11 = 2 / (t - b);
            p.m02 = (r + l) / (r - l);
            p.m12 = (t + b) / (t - b);
            p.m22 = -(far + near) / (far - near);
            p.m32 = -1;
            p.m23 = -(2 * far * near) / (far - near);

            // row 1
            p.m10 = 0.0f;
            p.m13 = 0.0f;

            // row 2
            p.m20 = 0.0f;
            p.m21 = 0.0f;

            // row 3
            p.m30 = 0.0f;
            p.m31 = 0.0f;
            p.m33 = 0.0f;

            return p;
        }

        public void DrawFrustum(Transform camera)
        {
            if (camera == null)
            {
                return;
            }

            Vector3 TopLeftCorner = new Vector3(
                    VirtualWidth / 2f,
                    VirtualHeight / 2f,
                    0
                    );

            Vector3 TopRightCorner = new Vector3(
                    -VirtualWidth / 2f,
                    VirtualHeight / 2f,
                    0
                    );

            Vector3 BottomLeftCorner = new Vector3(
                    VirtualWidth / 2f,
                    -VirtualHeight / 2f,
                    0
                    );

            Vector3 BottomRightCorner = new Vector3(
                    -VirtualWidth / 2f,
                    -VirtualHeight / 2f,
                    0
                    );

            TopLeftCorner = transform.position + transform.rotation * TopLeftCorner;
            TopRightCorner = transform.position + transform.rotation * TopRightCorner;
            BottomLeftCorner = transform.position + transform.rotation * BottomLeftCorner;
            BottomRightCorner = transform.position + transform.rotation * BottomRightCorner;

            Gizmos.DrawLine(camera.position, TopLeftCorner);
            Gizmos.DrawLine(camera.position, TopRightCorner);
            Gizmos.DrawLine(camera.position, BottomLeftCorner);
            Gizmos.DrawLine(camera.position, BottomRightCorner);
        }

        #endregion
        #endregion
        #region 2D_3D
        public int Get2D3DMode()
        {
            return RenderTrackingDevice.Instance.Get2D3DMode() ? 3 : 2;
        }

        public void Set2D3DMode(int modeId)
        {
            RenderTrackingDevice.Instance.Set2D3DMode(modeId == 3);
        }

        private void Request2D3DUpdate()
        {
            if (this.DesiredLightfieldMode == LightfieldMode.On)
            {
                Set2D3DMode(3);
                _numViewsX = 2;
            }
            else
            {
                Set2D3DMode(2);
                _numViewsX = 1;
            }
        }

        #endregion
        #region Device_Values

        public Vector2Int GetDeviceViewResolution()
        {
            if (RenderTrackingDevice.Instance != null)
            {
                return RenderTrackingDevice.Instance.GetDeviceViewResolution();
            }
            return new Vector2Int(1280, 800);
        }
        public float GetDeviceSystemDisparityPixels()
        {
            if (RenderTrackingDevice.Instance != null)
            {
                return RenderTrackingDevice.Instance.GetDeviceSystemDisparityPixels();
            }
            return 4.0f;
        }

        #endregion
        #region Editor_Preview

        private void EnsureEditorPreivewMaterialInitialized()
        {
            if (_editorPreviewMaterial == null)
            {
                _editorPreviewMaterial = new Material(Resources.Load<Shader>(_editorPreviewShaderName));
            }
        }

        private void SetEditorPreviewProperties()
        {
            // default values from LumePad 2
            const int defaultPanelResolutionX = 2560;
            const int defaultPanelResolutionY = 1600;
            const float defaultNumViews = 8;
            const float defaultActSingleTapCoef = 0.12f;
            const float defaultPixelPitch = 0.10389f;
            const float defaultN = 1.6f;
            const float defaultDOverN = 0.6926f;
            const float defaultS = 10.687498f;
            const float defaultAnglePx = 0.1759291824068146f; // theta
            const float defaultNo = 4.629999965429306f; // center view number
            const float defaultPOverDu = 3.0f;
            const float defaultPOverDv = 1.0f;
            const float defaultGamma = 1.99f;
            const float defaultSmooth = 0.05f;
            const float defaultOePitchX = defaultNumViews / defaultPOverDu;
            const float defaultTanSlantAngle = defaultPOverDv / defaultPOverDu;
            Vector3 defaultSubpixelCentersX = new Vector3(-0.333f, 0.0f, 0.333f);
            Vector3 defaultSubpixelCentersY = new Vector3(0.0f, 0.0f, 0.0f);
            if (GetViewCount() == 2)
            {
                _editorPreviewMaterial.SetTexture("_texture_0", GetEyeCamera(0).targetTexture);
                _editorPreviewMaterial.SetTexture("_texture_1", GetEyeCamera(1).targetTexture);
            }

            if (DesiredPreviewMode == EditorPreviewMode.SideBySide)
            {
                _editorPreviewMaterial.EnableKeyword("SideBySide");
            }
            else if (DesiredPreviewMode == EditorPreviewMode.Interlaced)
            {
                _editorPreviewMaterial.DisableKeyword("SideBySide");

                _editorPreviewMaterial.SetInt("_width", defaultPanelResolutionX);
                _editorPreviewMaterial.SetInt("_height", defaultPanelResolutionY);
                _editorPreviewMaterial.SetFloat("_actSingleTapCoef", defaultActSingleTapCoef);
                _editorPreviewMaterial.SetFloat("_pixelPitch", defaultPixelPitch);
                _editorPreviewMaterial.SetFloat("_n", defaultN);
                _editorPreviewMaterial.SetFloat("_d_over_n", defaultDOverN);
                _editorPreviewMaterial.SetFloat("_s", defaultS);
                _editorPreviewMaterial.SetFloat("_anglePx", defaultAnglePx);
                _editorPreviewMaterial.SetFloat("_no", defaultNo);
                _editorPreviewMaterial.SetFloat("_gamma", defaultGamma);
                _editorPreviewMaterial.SetFloat("_smooth", defaultSmooth);
                _editorPreviewMaterial.SetFloat("_oePitchX", defaultOePitchX);
                _editorPreviewMaterial.SetFloat("_tanSlantAngle", defaultTanSlantAngle);
                _editorPreviewMaterial.SetFloat("_faceX", viewerPositionPredicted.x);
                _editorPreviewMaterial.SetFloat("_faceY", viewerPositionPredicted.y);
                _editorPreviewMaterial.SetFloat("_faceZ", viewerPositionPredicted.z);
                _editorPreviewMaterial.SetVector("_subpixelCentersX", defaultSubpixelCentersX);
                _editorPreviewMaterial.SetVector("_subpixelCentersY", defaultSubpixelCentersY);
            }
            _editorPreviewMaterial.SetFloat("_numViews", defaultNumViews);
        }

        #endregion
    }
}
