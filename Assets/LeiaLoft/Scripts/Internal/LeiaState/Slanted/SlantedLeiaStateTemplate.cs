/****************************************************************
*
* Copyright 2019 © Leia Inc.  All rights reserved.
*
* NOTICE:  All information contained herein is, and remains
* the property of Leia Inc. and its suppliers, if any.  The
* intellectual and technical concepts contained herein are
* proprietary to Leia Inc. and its suppliers and may be covered
* by U.S. and Foreign Patents, patents in process, and are
* protected by trade secret or copyright law.  Dissemination of
* this information or reproduction of this materials strictly
* forbidden unless prior written permission is obtained from
* Leia Inc.
*
****************************************************************
*/
using System.Collections.Generic;
using UnityEngine;

namespace LeiaLoft
{
    /// <summary>
    /// ILeiaState implementation for Square-type displays
    /// </summary>
    public class SlantedLeiaStateTemplate : AbstractLeiaStateTemplate
    {

        // Need to replace these with proper shaders
        public static string OpaqueShaderName { get { return "LeiaLoft_Slanted_8V"; } }
        public static string OpaqueShaderNameLimitedViews { get { return "LeiaLoft_Slanted_8V"; } }
        public static string TransparentShaderName { get { return "LeiaLoft_Slanted_8V_Blending"; } }
        public static string TransparentShaderNameLimitedViews { get { return "LeiaLoft_Slanted_8V_Blending"; } }

        //Sharpening
        public static string SharpeningShaderName { get { return "LeiaLoft_ViewSharpening"; } }
        private Material _sharpening;

        public SlantedLeiaStateTemplate(DisplayConfig displayConfig) : base(displayConfig)
        {
            // this method was left blank intentionally
        }

        protected override Material CreateMaterial(bool alphaBlending)
        {
            if (_shaderName == null)
            {
                if (_viewsHigh * _viewsWide <= 8)
                {
                    SetShaderName(OpaqueShaderNameLimitedViews, TransparentShaderNameLimitedViews);
                }
                else
                {
                    SetShaderName(OpaqueShaderName, TransparentShaderName);
                }
            }

            return base.CreateMaterial(alphaBlending);
        }

        public override void DrawImage(LeiaCamera camera, LeiaStateDecorators decorators)
        {
            base.DrawImage(camera, decorators);
            Graphics.Blit(interlacedAlbedoTexture, Camera.current.activeTexture, _sharpening);
        }

        public void UpdateSharpeningParameters()
        {
            if (_sharpening == null)
            {
                _sharpening = new Material(Resources.Load<Shader>(SharpeningShaderName));
            }

            //////////////NEW CODE COPIED FROM UNREAL
            DisplayConfig config = LeiaDisplay.Instance.GetDisplayConfig();

            // Get input ACT coefficients.
            List<float> actCoeffs = config.UserActCoefficients[0];

            int actCoeffsCount = actCoeffs.Count;

            // Compute view step rates.
            int viewStepRateX = (int)config.p_over_du;
            int viewStepRateY = (int)config.p_over_dv;

            int numViews = _displayConfig.NumViews.x;

            bool trackerIsOn = LeiaDisplay.Instance.tracker != null && LeiaDisplay.Instance.tracker.enabled && LeiaDisplay.Instance.tracker.CameraConnected;

            int numFaces = 0;

            if (LeiaDisplay.Instance.tracker != null)
            {
                numFaces = LeiaDisplay.Instance.tracker.NumFaces;
            }

            float beta = config.Beta;
            if (trackerIsOn
                    && LeiaDisplay.Instance.blackViews
                    && numFaces == 1
                    && numViews >= 5)
            {
                if (numViews % 2 == 0)
                    beta *= (actCoeffs[0] + 0.75f * actCoeffs[1] + 0.5f * actCoeffs[2] + 0.25f * actCoeffs[3]) / (actCoeffs[0] + actCoeffs[1] + actCoeffs[2] + actCoeffs[3] + actCoeffs[4] + actCoeffs[5]);
                else
                    beta *= (actCoeffs[0] + actCoeffs[1]) / (actCoeffs[0] + actCoeffs[1] + actCoeffs[2] + actCoeffs[3] + actCoeffs[4] + actCoeffs[5]);
            }

            // Compute normalizer from all act values and beta.
            float normalizer = 1.0f;
            for (int i = 0; i < actCoeffsCount; i++)
                normalizer -= beta * actCoeffs[i];

            List<Vector4> sharpeningVectors = new List<Vector4>();

            // Compute normalized sharpening shader values (OfsX, OfsY, Weight)

            for (int i = 1; i <= actCoeffsCount; i++)
            {
                float x0 = Mathf.Floor((float)i / (float)viewStepRateX);
                float x1 = -x0;
                float y0 = viewStepRateY * (i % viewStepRateX);
                float y1 = -y0;
                float z = actCoeffs[i - 1] / normalizer;
                // Skip zero weights.
                if (z == 0.0f)
                {
                    continue;
                }
                // Add two sharpening values.

                sharpeningVectors.Add(new Vector4(x0, y0, z, 0));

                sharpeningVectors.Add(new Vector4(x1, y1, z, 0));
            }

            // when game engine is already in linear color space, it is as if gamma is 1
            float correctedGamma = QualitySettings.activeColorSpace == ColorSpace.Linear ? 1f : _displayConfig.Gamma;

            const string gammaToken = "_gamma";
            const string sharpeningCenterToken = "_sharpeningCenter";
            const string sharpeningXYToken = "_sharpeningXY";
            const string sharpeningXYLengthToken = sharpeningXYToken + "_Length";

            // export data to shader
            _sharpening.SetFloat(gammaToken, correctedGamma);
            _sharpening.SetFloat(sharpeningCenterToken, 1.0f / normalizer);
            int count = sharpeningVectors.Count;
            if (count > 0)
            {
                _sharpening.SetVectorArray(sharpeningXYToken, sharpeningVectors);
            }
            _sharpening.SetFloat(sharpeningXYLengthToken, sharpeningVectors.Count);
        }

        public override void GetFrameBufferSize(out int width, out int height)
        {
            var tileWidth = _displayConfig.ViewResolution.x;
            var tileHeight = _displayConfig.ViewResolution.y;
            width = (int)(_viewsWide * tileWidth);
            height = (int)(_viewsHigh * tileHeight);
        }

        public override void GetTileSize(out int tileWidth, out int tileHeight)
        {
            tileWidth = _displayConfig.UserViewResolution.x;
            tileHeight = _displayConfig.UserViewResolution.y;
        }

        public override void UpdateViews(LeiaCamera leiaCamera)
        {
            base.UpdateViews(leiaCamera);
            var calculated = new CameraCalculatedParams(leiaCamera.Camera, leiaCamera.BaselineScaling, leiaCamera.ConvergenceDistance,
                _displayConfig.ViewResolution, _displayConfig.ResolutionScale, _displayConfig.SystemDisparityPixels);

            var near = Mathf.Max(1.0e-5f, leiaCamera.NearClipPlane);
            var far = Mathf.Max(near, leiaCamera.FarClipPlane);
            var halfDeltaX = calculated.ScreenHalfWidth;
            var halfDeltaY = calculated.ScreenHalfHeight;

            Matrix4x4 m = Matrix4x4.zero;

            float orthoSize = Mathf.Max(1E-5f, leiaCamera.Camera.orthographicSize);
            float halfSizeX = orthoSize * CameraCalculatedParams.GetViewportAspectFor(leiaCamera, _displayConfig);
            float baseline = 2.0f * halfSizeX * (leiaCamera.BaselineScaling * _displayConfig.ResolutionScale) * _displayConfig.SystemDisparityPixels * leiaCamera.ConvergenceDistance / (_displayConfig.UserViewResolution.x / _displayConfig.ResolutionScale);

            System.Func<int, int, float> GetPosX = (nx, ny) =>
            {
                if (leiaCamera.Camera.orthographic) { return GetEmissionX(nx, ny) * baseline + leiaCamera.CameraShift.x; }
                else { return calculated.EmissionRescalingFactor * (GetEmissionX(nx, ny) + leiaCamera.CameraShift.x); }
            };
            System.Func<int, int, float> GetPosY = (nx, ny) =>
            {
                if (leiaCamera.Camera.orthographic) { return GetEmissionY(nx, ny) * baseline + leiaCamera.CameraShift.y; }
                else { return calculated.EmissionRescalingFactor * (GetEmissionY(nx, ny) + leiaCamera.CameraShift.y); }
            };

            for (int ny = 0; ny < _viewsHigh; ny++)
            {
                for (int nx = 0; nx < _viewsWide; nx++)
                {
                    var viewId = ny * _viewsWide + nx;
                    var view = leiaCamera.GetView(viewId);

                    if (view.IsCameraNull)
                    {
                        continue;
                    }

                    float posx = GetPosX(nx, ny);
                    float posy = GetPosY(nx, ny);

                    // must set position before calculating projection-for-position
                    view.Position = new Vector3(posx, posy, leiaCamera.CameraShift.z);

                    m = CameraCalculatedParams.GetConvergedProjectionMatrixForPosition(view.Camera, leiaCamera.transform.position + leiaCamera.transform.forward * leiaCamera.ConvergenceDistance);

                    view.Matrix = m;
                    view.NearClipPlane = near;
                    view.FarClipPlane = far;
                }
            }
        }


        public override void UpdateState(LeiaStateDecorators decorators, ILeiaDevice device)
        {
            if (_material == null)
            {
                _material = CreateMaterial(decorators.AlphaBlending);
            }

            // inside of CheckRenderTechnique, write into UserNumViews based upon stereo and device orientation and call SetViewCount
            SetUserNumViewsFromDecoratorsAndDevice(decorators, device);

            RespectOrientation(decorators);
            UpdateEmissionPattern(decorators);
            var shaderParams = new ShaderFloatParams();

            shaderParams._width = _displayConfig.UserPanelResolution.x;
            shaderParams._height = _displayConfig.UserPanelResolution.y;
            shaderParams._viewResX = _displayConfig.UserViewResolution.x;
            shaderParams._viewResY = _displayConfig.UserViewResolution.y;

            //TODO: Set d over n from display config

            shaderParams.faceX = decorators.FacePosition.x;
            shaderParams.faceY = decorators.FacePosition.y;
            shaderParams.faceZ = decorators.FacePosition.z;

            if (decorators.DeltaXArray != null)
            {
                shaderParams._deltaXArray = decorators.DeltaXArray;
                shaderParams._deltaXArraySize = decorators.DeltaXArray.Length;
            }

            var offset = _displayConfig.AlignmentOffset;
            shaderParams._offsetX = offset.x + (decorators.ParallaxOrientation.IsInv() ? XOffsetWhenInverted() : 0);
            shaderParams._offsetY = offset.y + (decorators.ParallaxOrientation.IsInv() ? YOffsetWhenInverted() : 0);

            // _displayHorizontalViewCount represents the display's actual view count along the horizontal / major axis
            // can be NumViews.x or NumViews.y depending upon display orientation
            shaderParams._viewsX = _displayHorizontalViewCount;
            // due to SetUserNumViewsFromDecoratorsAndDevice calling SetViewCount (h, 1), _viewsHigh will always be 1
            shaderParams._viewsY = _viewsHigh;

            shaderParams._orientation = decorators.ParallaxOrientation.IsLandscape() ? 1 : 0;
            shaderParams._adaptFOVx = decorators.AdaptFOV.x;
            shaderParams._adaptFOVy = decorators.AdaptFOV.y;
            shaderParams._enableSwizzledRendering = 1;
            shaderParams._enableHoloRendering = 1;
            shaderParams._enableSuperSampling = 0;
            shaderParams._separateTiles = 1;


            var is2d = shaderParams._viewsY == 1 && shaderParams._viewsX == 1;

            if (decorators.ShowTiles || is2d)
            {
                shaderParams._enableSwizzledRendering = 0;
                shaderParams._enableHoloRendering = 0;
            }

            // enable interlacing view count in interlacing shader
            _material.EnableKeyword(string.Format("LEIA_INTERLACING_READ_{0}V", _displayHorizontalViewCount - 1));

            shaderParams._isFlippedAlignment = 0.0f;

            ScreenOrientation orientation = device.GetScreenOrientationRGB();

            float[] selectedInterlacingMatrix = _displayConfig.getInterlacingMatrixForOrientation(orientation);
            shaderParams._interlace_matrix = selectedInterlacingMatrix.ToMatrix4x4();
            float[] selelctedInterlacingVector = _displayConfig.getInterlacingVectorForOrientation(orientation);
            shaderParams._interlace_vector = selelctedInterlacingVector.ToVector4();

            if (decorators.ShowTiles)
            {
                _material.EnableKeyword("ShowTiles");
            }
            else
            {
                _material.DisableKeyword("ShowTiles");
            }

            // note - since this is ILeiaState.UpdateState, be sure that you called UpdateState
            _material.SetFloat("dynamic_interlace_scale", 1.0f);
            _material.SetFloat("dynamic_interlace_cos", 1.0f);
            _material.SetFloat("dynamic_interlace_sin", 0.0f);

            SetShaderSubpixelKeywordsFromMatrix(shaderParams._interlace_matrix);

            foreach (string keyword in _displayConfig.InterlacingMaterialEnableKeywords)
            {
                _material.EnableKeyword(keyword);
            }

            SetInterlacedBackgroundPropertiesFromDecorators(decorators);

            shaderParams._showCalibrationSquares = decorators.ShowCalibration ? 1 : 0;
            shaderParams.ApplyTo(_material);

            UpdateSharpeningParameters();
        }

        public override void Release()
        {
            // release sharpening _material
            if (_sharpening != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(_sharpening);
                }
                else
                {
                    GameObject.DestroyImmediate(_sharpening);
                }
            }
            base.Release();
        }
    }
}
