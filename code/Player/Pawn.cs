using Sandbox;

namespace CrazyEights;

public partial class Pawn : AnimatedEntity
{
    public ClothingContainer Clothing = new();
    [Net, Local] public Hand Hand { get; set; }
    [Net, Predicted] public PawnAnimator Animator { get; set; }

    private WorldNameplate Nameplate;

    public Pawn() { }

    public Pawn(Client cl) : this()
    {
        if(cl.IsBot)
            Clothing.LoadRandomClothes();
        else
            Clothing.LoadFromClient(cl);

        Clothing.DressEntity(this);
    }

	public CameraMode CameraMode
    {
        get => Components.Get<CameraMode>();
        set => Components.Add(value);
    }

    public override void Spawn()
    {
        SetModel("models/citizen/crazyeights_citizen.vmdl");

        Hand = new Hand();

        EnableDrawing = true;
        EnableAllCollisions = true;
        EnableHitboxes = true;
        EnableHideInFirstPerson = false;

        Tags.Add("player");

        base.Spawn();
    }

    public override void ClientSpawn()
    {
        base.ClientSpawn();

        // Spawn nameplate if not local player's pawn
        if(Local.Pawn != this)
            Nameplate = new(this);
    }

    public override void Simulate(Client cl)
    {
        base.Simulate(cl);
        Animator.Simulate(cl, this, null);

        UpdateEyesTransforms();
    }

    public override void FrameSimulate(Client cl)
    {
        base.FrameSimulate(cl);
        Animator.FrameSimulate(cl, this, null);

        UpdateEyesTransforms();
        UpdateBodyGroups();
        CheckNameplates();
    }

	public override void BuildInput(InputBuilder input)
    {
        base.BuildInput(input);

        if(input.StopProcessing)
            return;

        var inputAngles = input.ViewAngles;
        var clampedAngles = new Angles(
            inputAngles.pitch.Clamp(-45, 60),
            inputAngles.yaw.Clamp(-75, 75),
            inputAngles.roll
        );

        input.ViewAngles = clampedAngles;
    }

    public override void PostCameraSetup(ref CameraSetup setup)
    {
        setup.ZNear = 0.1f;
        base.PostCameraSetup(ref setup);
    }

    private void UpdateEyesTransforms()
    {
        var eyes = GetAttachment("eyes", false) ?? default;
        EyeLocalPosition = eyes.Position + Vector3.Up * 2f - Vector3.Forward * 4f;
        EyeLocalRotation = Input.Rotation;
    }

    private void UpdateBodyGroups()
    {
        if(IsClient && IsLocalPawn)
            SetBodyGroup("Head", 1);
        else
            SetBodyGroup("Head", 0);
    }

    private void CheckNameplates()
    {
        var tr = Trace.Ray(EyePosition, EyePosition + EyeRotation.Forward * 100f)
            .UseHitboxes()
            .WithTag("player")
            .Ignore(this)
            .Run();

        if(!tr.Hit) return;

        var pawn = tr.Entity as Pawn;
        pawn.Nameplate.Appeared = 0;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Nameplate?.Delete();
    }
}
