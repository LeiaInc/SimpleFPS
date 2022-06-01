using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LeiaLoft;

public class EyeTrackingDebugCanvas : MonoBehaviour
{
    public GameObject EyeTrackingStatusBar;
    public Text textLabel;
    BlinkTrackingUnityPlugin blink;

    public GameObject logoScene;
    public GameObject patternMediaViewer;

    public GameObject debugPanel;

    LeiaDisplay leiaDisplay;

    public EyeTrackingModeDropdown eyeTrackingMode;

    // Start is called before the first frame update

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

    void Start()
    {
        leiaDisplay = FindObjectOfType<LeiaDisplay>();
        blink = FindObjectOfType<BlinkTrackingUnityPlugin>();
        Invoke("UpdateStatus", 1f);
        eyeTrackingMode.SetTrackedStereoMode();
    }

    public void ShowPattern(bool visible)
    {
        logoScene.SetActive(!visible);
        patternMediaViewer.SetActive(visible);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            debugPanel.SetActive(!debugPanel.activeSelf);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            shifter.enabled = false;
            Invoke("EnableShifter",.5f);
            if (!leiaDisplay.viewPeeling)
            {
                eyeTrackingMode.SetViewPeelingMode();
            }
            else
            {
                eyeTrackingMode.SetTrackedStereoMode();
            }
        }
    }

    void EnableShifter()
    {
        shifter.enabled = true;
    }

    void UpdateStatus()
    {
        Invoke("UpdateStatus", .1f);

        if (Application.isEditor)
        {
            textLabel.text = "Eye tracking not supported in editor. Make a build to test with eye tracking.";
            EyeTrackingStatusBar.SetActive(true);
        }
        else
        if (!blink.CameraConnected)
        {
            textLabel.text = "Eye tracking camera not connected. Check the USB connection.";
            EyeTrackingStatusBar.SetActive(true);
        }
        else if (blink.NumFaces == 0)
        {
            textLabel.text = "No faces detected.";
            EyeTrackingStatusBar.SetActive(true);
        }
        else
        {
            EyeTrackingStatusBar.SetActive(false);
        }
    }
}
