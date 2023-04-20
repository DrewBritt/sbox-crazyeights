using Sandbox;

namespace CrazyEights;

public partial class SpectatorController : EntityComponent<Spectator>, ISingletonComponent
{
    /// <summary>
	/// Normalized accumulation of Input.AnalogLook
	/// </summary>
    [ClientInput] public Angles LookInput { get; protected set; }
    [ClientInput] public Vector3 MoveInput { get; protected set; }
    [ClientInput] public ButtonState RunButton { get; protected set; }
    [ClientInput] public ButtonState DuckButton { get; protected set; }

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
    public Ray AimRay
    {
        get
        {
            return new Ray(Entity.Position, LookInput.Forward);
        }
    }

    private float baseMoveSpeed = 300f;
    private float moveSpeed = 1f;

    public virtual void BuildInput()
    {
        LookInput += Input.AnalogLook;
        MoveInput = Input.AnalogMove;
        RunButton = Input.Down("Run");

        Input.ClearActions();
        Input.AnalogMove = default;
    }

    public virtual void FrameSimulate(IClient cl)
    {
        CheckNameplates();
    }

    public virtual void Simulate(IClient cl)
    {
        FreeMove();
    }

    private void FreeMove()
    {
        moveSpeed = 1f;

        if(RunButton.IsDown) moveSpeed = 5f;
        if(DuckButton.IsDown) moveSpeed = .25f;
        var mv = MoveInput.Normal * baseMoveSpeed * RealTime.Delta * Entity.Rotation * moveSpeed;

        Entity.Position += mv;
        Entity.Rotation = Rotation.From(LookInput);
    }

    /// <summary>
    /// Traces for Players and activates the nameplate above them.
    /// </summary>
    private void CheckNameplates()
    {
        // Trace for players
        var tr = Trace.Ray(AimRay.Position, AimRay.Position + AimRay.Forward * 300f)
            .UseHitboxes()
            .WithTag("player")
            .Ignore(Entity)
            .Run();

        if(!tr.Hit) return;

        GameManager.Current.Hud.ActivateCrosshair();
        var pawn = tr.Entity as Player;
        pawn.Nameplate?.Activate();
    }
}
