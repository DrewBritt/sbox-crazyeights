using System.ComponentModel.DataAnnotations;
using Sandbox;
using SandboxEditor;

namespace CrazyEights;

[Model]
[SupportsSolid]
[Library("ce_table")]
[Display(Name = "Crazy Eights Table", Description = "A table in which cards will appear on.")]
[HammerEntity]
public partial class GameTable : ModelEntity
{
    public override void Spawn()
    {
        base.Spawn();

        SetupPhysicsFromModel(PhysicsMotionType.Static);
        EnableAllCollisions = true;
        EnableDrawing = true;
    }
}
