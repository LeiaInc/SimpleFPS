using LeiaLoft;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaselineLabel : MonoBehaviour
{
    LeiaCamera leiaCamera;
    EyeTrackingCameraShift shifter;
    Text text;
    // Start is called before the first frame update
    void Start()
    {
        shifter = FindObjectOfType<EyeTrackingCameraShift>();
        text = GetComponent<Text>();
        leiaCamera = FindObjectOfType<LeiaCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "BaselineScaling = "+leiaCamera.BaselineScaling+"\nFOV: "+leiaCamera.Camera.fieldOfView+"\nSliding Scale: "+shifter.slidingScale;
    }
}
