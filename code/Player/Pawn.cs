using System.Linq;
using Sandbox;

namespace CrazyEights;

public partial class Pawn : AnimatedEntity
{
    public ClothingContainer Clothing = new();
    [Net] public PlayerHand Hand { get; set; }
    [Net, Predicted] public PawnAnimator Animator { get; set; }
    public PlayerChair PlayerChair { get; set; }

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

    public override void FrameSimulate(Client cl)
    {
        base.FrameSimulate(cl);

        if(Input.Pressed(InputButton.PrimaryAttack))
        {
            var tr = Trace.Ray(EyePosition, EyePosition + EyeRotation.Forward * 100f)
                .WithAnyTags("card", "deck")
                .EntitiesOnly()
                .Run();
            if(tr.Hit)
                GamePieceInteract(tr.Entity);
        }

        Animator.FrameSimulate(cl, this, null);

        UpdateEyesTransforms();
        UpdateBodyGroups();
        CheckNameplates();

        var attachment = GetAttachment("handStartPos").GetValueOrDefault();
        DebugOverlay.Line(attachment.Position, Position + Vector3.Up * 43 );
    }

    private void GamePieceInteract(Entity piece)
    {
        if(IsServer) return;

        if(piece is not Deck && piece is not CardEntity) return;

        if(piece is Deck)
            ConsoleSystem.Run($"ce_drawcard {Client.IsBot}");

        if(piece is CardEntity)
        {
            var card = piece as CardEntity;
            if(card.Suit == CardSuit.Wild)
                Game.Current.Hud.OpenSuitSelection(card);
            else
                ConsoleSystem.Run($"ce_playcard {card.NetworkIdent} 0 {Client.IsBot}");
        }
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
        var tr = Trace.Ray(EyePosition, EyePosition + EyeRotation.Forward * 300f)
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

    /// <summary>
    /// Calculates and force-plays a card from this pawn's hand.
    /// Used for bots/AFK behavior.
    /// </summary>
    public void ForcePlayCard()
    {
        // Now lets calculate what card to play
        var playable = Hand.PlayableCards();

        // Draw if no cards are currently playable
        if(!playable.Any())
        {
            ConsoleSystem.Run($"ce_drawcard {Client.IsBot}");
            return;
        }

        // Try to play an action card first
        var actionCards = playable.Where(c => c.Rank == CardRank.Draw2 || c.Rank == CardRank.Reverse || c.Rank == CardRank.Skip);
        var numberCards = playable.Where(c => c.Rank < (CardRank)10);
        if(actionCards.Any())
        {
            // Play an action card 33% of the time, or if there are no number cards in the hand
            int rand = Rand.Int(1, 3);
            if(rand == 1 || !numberCards.Any())
            {
                // Get random action card
                var cards = actionCards.ToList();
                int index = Rand.Int(0, cards.Count - 1);
                var card = cards[index];

                ConsoleSystem.Run($"ce_playcard {card.NetworkIdent} 0 {Client.IsBot}");
                return;
            }
        }

        // Then try to play a wild card
        var wildCards = playable.Where(c => c.Suit == CardSuit.Wild);
        if(wildCards.Any())
        {
            // Play a wild 20% of the time, or if there are only wild cards in the hand (no numbers/action as determined above)
            int rand = Rand.Int(1, 5);
            if(rand == 1 || wildCards.SequenceEqual(playable))
            {
                // Get random wild card
                var cards = wildCards.ToList();
                int index = Rand.Int(0, cards.Count - 1);
                var card = cards[index];

                ConsoleSystem.Run($"ce_playcard {card.NetworkIdent} {Hand.GetMostPrevelantSuit()} {Client.IsBot}");
                return;
            }
        }

        // Otherwise, just play a regular number card
        var numberList = numberCards.ToList();
        int randIndex = Rand.Int(0, numberList.Count - 1);
        var cardToPlay = numberList[randIndex];
        ConsoleSystem.Run($"ce_playcard {cardToPlay.NetworkIdent} 0 {Client.IsBot}");
    }
}
