using System.ComponentModel.DataAnnotations;
using Sandbox;
using SandboxEditor;

namespace CrazyEights;

[Model]
[SupportsSolid]
[Library("ce_chair")]
[Display(Name = "Crazy Eights Player Chair", Description = "A chair in which a player's pawn will spawn and sit in.")]
[HammerEntity]
public partial class PlayerChair : ModelEntity
{
    public bool HasPlayer { get; private set; } = false;

    public override void Spawn()
    {
        base.Spawn();

        SetupPhysicsFromModel(PhysicsMotionType.Static);
        EnableAllCollisions = false;
        EnableDrawing = false;
    }

    /// <summary>
    /// Seat player in the chair and make it appear
    /// </summary>
    /// <param name="player"></param>
    public void SeatPlayer(Pawn player)
    {
        // Enable chair
        EnableDrawing = true;

        // Teleport and parent player to chair
        player.Transform = this.Transform;
        HasPlayer = true;
    }
}
