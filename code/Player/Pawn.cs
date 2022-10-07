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

        // Create SuitSelection if local pawn
        if(Local.Pawn == this)
        {
            SuitSelection = new SuitSelectionEntity();
            SuitSelection.Position = Position + (Vector3.Up * 40f) + (Rotation.Forward * 20f);
            SuitSelection.Rotation = Rotation;
            SuitSelection.LocalRotation = LocalRotation.RotateAroundAxis(Vector3.Up, 180f);
        }
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

    /// <summary>
    /// Traces for Players and activates the nameplate above them.
    /// </summary>
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
    /// <summary>
    /// Traces for Interactables (cards in the players's hand/the deck),
    /// and handles input + various effects.
    /// </summary>
    private void CheckInteractables()
    {
        // Disable glow if it was enabled on a card the player didn't play, and they are no longer the player.
        // Then we return to avoid tracing every frame we don't need it.
        if(Game.Current.CurrentPlayer != this)
        {
            if(lastLookedAt.IsValid() && lastLookedAt.Components.TryGet<Glow>(out var glow))
            {
                glow.Enabled = false;
                lastLookedAt = null;
            }
            return;
        }

        // Trace for interactable object
        var ent = GetInteractableEntity();

        // If hit entity is either a Card in the player's hand, or the Deck:
        if(ent.IsValid())
        {
            Game.Current.Hud.ActivateCrosshair();
            if(ent != lastLookedAt) // Play sound once per unique lastLookedAt
                Sound.FromScreen("click1");

            // Disables glow on previously looked at cards when moving from one card to another.
            if(lastLookedAt.IsValid() && ent != lastLookedAt && lastLookedAt.Components.TryGet<Glow>(out var oldGlow))
                oldGlow.Enabled = false;

            lastLookedAt = ent as ModelEntity;

            // Apply glow if found
            var glow = ent.Components.GetOrCreate<Glow>();
            glow.Enabled = true;
            glow.Color = new Color(1f, 1f, 1f, 1f);

            // Interaction
            if(Input.Pressed(InputButton.PrimaryAttack))
                CardInteract(ent);
        } else
        {
            // Otherwise we're looking at nothing and want to disable lastLookedAt's glow.
            if(lastLookedAt.IsValid() && lastLookedAt.Components.TryGet<Glow>(out var lastGlow))
                lastGlow.Enabled = false;
            lastLookedAt = null;
        }
    }

    /// <summary>
    /// Gets the Interactable (Card/Deck) entity in front of the player's eyesight.
    /// </summary>
    public Entity GetInteractableEntity()
    {
        if(Game.Current.CurrentPlayer != this) return null;

        if(Hand == null) return null;

        var tr = Trace.Ray(EyePosition, EyePosition + EyeRotation.Forward * 100f)
                .WithAnyTags("card", "deck", "suitselection")
                .EntitiesOnly()
                .IncludeClientside()
                .Run();

        if(!tr.Entity.IsValid()) return null;

        if(tr.Entity is not Deck)
            if(!Hand.Cards.Contains(tr.Entity) && !tr.Entity.Tags.Has("suitselection"))
                return null;

        return tr.Entity;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Nameplate?.Delete();
    }
}
