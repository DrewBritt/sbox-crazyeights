using System.ComponentModel;
using Sandbox;

namespace CrazyEights;

public partial class Player
{
    /// <summary>
	/// Normalized accumulation of Input.AnalogLook
	/// </summary>
    [ClientInput] public Angles LookInput { get; protected set; }

    /// <summary>
    /// Position a player should be looking from in world space.
    /// </summary>
    public Vector3 EyePosition => AimRay.Position;

    /// <summary>
    /// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity.
    /// </summary>
    public Rotation EyeRotation => Rotation.LookAt(AimRay.Forward);

    /// <summary>
    /// Override the aim ray to use the player's eye position and rotation.
    /// </summary>
    public override Ray AimRay
    {
        get
        {
            var eyeTransform = GetAttachment("eyes") ?? default;
            return new Ray(eyeTransform.Position, LookInput.Forward);
        }
    }

    public override void BuildInput()
    {
        if(Input.StopProcessing)
            return;

        // Calculate new ViewAngles from input diff, then clamp.
        var lookInput = (LookInput + Input.AnalogLook);
        var clampedAngles = new Angles(
            lookInput.pitch.Clamp(-60, 75),
            lookInput.yaw.Clamp(LocalRotation.Yaw() - 80f, LocalRotation.Yaw() + 80f),
            0
        );
        LookInput = clampedAngles;

        PlayerCamera?.BuildInput();

        base.BuildInput();
    }
}
