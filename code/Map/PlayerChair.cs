using System.ComponentModel.DataAnnotations;
using Sandbox;
using Editor;

namespace CrazyEights;

[Model]
[SupportsSolid]
[Library("ce_chair")]
[Display(Name = "Crazy Eights Player Chair", Description = "A chair in which a player's pawn will spawn and sit in.")]
[HammerEntity]
public partial class PlayerChair : ModelEntity
{
    public bool HasPlayer { get; private set; } = false;
    private Pawn player;

    public override void Spawn()
    {
        base.Spawn();

        SetupPhysicsFromModel(PhysicsMotionType.Static);
        EnableAllCollisions = false;
        EnableDrawing = false;
    }

    /// <summary>
    /// Seats player in the chair and makes it visible
    /// </summary>
    /// <param name="player"></param>
    public void SeatPlayer(Pawn player)
    {
        this.player = player;
        EnableDrawing = true;
        player.Transform = this.Transform;
        player.Position += (this.Transform.Rotation.Forward * 2f) + (Vector3.Up * 4f);
        player.PlayerChair = this;
        HasPlayer = true;
    }

    /// <summary>
    /// Frees chair from use and makes it invisible
    /// </summary>
    public void RemovePlayer()
    {
        if(!HasPlayer) return;

        EnableDrawing = false;
        player.PlayerChair = null;
        HasPlayer = false;
    }
}
