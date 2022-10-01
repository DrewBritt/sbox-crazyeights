using System.Linq;
using Sandbox;
using Sandbox.Component;

namespace CrazyEights;

public partial class Pawn : AnimatedEntity
{
    [Net, Predicted] public PawnAnimator Animator { get; set; }
    public ClothingContainer Clothing = new();
    public WorldNameplate Nameplate;

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

    public override void FrameSimulate(Client cl)
    {
        base.FrameSimulate(cl);

        Animator.FrameSimulate(cl, this, null);

        UpdateEyesTransforms();
        UpdateBodyGroups();
        CheckNameplates();

        if(Game.Current.CurrentPlayer == this)
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

        Game.Current.Hud.ActivateCrosshair();
        var pawn = tr.Entity as Pawn;
        pawn.Nameplate.Activate();
    }

    // Interactable looked at last frame. Possibly null.
    ModelEntity lastLookedAt;
    private void CheckInteractables()
    {
        // Trace for interactable object
        var ent = GetInteractableEntity();

        // If hit entity is either a Card in the player's hand, or the Deck
        if(ent.IsValid() && Hand != null && (Hand.Cards.Contains(ent) || ent is Deck))
        {
            Game.Current.Hud.ActivateCrosshair();
            if(ent != lastLookedAt)
                Sound.FromScreen("click1");

            // Apply glow if found
            var glow = ent.Components.GetOrCreate<Glow>();
            glow.Enabled = true;
            glow.Color = new Color(1f, 1f, 1f, 1f);

            // Disable glow if looking at new interactable
            if(lastLookedAt.IsValid() && ent != lastLookedAt && lastLookedAt.Components.TryGet<Glow>(out var previousGlow))
                previousGlow.Enabled = false;
            lastLookedAt = ent as ModelEntity;

            // Interaction
            if(Input.Pressed(InputButton.PrimaryAttack))
                CardInteract(ent);

            return;
        }

        // Disable glow if not looking at anything
        if(lastLookedAt.IsValid() && lastLookedAt.Components.TryGet<Glow>(out var lastGlow))
        {
            lastGlow.Enabled = false;
            lastLookedAt = null;
        }
    }

    public Entity GetInteractableEntity()
    {
        if(Game.Current.CurrentPlayer != this) return null;

        var tr = Trace.Ray(EyePosition, EyePosition + EyeRotation.Forward * 100f)
                .WithAnyTags("card", "deck")
                .EntitiesOnly()
                .Run();

        if(Hand == null || (!Hand.Cards.Contains(tr.Entity) && tr.Entity is not Deck)) return null;
        return tr.Entity;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Nameplate?.Delete();
    }
}
