using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LeiaLoft;

[RequireComponent(typeof(LeiaCamera))]
public class EyeTrackingCameraShift : MonoBehaviour
{
    private ToggleScaleTranslate cameraShiftX_ScaleTranslate = new ToggleScaleTranslate(1f, 0, ToggleScaleTranslate.ModificationMode.ON);
    private KeyCode ctrlTogglePeeling = KeyCode.P;
    private KeyCode ctrlToggleShift = KeyCode.S;

    [HideInInspector] public Vector4 peelControls = new Vector4(1, 1, 0, 0);

    private LeiaCamera mLeiaCamera;

    bool isPeeling = true;
    bool isShifting = false;

    public float slidingScale;

    public void SetSlidingScale(float newSlidingScale)
    {
        slidingScale = Mathf.Round(newSlidingScale * 10f) / 10f;
        LeiaDisplay.Instance.slidingScale = slidingScale;
    }

    private Matrix4x4 faceTrackingCameraTransform = new Matrix4x4
    {
        m00 = -0.1f,
        m01 = 0,
        m02 = 0,
        m03 = 0,
        m10 = 0,
        m11 = 0.1f,
        m12 = 0,
        m13 = 0,
        m20 = 0,
        m21 = 0,
        m22 = 1,
        m23 = 0,
        m30 = 0,
        m31 = 0,
        m32 = 0,
        m33 = 1
    };

    Vector2 previousShift;

    // Use this for initialization
    void Start()
    {
        if (mLeiaCamera == null)
        {
            mLeiaCamera = GetComponent<LeiaCamera>();
        }
        SetSlidingScale(mLeiaCamera.BaselineScaling / 7f);
        //SetSlidingScale(mLeiaCamera.BaselineScaling / 7f);
        //Debug.Log("Set sliding scale to "+mLeiaCamera.BaselineScaling / 7f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(ctrlTogglePeeling) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            isPeeling = !isPeeling;
        }

        float peelZ = peelControls.z;
        // user can control peeling params from this peelControls property on EyeTrackingCameraShift. but data is ultimately passed to SlantedLeiaStateTemplate.peel_ScaleTranslate
        if (LeiaDisplay.Instance.viewPeeling)
        {
            peelZ = Mathf.RoundToInt(peelControls.z);
        }


        AbstractLeiaStateTemplate.peel_ScaleTranslate = new ToggleScaleTranslate(
            Mathf.RoundToInt(peelControls.x),
            peelZ,
            isPeeling ? ToggleScaleTranslate.ModificationMode.ON : ToggleScaleTranslate.ModificationMode.NOSHIFT
            );

        LeiaDisplay.Instance.UpdateLeiaState();

        if (Input.GetKeyDown(ctrlToggleShift) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            if (cameraShiftX_ScaleTranslate.mode == ToggleScaleTranslate.ModificationMode.ON)
            {
                cameraShiftX_ScaleTranslate.mode = ToggleScaleTranslate.ModificationMode.NOSHIFT;
            }
            else
            {
                cameraShiftX_ScaleTranslate.mode = ToggleScaleTranslate.ModificationMode.ON;
            }
        }
    }
}
