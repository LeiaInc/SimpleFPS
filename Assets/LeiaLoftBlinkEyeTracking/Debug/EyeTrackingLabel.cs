using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LeiaLoft;

public class EyeTrackingLabel : MonoBehaviour {
	
    public BlinkTrackingUnityPlugin blink;
	public Text label;

	void Start () {
        blink = FindObjectOfType<BlinkTrackingUnityPlugin>();
	}
	
	void Update () {
		if (blink.trackingResult.numDetectedFaces > 0)
		{
			LeiaHeadTracking.Vector3 facePos = blink.trackingResult.detectedFaces[0].pos;

			label.text = string.Format(
				"Face position = [{0:F2},{1:F2},{2:F2}]",
				facePos.x,
				facePos.y,
				facePos.z
			);

			label.text += string.Format("\ndiStretch is {0}", LeiaDisplay.Instance.getStretch());
			label.text += string.Format("\nN is {0}", LeiaDisplay.Instance.getN(facePos.x, facePos.y, facePos.z, 0, 0));
		}
		else if (label != null)
        {
			label.text = string.Format(
				"Null or zero faces\nFace position = [{0:F2},{1:F2},{2:F2}]",
				0.001f,
				0.0001f,
				0.00001f
				);
		}
	}
}
