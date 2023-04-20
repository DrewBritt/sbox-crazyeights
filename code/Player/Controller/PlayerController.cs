﻿using Sandbox;
using Sandbox.Component;

namespace CrazyEights;

public partial class PlayerController : EntityComponent<Player>, ISingletonComponent
{
    /// <summary>
	/// Normalized accumulation of Input.AnalogLook
	/// </summary>
    [ClientInput] public Angles LookInput { get; set; }

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

    /// <summary>
    /// Called on player swapping in to this controller's pawn. Temporary workaround until Entity.OnClientActive is consistent.
    /// </summary>
    [ClientRpc]
    public void CreateSuitSelection()
    {
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
            lookInput.pitch.Clamp(-60, 50),
            lookInput.yaw.Clamp(Entity.LocalRotation.Yaw() - 80f, Entity.LocalRotation.Yaw() + 80f),
            0
        );
        LookInput = clampedAngles;
    }

    public virtual void Simulate(IClient cl)
    {

    }

    public virtual void FrameSimulate(IClient cl)
    {
        if(GameManager.Current.CurrentPlayer == Entity)
            GameManager.Current.Hud.ActivateCrosshair();

        CheckNameplates();
        CheckInteractables();
    }

    public void HideSuitSelection()
    {
        if(SuitSelection.IsValid())
            SuitSelection.Hide();
    }

    /// <summary>
    /// Gets the Interactable (Card/Deck) entity in front of the player's eyesight.
    /// </summary>
    public Entity GetInteractableEntity()
    {
        if(GameManager.Current.CurrentPlayer != Entity) return null;

        if(Entity.HandDisplay == null) return null;

        var tr = Trace.Ray(AimRay.Position, AimRay.Position + AimRay.Forward * 100f)
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
        if(GameManager.Current.CurrentPlayer == null) return;

        // Disable glow if it was enabled on the deck/a card the player didn't play, and they are no longer the player.
        // Then we return to avoid tracing every frame we don't need it.
        if(GameManager.Current.CurrentPlayer != Entity)
        {
            if(lastLookedAt.IsValid())
            {
                if(lastLookedAt is CardEntity && lastLookedAt.Components.TryGet<Glow>(out var lastGlow))
                    lastGlow.Enabled = false;
                else if(lastLookedAt is DeckEntity deck)
                    deck.CardStackParticles.SetPosition(2, Vector3.Zero);
            }
            return;
        }

        // Trace for interactable object
        var ent = GetInteractableEntity();

        // If hit entity is either a Card in the player's hand, or the Deck:
        if(ent.IsValid())
        {
            // Disables glow on previously looked at cards when moving from one card to another.
            if(lastLookedAt.IsValid() && ent != lastLookedAt && lastLookedAt.Components.TryGet<Glow>(out var oldGlow))
                oldGlow.Enabled = false;

            // Block effects and interact on non-playable card, 
            if(ent is CardEntity cardEnt && !ent.Tags.Has("suitselection"))
            {
                if(!cardEnt.IsPlayable()) return;
                cardEnt.LocalPosition += Vector3.Up * 6f;
            }

            if(ent != lastLookedAt) // Play sound once per unique lastLookedAt
                Sound.FromScreen("click1");

            lastLookedAt = ent as ModelEntity;

            // Apply glow for playable card or draw pile.
            if(lastLookedAt is CardEntity)
            {
                var glow = ent.Components.GetOrCreate<Glow>();
                glow.Enabled = true;
                glow.Color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                var particles = (ent as DeckEntity).CardStackParticles;
                particles.SetPosition(2, Vector3.One);
            }


            // Interaction
            if(Input.Pressed("Interact"))
                CardInteract(ent);
        }
        else
        {
            // Otherwise we're looking at nothing and want to disable lastLookedAt's glow.
            if(lastLookedAt.IsValid())
            {
                if(lastLookedAt is CardEntity && lastLookedAt.Components.TryGet<Glow>(out var lastGlow))
                    lastGlow.Enabled = false;
                else if(lastLookedAt is DeckEntity deck)
                    deck.CardStackParticles.SetPosition(2, Vector3.Zero);
            }

            lastLookedAt = null;
        }
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
        {
            ConsoleSystem.Run($"ce_drawcard {Entity.Client.IsBot}");
            Entity.Animator.PlayFacialPose(PlayerFacialPose.Negative);
            Entity.Animator.DidAction();
        }

        if(card is CardEntity cardEnt)
        {
            if(cardEnt.Suit == CardSuit.Wild)
            {
                // Wants to play a Wild card, open SuitSelection
                SuitSelection.Display(cardEnt.NetworkIdent, cardEnt.Rank);
                return;
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

            Entity.Animator.PlayFacialPose(PlayerFacialPose.Positive);
            Entity.Animator.DidAction();
        }
    }
}
