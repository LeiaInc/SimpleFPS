
using LeiaLoft;
using UnityEngine;

[ExecuteInEditMode]
public class Eye : MonoBehaviour
{
    public LeiaDisplay leiaDisplay;
    public Camera _eyecamera;
    public Camera eyecamera
    {
        get
        {
            if (_eyecamera == null)
            {
                _eyecamera = transform.GetComponent<Camera>();
            }

            if (_eyecamera == null)
            {
                _eyecamera = transform.gameObject.AddComponent<Camera>();
            }
            return _eyecamera;
        }
    }

    public RenderTexture TargetTexture
    {
        get { return !eyecamera ? null : eyecamera.targetTexture; }
        set { if (eyecamera) { eyecamera.targetTexture = value; } }
    }

    public Vector2 offset;

    void Start()
    {
        if (eyecamera == null)
        {
            _eyecamera = transform.gameObject.AddComponent<Camera>();
        }
    }

    /// <summary>
    /// Creates a renderTexture.
    /// </summary>
    /// <param name="width">Width of renderTexture in pixels</param>
    /// <param name="height">Height of renderTexture in pixels</param>
    /// <param name="viewName">Name of renderTexture</param>
    public void SetTextureParams(int width, int height, string viewName)
    {
        if (eyecamera == null)
        {
            return;
        }

        if (eyecamera.targetTexture == null)
        {
            TargetTexture = CreateRenderTexture(width, height, viewName);
        }
        else
        {
            if (TargetTexture.width != width ||
                TargetTexture.height != height)
            {
                Release();
                TargetTexture = CreateRenderTexture(width, height, viewName);
            }
        }
    }
    private static RenderTexture CreateRenderTexture(int width, int height, string rtName)
    {
        var leiaViewSubTexture = new RenderTexture(width, height, 24)
        {
            name = rtName,
        };
        //leiaViewSubTexture.ApplyIntermediateTextureRecommendedProperties();
        //leiaViewSubTexture.ApplyLeiaViewRecommendedProperties();
        leiaViewSubTexture.Create();

        return leiaViewSubTexture;
    }

    public void Release()
    {
        // targetTexture can be null at this point in execution
        if (TargetTexture != null)
        {
            if (Application.isPlaying)
            {
                TargetTexture.Release();
                GameObject.Destroy(TargetTexture);
            }
            else
            {
                TargetTexture.Release();
                GameObject.DestroyImmediate(TargetTexture);
            }

            TargetTexture = null;
        }
    }

    public void Update()
    {
        if (leiaDisplay == null)
        {
            DestroyImmediate(gameObject);
            return;
        }
        float virtualBaseline = leiaDisplay.DepthFactor * leiaDisplay.ViewersHead.ipdMM * leiaDisplay.MMToVirtual;
        transform.localPosition = offset * virtualBaseline;

        eyecamera.transform.rotation = leiaDisplay.transform.rotation;

        if (leiaDisplay.mode == LeiaDisplay.ControlMode.CameraDriven)
        {
            //camera.fieldOfView = leiaDisplay.cam.fieldOfView;
        }
        else //if display driven
        {

        }

        Matrix4x4 p = leiaDisplay.GetProjectionMatrixForCamera(
            eyecamera, 
            transform.parent.localPosition,
            true
        );

        eyecamera.projectionMatrix = p;
    }

    void SetPositionFromRealEyePosition(Vector3 EyePositionMM)
    {
        transform.localPosition = EyePositionMM * leiaDisplay.VirtualHeight / leiaDisplay.HeightMM;
    }

    private void OnDrawGizmos()
    {
        leiaDisplay.DrawFrustum(transform);
    }
}
