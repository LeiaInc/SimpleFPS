#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using static LeiaLoft.LeiaDisplay;

namespace LeiaLoft
{
    [CustomEditor(typeof(LeiaDisplay))]
    public class LeiaDisplayEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            LeiaDisplay targetComponent = (LeiaDisplay)target;

            Undo.RecordObject(targetComponent, "LeiaDisplayChanges");
            Undo.RecordObject(targetComponent.transform, "LeiaDisplayTransformChanges");
            if (targetComponent.DriverCamera != null)
            {
                Undo.RecordObject(targetComponent.DriverCamera, "DriverCameraChanges");
            }

            // Custom inspector code here
            //EditorGUILayout.LabelField("Custom Inspector for YourComponent");
            //targetComponent.someVariable = EditorGUILayout.IntField("Some Variable", targetComponent.someVariable);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            Texture imageTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/LeiaLoftNew/leialogo.png");
            GUILayout.Label(imageTexture, GUILayout.Height(200), GUILayout.Width(600)); // Adjust size as needed
            GUILayout.FlexibleSpace(); // Add more flexible space to center the image
            GUILayout.EndHorizontal();

            //Preview Mode (Stereo / Interlaced)
            string[] options = new string[]
            {
                "SideBySide", "Interlaced",
            };
            targetComponent.DesiredPreviewMode = (EditorPreviewMode)EditorGUILayout.Popup("Preview Mode", (int)targetComponent.DesiredPreviewMode, options);

            //FIELD OF VIEW
            if (targetComponent.DriverCamera != null)
            {
                float PrevFieldOfView = targetComponent.DriverCamera.fieldOfView;

                targetComponent.DriverCamera.fieldOfView = EditorGUILayout.FloatField(
                    "Field Of View",
                    targetComponent.DriverCamera.fieldOfView
                );

                if (targetComponent.DriverCamera.fieldOfView != PrevFieldOfView)
                {
                    //set parallax factor based on the new field of view
                }
            }
            else
            {
                float PrevFieldOfView = targetComponent.ViewersHead.headcamera.fieldOfView;

                targetComponent.ViewersHead.headcamera.fieldOfView = Mathf.Clamp(EditorGUILayout.FloatField(
                    "Field Of View", targetComponent.ViewersHead.headcamera.fieldOfView), 2.13805f, 61.82141f);

                if (targetComponent.ViewersHead.headcamera.fieldOfView != PrevFieldOfView)
                {
                    targetComponent.ParallaxFactor = 1f / (targetComponent.HeightMM / (2f * targetComponent.ViewingDistanceMM * Mathf.Tan(Mathf.Deg2Rad * targetComponent.ViewersHead.headcamera.fieldOfView / 2f)));
                }
            }

            //VIRTUAL DISPLAY HEIGHT

            EditorGUI.BeginDisabledGroup(targetComponent.mode == LeiaDisplay.ControlMode.CameraDriven);
            float VirtualHeightPrev = targetComponent.VirtualHeight;
            targetComponent.VirtualHeight = EditorGUILayout.FloatField("Virtual Display Height", targetComponent.VirtualHeight);

            if (VirtualHeightPrev != targetComponent.VirtualHeight)
            {
                //update camera rig based on change in virtual display height

            }

            //PARALLAX

            float ParallaxPrev = targetComponent.ParallaxFactor;
            targetComponent.ParallaxFactor = Mathf.Clamp(
                EditorGUILayout.FloatField("Parallax", targetComponent.ParallaxFactor),
                .1f, 5f
            );
            EditorGUI.EndDisabledGroup();

            if (ParallaxPrev != targetComponent.ParallaxFactor)
            {
                targetComponent.ViewersHead.headcamera.fieldOfView = Mathf.Atan((targetComponent.ParallaxFactor * targetComponent.HeightMM) / targetComponent.ViewingDistanceMM) * Mathf.Rad2Deg;
                Debug.Log("gets here");
            }

            //DEPTH

            targetComponent.DepthFactor = EditorGUILayout.FloatField("Depth", targetComponent.DepthFactor);

            if (targetComponent.DepthFactor < .1f)
            {
                targetComponent.DepthFactor = .1f;
            }
            if (targetComponent.DepthFactor > 5f)
            {
                targetComponent.DepthFactor = 5f;
            }

            //DISPLAY DISTANCE FROM CAMERA

            if (targetComponent.mode == LeiaDisplay.ControlMode.CameraDriven)
            {
                targetComponent.ConvergenceDistance = EditorGUILayout.FloatField(
                    "Display Distance",
                    targetComponent.ConvergenceDistance
                );

                if (targetComponent.ConvergenceDistance < .1f)
                {
                    targetComponent.ConvergenceDistance = .1f;
                }
            }

            //RESET BUTTON

            if (GUILayout.Button("Reset"))
            {
                targetComponent.ParallaxFactor = 1;
                targetComponent.DepthFactor = 1;
            }

            targetComponent.Update();
        }
    }
}
#endif