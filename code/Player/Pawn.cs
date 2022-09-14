using Sandbox;

namespace CrazyEights;

public partial class Pawn : AnimatedEntity
{
    public ClothingContainer Clothing = new();
    [Net, Local] public Hand Hand { get; set; }
    [Net, Predicted] public PawnAnimator Animator { get; set; }

    public Pawn() { }

    public Pawn(Client cl) : this()
    {
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
        EnableHideInFirstPerson = false;

        base.Spawn();
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

    private void UpdateEyesTransforms()
    {
        var eyes = GetAttachment("eyes", false) ?? default;
        EyeLocalPosition = eyes.Position + Vector3.Up * 2f - Vector3.Forward * 4f;
        EyeLocalRotation = Input.Rotation;

        Position = Position.WithZ(35);
    }

    private void UpdateBodyGroups()
    {
        if(IsClient && IsLocalPawn)
            SetBodyGroup("Head", 1);
        else
            SetBodyGroup("Head", 0);
    }

    public override void PostCameraSetup(ref CameraSetup setup)
    {
        setup.ZNear = 0.1f;
        base.PostCameraSetup(ref setup);
    }
}
