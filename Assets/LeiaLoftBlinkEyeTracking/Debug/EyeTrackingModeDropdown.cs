using LeiaLoft;
using UnityEngine;
using UnityEngine.UI;

public class EyeTrackingModeDropdown : MonoBehaviour
{
    LeiaDisplay _leiaDisplay;
    LeiaDisplay leiaDisplay
    {
        get
        {
            if (!_leiaDisplay)
            {
                _leiaDisplay = FindObjectOfType<LeiaDisplay>();
            }
            return _leiaDisplay;
        }
    }
    LeiaCamera leiaCamera;
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

    LeiaVirtualDisplay _leiaVirtualDisplay;
    LeiaVirtualDisplay leiaVirtualDisplay
    {
        get
        {
            if (!_leiaVirtualDisplay)
            {
                _leiaVirtualDisplay = FindObjectOfType<LeiaVirtualDisplay>();
            }
            return _leiaVirtualDisplay;
        }
    }

    EyeTrackingCameraShift shifter;

    public GameObject baselineSliderBox;
    public GameObject cameraShiftScaleSliderBox;

    public Text slideScaleLabel;
    public Text baselineLFLabel;
    public Text baselineStereoLabel;

    public Slider slideScaleSlider;
    public Slider baselineLFSlider;
    public Slider baselineStereoSlider;

    public Dropdown dropdown;

    // Start is called before the first frame update
    void Start()
    {
        leiaCamera = FindObjectOfType<LeiaCamera>();
        shifter = FindObjectOfType<EyeTrackingCameraShift>();

        baselineLFLabel.text = "bX: " + leiaCamera.BaselineScaling;
        baselineStereoLabel.text = "bX: " + leiaCamera.BaselineScaling*2f;
        slideScaleLabel.text = "aX: " + shifter.slidingScale;
        
        slideScaleSlider.SetValueWithoutNotify(leiaVirtualDisplay.slidingScaleStereo);
        baselineLFSlider.SetValueWithoutNotify(leiaVirtualDisplay.baselineScaleLF);
        baselineStereoSlider.SetValueWithoutNotify(leiaVirtualDisplay.baselineScaleStereo);
        
    }

    private void Update()
    {
        
        slideScaleSlider.SetValueWithoutNotify(leiaVirtualDisplay.slidingScaleStereo);
        baselineLFSlider.SetValueWithoutNotify(leiaVirtualDisplay.baselineScaleLF);
        baselineStereoSlider.SetValueWithoutNotify(leiaVirtualDisplay.baselineScaleStereo);
        
        baselineLFLabel.text = "bX: " + leiaVirtualDisplay.baselineScaleLF;
        baselineStereoLabel.text = "bX: " + leiaVirtualDisplay.baselineScaleStereo;
        slideScaleLabel.text = "aX: " + leiaVirtualDisplay.slidingScaleStereo;
    }

    public void OnValueChanged(int selected)
    {
        if (selected == 0)
        {
            SetViewPeelingMode();
        }
        else
        {
            SetTrackedStereoMode();
        }
    }

    public void SetViewPeelingMode()
    {
        //View Peeling
        leiaDisplay.viewPeeling = true;
        leiaDisplay.DesiredRenderTechnique = LeiaDisplay.RenderTechnique.Default;
        cameraShiftScaleSliderBox.SetActive(false);
        baselineLFSlider.transform.parent.gameObject.SetActive(true);
        baselineStereoSlider.transform.parent.gameObject.SetActive(false);
        dropdown.SetValueWithoutNotify(0);
        //baselineLFSlider.value = (blink.B_AT_Z0_LF);
    }

    public void SetTrackedStereoMode()
    {
        //Tracked Stereo
        leiaDisplay.viewPeeling = false;
        leiaDisplay.DesiredRenderTechnique = LeiaDisplay.RenderTechnique.Stereo;
        cameraShiftScaleSliderBox.SetActive(true);
        baselineLFSlider.transform.parent.gameObject.SetActive(false);
        baselineStereoSlider.transform.parent.gameObject.SetActive(true);
        dropdown.SetValueWithoutNotify(1);
        //baselineStereoSlider.value = (blink.B_AT_Z0_Stereo);
    }

    public void OnBaselineLFSliderChanged(float newBaseline)
    {
        blink.B_AT_Z0_LF = newBaseline;
        baselineLFLabel.text = "bX: " + newBaseline;
        //leiaCamera.BaselineScaling = newBaseline;
    }

    public void OnBaselineStereoSliderChanged(float newBaseline)
    {
        blink.B_AT_Z0_Stereo = newBaseline;
        baselineStereoLabel.text = "bX: " + newBaseline;
        //leiaCamera.BaselineScaling = newBaseline;
    }

    public void OnCameraShiftScaleSliderChanged(float newCameraShiftScale)
    {
        slideScaleLabel.text = "aX: " + newCameraShiftScale;
        shifter.SetSlidingScale(newCameraShiftScale);
    }
}
