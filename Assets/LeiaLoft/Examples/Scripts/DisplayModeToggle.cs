using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LeiaLoft;

public class DisplayModeToggle : MonoBehaviour
{
    [SerializeField] Sprite toggleOnSprite;
    [SerializeField] Sprite toggleOffSprite;
    [SerializeField] Image ToggleImage;
    LeiaDisplay leiaDisplay;

    public void Toggle2D3D()
    {
        if(leiaDisplay == null)
        {
            leiaDisplay = FindObjectOfType<LeiaDisplay>();
            if (leiaDisplay == null)
            {
                Debug.LogError("DisplayModeToggle:Toggle2D3D() LeiaDisplayDoes not exist in scene.");
                return;
            }
        }
        if(leiaDisplay.DesiredLightfieldMode == LeiaDisplay.LightfieldMode.Off)
        {
            Debug.Log("3D On");
            ToggleImage.sprite = toggleOnSprite;
            leiaDisplay.DesiredLightfieldMode = LeiaDisplay.LightfieldMode.On;
        }
        else
        {
            Debug.Log("3D Off");
            ToggleImage.sprite = toggleOffSprite;
            leiaDisplay.DesiredLightfieldMode = LeiaDisplay.LightfieldMode.Off;
        }
    }
}
