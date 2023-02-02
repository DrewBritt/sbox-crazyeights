using System.Runtime.CompilerServices;
using Sandbox;
using Sandbox.Component;

namespace CrazyEights;

public partial class PlayerController : EntityComponent<Player>, ISingletonComponent
{
    /// <summary>
	/// Normalized accumulation of Input.AnalogLook
	/// </summary>
    [ClientInput] public Angles LookInput { get; protected set; }

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
            var eyeTransform = Entity.GetAttachment("eyes") ?? default;
            return new Ray(eyeTransform.Position, LookInput.Forward);
        }
    }

    /// <summary>
    /// Displays CardEntities in front of the player that allow suit selection when playing a Wild/Draw4.
    /// </summary>
    private SuitSelectionEntity SuitSelection { get; set; }

    protected override void OnActivate()
    {
        if(!Entity.IsLocalPawn) return;

        LookInput = Entity.Rotation.Angles().WithRoll(0);

        SuitSelection = new SuitSelectionEntity();
        SuitSelection.Position = Entity.Position + (Vector3.Up * 40f) + (Entity.Rotation.Forward * 20f);
        SuitSelection.Rotation = Entity.Rotation;
        SuitSelection.LocalRotation = Entity.LocalRotation.RotateAroundAxis(Vector3.Up, 180f);
    }

    public virtual void BuildInput()
    {
        if(Input.StopProcessing)
            return;

        // Calculate new ViewAngles from input diff, then clamp.
        var lookInput = (LookInput + Input.AnalogLook);
        var clampedAngles = new Angles(
            lookInput.pitch.Clamp(-60, 75),
            lookInput.yaw.Clamp(Entity.LocalRotation.Yaw() - 80f, Entity.LocalRotation.Yaw() + 80f),
            0
        );
        LookInput = clampedAngles;

        CheckEmoteWheel();
    }

    private void CheckEmoteWheel()
    {
        if(Input.Down(InputButton.SecondaryAttack))
            GameManager.Current.Hud.ActivateEmoteWheelOverlay();
    }

    public virtual void Simulate(IClient cl)
    {
        
    }

    public virtual void FrameSimulate(IClient cl)
    {
        CheckNameplates();
        CheckInteractables();
    }

    public void HideSuitSelection()
    {
        if(SuitSelection.IsValid())
            SuitSelection.Hide();
    }

    /// <summary>
    /// Traces for Players and activates the nameplate above them.
    /// </summary>
    protected void CheckNameplates()
    {
        
        // Trace for players
        var tr = Trace.Ray(Entity.Controller.AimRay.Position, Entity.Controller.AimRay.Position + Entity.Controller.AimRay.Forward * 300f)
            .UseHitboxes()
            .WithTag("player")
            .Ignore(Entity)
            .Run();

        if(!tr.Hit) return;

        
        GameManager.Current.Hud.ActivateCrosshair();
        var pawn = tr.Entity as Player;
        pawn.Nameplate.Activate();
    }

    // Interactable looked at last frame. Possibly null.
    ModelEntity lastLookedAt;
    /// <summary>
    /// Traces for Interactables (cards in the players's hand/the deck),
    /// and handles input + various effects.
    /// </summary>
    protected void CheckInteractables()
    {
        if(GameManager.Current.CurrentPlayer == null) return;

        // Disable glow if it was enabled on a card the player didn't play, and they are no longer the player.
        // Then we return to avoid tracing every frame we don't need it.
        if(GameManager.Current.CurrentPlayer != Entity)
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
            GameManager.Current.Hud.ActivateCrosshair();
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
        }
        else
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
        if(GameManager.Current.CurrentPlayer != Entity) return null;

        if(Entity.HandDisplay == null) return null;

        var tr = Trace.Ray(Entity.Controller.AimRay.Position, Entity.Controller.AimRay.Position + Entity.Controller.AimRay.Forward * 100f)
                .WithAnyTags("card", "deck", "suitselection")
                .EntitiesOnly()
                .IncludeClientside()
                .Run();

        if(!tr.Entity.IsValid()) return null;

        if(tr.Entity is not DeckEntity)
            if(!Entity.HandDisplay.Cards.Contains(tr.Entity as CardEntity) && !tr.Entity.Tags.Has("suitselection"))
                return null;

        return tr.Entity;
    }

    /// <summary>
    /// Player has attempted to interact with a card/the discard pile.
    /// </summary>
    /// <param name="card"></param>
    private void CardInteract(Entity card)
    {
        if(Game.IsServer) return;

        if(card is not DeckEntity && card is not CardEntity) return;

        // Player wishes to draw a card
        if(card is DeckEntity)
            ConsoleSystem.Run($"ce_drawcard {Entity.Client.IsBot}");

        if(card is CardEntity)
        {
            var cardEnt = card as CardEntity;
            if(cardEnt.Suit == CardSuit.Wild)
            {
                // Wants to play a Wild card, open SuitSelection
                SuitSelection.Display(cardEnt.NetworkIdent, cardEnt.Rank);
            }
            else if(cardEnt.Tags.Has("suitselection"))
            {
                // Has chosen suit for Wild card from SuitSelection card
                ConsoleSystem.Run($"ce_playcard {SuitSelection.CardNetworkIdent} {cardEnt.Suit}");
            }
            else
            {
                // Playing a normal card
                ConsoleSystem.Run($"ce_playcard {cardEnt.NetworkIdent} 0");
            }
        }
    }
}
