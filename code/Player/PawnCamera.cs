using Sandbox;
using Sandbox.Effects;

namespace CrazyEights;

public partial class PawnCamera : CameraMode
{
    private float FOV = 90f;
    private ScreenEffects screenEffects = null;

    public override void Activated()
    {
        var pawn = Local.Pawn;
        if(pawn == null) return;

        Position = pawn.EyePosition;
        Rotation = Rotation.From(1, 0, 0); // Intialize as anything other than (0,0,0) as Lerping 0 with 0 is NaN = black screen + 50fps
        ZNear = .1f;
        ZFar = 5000;

        // Provides Vignette for alert effects
        screenEffects = Map.Camera.FindOrCreateHook<ScreenEffects>();
    }

    public override void Deactivated()
    {
        // Cleanup effects when swapping cameras (devcam)
        Map.Camera.RemoveAllHooks<ScreenEffects>();
    }

    public override void Update()
    {
        var pawn = Local.Pawn;
        if(pawn == null) return;

        Viewer = pawn;

        Position = Position.LerpTo(pawn.EyePosition, 20 * Time.Delta);
        Rotation = pawn.EyeRotation;

        float targetFOV = FOV;
        FieldOfView = FieldOfView.LerpTo(targetFOV, 10f * Time.Delta);

        // Gradually lerp Vignette back to 0.
        if(screenEffects != null && screenEffects.Vignette.Intensity > 0f)
        {
            var intensity = screenEffects.Vignette.Intensity;
            screenEffects.Vignette.Intensity = intensity.LerpTo(0f, 3f * Time.Delta);
        }
    }

    public override void BuildInput(InputBuilder inputBuilder)
    {
        base.BuildInput(inputBuilder);

        if(inputBuilder.StopProcessing)
            return;

        FOV -= inputBuilder.MouseWheel * 10f;
        FOV = FOV.Clamp(10, 90);
    }

    /// <summary>
    /// Sets Vignette.Intensity.
    /// </summary>
    /// <param name="intensity"></param>
    public void SetVignetteIntensity(float intensity) => screenEffects.Vignette.Intensity = intensity;
    /// <summary>
    /// Sets Vignette.Color
    /// </summary>
    /// <param name="color"></param>
    public void SetVignetteColor(Color color) => screenEffects.Vignette.Color = color;
}
