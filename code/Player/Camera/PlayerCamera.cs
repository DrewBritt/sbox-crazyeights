using Sandbox;
using Sandbox.Effects;

namespace CrazyEights;

public partial class PlayerCamera
{
    private float FOV = 90f;
    private ScreenEffects screenEffects = null;

    public PlayerCamera()
    {
        Camera.Rotation = Rotation.From(1, 0, 0); // Intialize as anything other than (0,0,0) as Lerping 0 with 0 is NaN = black screen + 50fps
        Camera.ZNear = .1f;
        Camera.ZFar = 5000;

        // Provides Vignette for alert effects
        screenEffects = Camera.Main.FindOrCreateHook<ScreenEffects>();
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

    public void BuildInput()
    {
        if(Input.StopProcessing)
            return;

        FOV -= Input.MouseWheel * 10f;
        FOV = FOV.Clamp(10, 90);
    }

    public void Update(Player player)
    {
        Camera.Position = Camera.Position.LerpTo(player.EyePosition, 20 * Time.Delta);
        Camera.Rotation = player.EyeRotation;
        Camera.FirstPersonViewer = player;

        float targetFOV = FOV;
        Camera.Main.FieldOfView = Camera.Main.FieldOfView.LerpTo(targetFOV, 10f * Time.Delta);

        // Gradually lerp Vignette back to 0.
        if(screenEffects != null && screenEffects.Vignette.Intensity > 0f)
        {
            var intensity = screenEffects.Vignette.Intensity;
            screenEffects.Vignette.Intensity = intensity.LerpTo(0f, 3f * Time.Delta);
        }
    }

    ~PlayerCamera()
    {
        // Cleanup effects when swapping cameras (devcam)
        //Camera.Main.RemoveAllHooks<ScreenEffects>(); TODO: Investigate cause of crashing?
    }
}
