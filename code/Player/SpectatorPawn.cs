using Sandbox;

namespace CrazyEights;

public partial class SpectatorPawn : Entity
{
    public SpectatorPawn() { }

    public CameraMode CameraMode
    {
        get => Components.Get<CameraMode>();
        set => Components.Add(value);
    }

    public override void FrameSimulate(Client cl)
    {
        base.FrameSimulate(cl);

        CheckNameplates();
    }

    private void CheckNameplates()
    {
        var cam = CameraMode;

        // Trace for players
        var tr = Trace.Ray(cam.Position, cam.Position + cam.Rotation.Forward * 300f)
            .UseHitboxes()
            .WithTag("player")
            .Ignore(this)
            .Run();

        if(!tr.Hit) return;

        Game.Current.Hud.EnableCrosshair();
        var pawn = tr.Entity as Pawn;
        pawn.Nameplate.Activate();
    }
}
