using LeiaLoft;
using UnityEngine;
using UnityEngine.UI;

public class DisplayConfigDebugText : MonoBehaviour
{
    public GameObject errorPanel;
    Text text;
    DisplayConfig config;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
        UpdateNow();

        if (errorPanel != null)
        {
            if (config.status != DisplayConfig.Status.SuccessfullyLoadedFromDevice)
            {
                errorPanel.SetActive(true);
            }
        }
    }

    void UpdateNow()
    {
        config = LeiaDisplay.Instance.GetDisplayConfig(); //new DisplayConfigv2();
        text.text = "config.ToString() = \n"+config.ToStringV2();
        Invoke("UpdateNow",.5f);
    }
}
