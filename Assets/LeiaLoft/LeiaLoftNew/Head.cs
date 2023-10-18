using LeiaLoft;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class Head : MonoBehaviour
{
    public LeiaDisplay leiaDisplay;
    public List<Eye> eyes;
    public Vector3 HeadPositionMM;
    public float ipdMM = 63;
    public Camera headcamera;

    public List<Vector2> ViewConfig;

    public void InitHead(List<Vector2> ViewConfig, LeiaDisplay leiaDisplay)
    {
        headcamera = gameObject.AddComponent<Camera>();
        headcamera.depth = 1f;
        //headcamera.enabled = false;
        //EditorUtility.CopySerialized(leiaDisplay.cam, gameObject);
        HeadPositionMM = new Vector3(0, 0, leiaDisplay.ViewingDistanceMM);

        this.leiaDisplay = leiaDisplay;
        this.ViewConfig = ViewConfig;

        eyes = new List<Eye>();
        foreach (var offset in ViewConfig)
        {
            GameObject pivotGO = new GameObject("Eye");
            pivotGO.transform.parent = transform;
            Eye newEye = pivotGO.AddComponent<Eye>();
            newEye.offset = offset;
            newEye.leiaDisplay = leiaDisplay;
            newEye.eyecamera.nearClipPlane = headcamera.nearClipPlane;
            newEye.eyecamera.farClipPlane = headcamera.farClipPlane;
            eyes.Add(newEye);
        }
    }

    public void Update()
    {
        if (leiaDisplay == null || leiaDisplay.ViewersHead != this)
        {
            DestroyImmediate(gameObject);
            return;
        }

        transform.localPosition = leiaDisplay.RealToVirtualCenterFacePosition(
            HeadPositionMM
        );

        transform.LookAt(leiaDisplay.transform.position, leiaDisplay.transform.up);

        //Debug.DrawLine(transform.position, leiaDisplay.transform.position);

        Matrix4x4 p = leiaDisplay.GetProjectionMatrixForCamera(headcamera, Vector3.zero, false);

        headcamera.projectionMatrix = p;

        foreach (Eye eye in eyes)
        {
            eye.eyecamera.nearClipPlane = headcamera.nearClipPlane;
            eye.eyecamera.farClipPlane = headcamera.farClipPlane;
            eye.Update();
        }
    }

    private void OnDrawGizmos()
    {
        //LeiaDisplay.DrawFrustum(transform, leiaDisplay);
    }


    void OnPostRender()
    {
        if (Application.isPlaying)
        {
            leiaDisplay.RenderImage();
        }
    }
}
