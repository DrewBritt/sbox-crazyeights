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
    public Vector3 EyePosition
    {
        get => Transform.PointToWorld(EyeLocalPosition);
        set => EyeLocalPosition = Transform.PointToLocal(value);
    }

    /// <summary>
	/// Position a player should be looking from in local to the entity coordinates.
	/// </summary>
	[Net, Predicted, Browsable(false)]
    public Vector3 EyeLocalPosition { get; set; }

    /// <summary>
    /// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity.
    /// </summary>
    [Browsable(false)]
    public Rotation EyeRotation
    {
        get => Transform.RotationToWorld(EyeLocalRotation);
        set => EyeLocalRotation = Transform.RotationToLocal(value);
    }

    /// <summary>
    /// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity. In local to the entity coordinates.
    /// </summary>
    [Net, Predicted, Browsable(false)]
    public Rotation EyeLocalRotation { get; set; }

    /// <summary>
    /// Override the aim ray to use the player's eye position and rotation.
    /// </summary>
    public override Ray AimRay => new Ray(EyePosition, EyeRotation.Forward);

    public override void BuildInput()
    {
        if(Input.StopProcessing)
            return;

        // Calculate new ViewAngles from input diff, then clamp.
        var lookInput = (LookInput + Input.AnalogLook);
        var clampedAngles = new Angles(
            lookInput.pitch.Clamp(-60, 75),
            lookInput.yaw.Clamp(-80, 80),
            lookInput.roll
        );
        LookInput = clampedAngles.Normal;

        PlayerCamera?.BuildInput();

        base.BuildInput();
    }
}
