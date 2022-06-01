using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LeiaLoft;

public class DebugLeiaViews : MonoBehaviour
{
    public Transform debugCameraPrefab;

    public GameObject[] debugCameras;

    LeiaVirtualDisplay leiaVirtualDisplay;

    // Start is called before the first frame update
    void Start()
    {
        leiaVirtualDisplay = FindObjectOfType<LeiaVirtualDisplay>();
        Invoke("InstantiateCameras", 1f);
    }

    // Update is called once per frame
    void InstantiateCameras()
    {
        debugCameras = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform newDebugCamIcon = Instantiate(
                debugCameraPrefab,
                transform.GetChild(i).position,
                Quaternion.identity,
                transform.GetChild(i)
                );

            debugCameras[i] = newDebugCamIcon.gameObject;

            newDebugCamIcon.GetComponentInChildren<TextMesh>().text = "" + i;
        }
    }

    void Update()
    {
        if (debugCameras == null)
            return;

        if (LeiaDisplay.Instance.DesiredRenderTechnique == LeiaDisplay.RenderTechnique.Stereo)
        {
            for (int i = 0; i < debugCameras.Length; i++)
            {
                if (debugCameras[i] != null)
                {
                    //In stereo only enable the first two camera icons
                    debugCameras[i].SetActive(i < 4);
                }
            }
        }
        else
        {
            for (int i = 0; i < debugCameras.Length; i++)
            {
                if (debugCameras[i] != null)
                {
                    //In LF enable all camera icons
                    debugCameras[i].SetActive(true);
                }
            }
        }

        for (int i = 0; i < debugCameras.Length; i++)
        {
            if (debugCameras[i] != null)
            {
                debugCameras[i].transform.localScale = Vector3.one * leiaVirtualDisplay.height / 15f;
            }
        }
    }
}
