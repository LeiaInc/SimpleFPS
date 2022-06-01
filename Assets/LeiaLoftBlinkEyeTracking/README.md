# Setting up eye tracking

- Add LeiaLoft/Plugins/EyeTracking/Blink Eye Tracking.prefab to your scene(s)
- Add EyeTrackingCameraShift MonoBehaviour to your LeiaCamera(s) in your scene(s)

# Eye tracking build documentation

- At build time, ensure that you have enabled "Use unsafe code"
- At build time, confirm you issued an x86_64 bit builds. x86 is not supported

# Eye tracking contribution documentation

See new chunks of code in

     LeiaDisplay #region dynamic_interlacing_display
     AbstractLeiaStateTemplate #region view_peeling_code
     Substantial inline changes in LeiaLoft_View_Index_Calculator.cginc
     Substantial inline changes in LeiaLoft_Texture_Read
     Substantial inline changes in LeiaLoft_Slanted_8V

