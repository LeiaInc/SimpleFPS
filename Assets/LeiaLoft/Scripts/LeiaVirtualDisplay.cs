using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LeiaLoft;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class LeiaVirtualDisplay : MonoBehaviour
{
    BlinkTrackingUnityPlugin _blink;
    BlinkTrackingUnityPlugin blink
    {
        get
        {
            if (!_blink)
            {
                _blink = FindObjectOfType<BlinkTrackingUnityPlugin>();
            }
            return _blink;
        }
    }

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
    public Text debugText;
    public bool ShowAtRuntime;

    public float height = 5f;
    float convergenceSmoothed;
    public float _slidingScaleStereo = 7f;
    public float slidingScaleStereo
    {
        get
        {
            return _slidingScaleStereo;
        }
        set
        {
            _slidingScaleStereo = value;
        }
    }
    public float slidingScaleLF = 1f;
    public float baselineScaleStereo = 63;
    public float baselineScaleLF = 20f;
    public bool cameraZaxisMovement;

    public void SetCameraZaxisMovementEnabled(bool cameraZaxisMovement)
    {
        this.cameraZaxisMovement = cameraZaxisMovement;
    }

    public void SetSlidingScaleStereo(float newVal)
    {
        slidingScaleStereo = newVal;
    }
    public void SetSlidingScaleLF(float newVal)
    {
        slidingScaleLF = newVal;
    }
    public void SetBaselineScaleStereo(float newVal)
    {
        baselineScaleStereo = newVal;
    }
    public void SetBaselineScaleLF(float newVal)
    {
        baselineScaleLF = newVal;
    }
    public void SetHeight(float height)
    {
        this.height = height;
    }
    
    public Transform[] corners;
    public Transform[] sides;
    public Transform logo;
    
    Transform _model;
    Transform model
    {
        get
        {
            if (_model == null)
                _model = transform.Find("Model");

            return _model;
        }
    }

    public enum ControlMode { DrivesLeiaCamera, DrivenByLeiaCamera };
    public ControlMode controlMode = ControlMode.DrivesLeiaCamera;
    ControlMode controlModePrev = ControlMode.DrivesLeiaCamera;

    LeiaCamera _leiaCamera;
    LeiaCamera leiaCamera
    {
        get
        {
            if (_leiaCamera == null)
            {
                if (controlMode == ControlMode.DrivesLeiaCamera)
                {
                    _leiaCamera = GetComponentInChildren<LeiaCamera>();
                }
                else
                {
                    _leiaCamera = transform.parent.GetComponent<LeiaCamera>();
                }
            }
            return _leiaCamera;
        }
    }

    void Start()
    {
        /*
        if (LeiaDisplay.Instance != null)
        {
            DisplayConfig config = LeiaDisplay.Instance.GetDisplayConfig();
            float displayHeightMM = config.PanelResolution.y * config.DotPitchInMm.y;
            baselineScaleStereo = 63 * height / displayHeightMM;
        }*/

        UpdateDisplayGizmos();
    }

    void UpdateDisplayGizmos()
    {
        if (logo == null)
        {
            logo = model.Find("LeiaLogo");
        }

        if (sides == null || sides.Length == 0 || sides[0] == null)
        {
            sides = new Transform[4];
            sides[0] = model.Find("Side1");
            sides[1] = model.Find("Side2");
            sides[2] = model.Find("Side3");
            sides[3] = model.Find("Side4");

            if (sides[0] == null)
            {
                sides[0] = model.GetChild(4);
                sides[1] = model.GetChild(5);
                sides[2] = model.GetChild(6);
                sides[3] = model.GetChild(7);
            }
        }
        if (corners == null || corners.Length == 0 || corners[0] == null)
        {
            corners = new Transform[4];
            corners[0] = model.Find("Corner1");
            corners[1] = model.Find("Corner2");
            corners[2] = model.Find("Corner3");
            corners[3] = model.Find("Corner4");
            
            if (corners[0] == null)
            {
                corners[0] = model.GetChild(0);
                corners[1] = model.GetChild(1);
                corners[2] = model.GetChild(2);
                corners[3] = model.GetChild(3);
            }
        }

        if (Application.isPlaying)
        {
            LeiaDisplay.Instance.cameraDriven = (this.controlMode == ControlMode.DrivenByLeiaCamera);
            LeiaDisplay.Instance.enableZCameraShift = cameraZaxisMovement;
        }

        if (height < .0001f)
        {
            height = .0001f;
        }

        if (this.controlMode == ControlMode.DrivenByLeiaCamera)
        {
            height = Mathf.Tan((leiaCamera.Camera.fieldOfView / 2f) / Mathf.Rad2Deg) * (2f * leiaCamera.ConvergenceDistance);
            transform.localPosition = new Vector3(0, 0, leiaCamera.ConvergenceDistance);
        }

        if (this.controlMode == ControlMode.DrivesLeiaCamera)
        {
            if (!Input.GetMouseButton(0))
            {
                if (transform.localScale.x != 1)
                {
                    this.height *= transform.localScale.x;
                    transform.localScale = Vector3.one;
                }
                if (transform.localScale.y != 1)
                {
                    this.height *= transform.localScale.y;
                    transform.localScale = Vector3.one;
                }
                if (transform.localScale.z != 1)
                {
                    this.height *= transform.localScale.z;
                    transform.localScale = Vector3.one;
                }
            }
        }

        float length = this.height * Screen.width / Screen.height;
        float width = this.height * .05f;

        if (Application.isPlaying)
        {
            DisplayConfig config = LeiaDisplay.Instance.GetDisplayConfig();
            float displayHeightMM = config.PanelResolution.y * config.DotPitchInMm.y;

            float targetBaseline = 0;
            //Set baseline and sliding scale
            if (!LeiaDisplay.Instance.viewPeeling)
            {
                targetBaseline = baselineScaleStereo;
                this.shifter.SetSlidingScale(slidingScaleStereo);
            }
            else
            {
                targetBaseline = baselineScaleLF;
                this.shifter.SetSlidingScale(slidingScaleLF);
            }

            if (this.controlMode == ControlMode.DrivesLeiaCamera)
            {
                if (blink.faceTransitionState == BlinkTrackingUnityPlugin.FaceTransitionState.ReducingBaseline)
                {
                    leiaCamera.BaselineScaling += (0 - leiaCamera.BaselineScaling) * Mathf.Min((Time.deltaTime * 5f), 1f);
                    if (leiaCamera.BaselineScaling < 1f)
                    {
                        blink.faceTransitionState = BlinkTrackingUnityPlugin.FaceTransitionState.SlidingCameras;
                    }
                }
                else if (blink.faceTransitionState == BlinkTrackingUnityPlugin.FaceTransitionState.SlidingCameras)
                {
                    leiaCamera.BaselineScaling = 0;
                }
                else if (blink.faceTransitionState == BlinkTrackingUnityPlugin.FaceTransitionState.IncreasingBaseline)
                {
                    leiaCamera.BaselineScaling += (targetBaseline - leiaCamera.BaselineScaling) * Mathf.Min((Time.deltaTime * 5f), 1f);

                    if (Mathf.Abs(leiaCamera.BaselineScaling - targetBaseline) < targetBaseline * .1f)
                    {
                        blink.faceTransitionState = BlinkTrackingUnityPlugin.FaceTransitionState.FaceLocked;
                    }
                }
                else
                {
                    leiaCamera.BaselineScaling = targetBaseline;
                }

                //Set camera Z position based on eye tracking
                float d = blink.faceZ * (height) / displayHeightMM;
                convergenceSmoothed += (d - convergenceSmoothed) * Mathf.Min((Time.deltaTime * 15f), 1f);
                if (cameraZaxisMovement)
                {
                    //Set camera z-position
                    leiaCamera.ConvergenceDistance = convergenceSmoothed;
                }
                else
                {
                    d = 600 * (height) / displayHeightMM;
                    convergenceSmoothed = d;
                }

                leiaCamera.transform.localPosition = new Vector3(0, 0, -convergenceSmoothed);
                leiaCamera.ConvergenceDistance = convergenceSmoothed;
            }
            else
            {
                blink.faceTransitionState = BlinkTrackingUnityPlugin.FaceTransitionState.FaceLocked;
            }
        }

        if (this.controlMode == ControlMode.DrivesLeiaCamera)
        {
            //Set the camera's field of view
            leiaCamera.Camera.fieldOfView = 2f * Mathf.Atan(
                (height) /
                (2f * leiaCamera.ConvergenceDistance)
                ) * Mathf.Rad2Deg;

            if (!Application.isPlaying)
            {
                //If app not playing then reset the camera to default position
                leiaCamera.transform.localPosition = new Vector3(0, 0, -1) * this.height;
                leiaCamera.ConvergenceDistance = this.height;
            }
        }
        else
        {
            //Set the camera's field of view
            /*
            leiaCamera.Camera.fieldOfView = 2f * Mathf.Atan(
                (height) /
                (2f * leiaCamera.ConvergenceDistance - this.leiaCamera.CameraShift.z)
                ) * Mathf.Rad2Deg;
            */
        }

        //Update the virtual display model
        sides[0].localPosition = new Vector3(0, height / 2f + .5f * width, 0);
        sides[1].localPosition = new Vector3(0, -height / 2f - .5f * width, 0);
        sides[2].localPosition = new Vector3(length / 2f + .5f * width, 0, 0);
        sides[3].localPosition = new Vector3(-length / 2f - .5f * width, 0, 0);

        sides[0].localScale = new Vector3(length, width, width);
        sides[1].localScale = new Vector3(length, width, width);
        sides[2].localScale = new Vector3(width, height, width);
        sides[3].localScale = new Vector3(width, height, width);

        corners[0].localPosition = new Vector3(length / 2f + .5f * width, height / 2f + .5f * width, 0);
        corners[1].localPosition = new Vector3(-length / 2f - .5f * width, height / 2f + .5f * width, 0);
        corners[2].localPosition = new Vector3(length / 2f + .5f * width, -height / 2f - .5f * width, 0);
        corners[3].localPosition = new Vector3(-length / 2f - .5f * width, -height / 2f - .5f * width, 0);

        corners[0].localScale = new Vector3(width, width, width);
        corners[1].localScale = new Vector3(width, width, width);
        corners[2].localScale = new Vector3(width, width, width);
        corners[3].localScale = new Vector3(width, width, width);

        corners[0].localRotation = Quaternion.Euler(0, 0, 270);
        corners[1].localRotation = Quaternion.Euler(0, 0, 0);
        corners[2].localRotation = Quaternion.Euler(0, 0, 180);
        corners[3].localRotation = Quaternion.Euler(0, 0, 90);

        logo.localPosition = new Vector3(0, -height / 2f - .5f * width, -width / 2f);
        logo.localScale = new Vector3(1f, 1f, 1f) * width / 2f;

        for (int i = 0; i < sides.Length; i++)
        {
            sides[i].GetComponent<MeshRenderer>().enabled = !Application.isPlaying || ShowAtRuntime;

#if UNITY_EDITOR
            if (Selection.Contains(sides[i].gameObject))
            {
                Selection.objects = new GameObject[] { gameObject };
            }
#endif
        }
        for (int i = 0; i < corners.Length; i++)
        {
            corners[i].GetComponent<MeshRenderer>().enabled = !Application.isPlaying || ShowAtRuntime;

#if UNITY_EDITOR
            if (Selection.Contains(corners[i].gameObject))
            {
                Selection.objects = new GameObject[] { gameObject };
            }
#endif
        }

        if (Application.isPlaying)
        {
            MeshRenderer[] logoMeshRenderers = logo.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < logoMeshRenderers.Length; i++)
            {
                logoMeshRenderers[i].enabled = ShowAtRuntime;
            }
        }
#if UNITY_EDITOR
        if (Selection.Contains(logo.gameObject))
        {
            Selection.objects = new GameObject[] { gameObject };
        }
#endif
    }

    void LateUpdate()
    {
        UpdateDisplayGizmos();
        if (debugText != null && Application.isPlaying)
        {
            debugText.text = "Camera localPosition: " + this.leiaCamera.transform.localPosition + "\n"
                + "leiaCamera.CameraShift = " + this.leiaCamera.CameraShift + "\n"
                + "leiaCamera.FieldOfView = " + this.leiaCamera.FieldOfView + "\n"
                + "leiaCamera.BaselineScaling = " + this.leiaCamera.BaselineScaling + "\n"
                + "leiaCamera.ConvergenceDistance = " + this.leiaCamera.ConvergenceDistance + "\n"
                + "blink.faceZ = " + blink.faceZ
                ;
        }
    }
    /*
    void OnDrawGizmos()
    {
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.matrix = rotationMatrix;
        Gizmos.color = new Color(1, 0, 0, 1f);

        float w, l, h;

        w = Screen.width;
        l = 1;
        h = Screen.height;

        Gizmos.DrawCube(transform.position + new Vector3(0, -h / 2f, 0) * scale,
            new Vector3(w, 10, 10) * scale);
        Gizmos.DrawCube(transform.position + new Vector3(0, h / 2f, 0) * scale,
            new Vector3(w, 10, 10) * scale);

        Gizmos.DrawCube(transform.position + new Vector3(-w / 2f, 0, 0) * scale,
            new Vector3(10, h, 10) * scale);
        Gizmos.DrawCube(transform.position + new Vector3(w / 2f, 0, 0) * scale,
            new Vector3(10, h, 10) * scale);
    }*/
}
