using System.Linq;
using Sandbox;
using Sandbox.Component;

namespace CrazyEights;

public partial class Pawn : AnimatedEntity
{
    [Net, Predicted] public PawnAnimator Animator { get; set; }
    public ClothingContainer Clothing = new();
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

    ModelEntity lastLookedAt;
    public override void FrameSimulate(Client cl)
    {
        base.FrameSimulate(cl);

        Animator.FrameSimulate(cl, this, null);

        UpdateEyesTransforms();
        UpdateBodyGroups();
        CheckNameplates();
        CheckInteractables();
    }

    public override void BuildInput(InputBuilder input)
    {
        base.BuildInput(input);

        if(input.StopProcessing)
            return;

        var inputAngles = input.ViewAngles;
        var clampedAngles = new Angles(
            inputAngles.pitch.Clamp(-60, 75),
            inputAngles.yaw.Clamp(-80, 80),
            inputAngles.roll
        );

        input.ViewAngles = clampedAngles;
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
        // Trace for players
        var tr = Trace.Ray(EyePosition, EyePosition + EyeRotation.Forward * 300f)
            .UseHitboxes()
            .WithTag("player")
            .Ignore(this)
            .Run();

        if(!tr.Hit) return;

        Game.Current.Hud.EnableCrosshair();
        var pawn = tr.Entity as Pawn;
        pawn.Nameplate.Appeared = 0;
    }
    
    private void CheckInteractables()
    {
        // Trace for interactable object
        var tr = Trace.Ray(EyePosition, EyePosition + EyeRotation.Forward * 100f)
                .WithAnyTags("card", "deck")
                .EntitiesOnly()
                .Run();

        // If hit entity is either a Card in the player's hand, or the Deck
        if(tr.Hit && Hand != null && (Hand.Cards.Contains(tr.Entity) || tr.Entity is Deck))
        {
            Game.Current.Hud.EnableCrosshair();

            // Apply glow if found
            var glow = tr.Entity.Components.GetOrCreate<Glow>();
            glow.Enabled = true;
            glow.Color = new Color(1f, 1f, 1f, 1f);

            // Disable glow if looking at new interactable
            if(lastLookedAt.IsValid() && tr.Entity != lastLookedAt && lastLookedAt.Components.TryGet<Glow>(out var previousGlow))
                previousGlow.Enabled = false;
            lastLookedAt = tr.Entity as ModelEntity;

            // Interaction
            if(Input.Pressed(InputButton.PrimaryAttack))
                CardInteract(tr.Entity);

            return;
        }

        // Disable glow if not looking at anything
        if(lastLookedAt.IsValid() && lastLookedAt.Components.TryGet<Glow>(out var lastGlow))
            lastGlow.Enabled = false;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Nameplate?.Delete();
    }
}
