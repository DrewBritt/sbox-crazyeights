using System.Linq;
using Sandbox;

namespace CrazyEights;

/// <summary>
/// Renders and provides collision for the deck of cards the players draw their cards from.
/// </summary>
public partial class DeckEntity : ModelEntity
{
    [Net, Change] protected int Count { get; set; }
    public Particles CardStackParticles { get; protected set; }

    public DeckEntity()
    {
        Transmit = TransmitType.Always;
    }

    public override void Spawn()
    {
        // Position this entity relative to the GameTable entity placed on the map.
        var spawn = Entity.All.OfType<DeckSpawn>().Where(d => d.SpawnTarget == SpawnTarget.Deck).FirstOrDefault();
        if(spawn.IsValid())
            Transform = spawn.Transform;

        Tags.Add("deck");
        EnableAllCollisions = true;
        SetupPhysicsFromAABB(PhysicsMotionType.Keyframed, new Vector3(-5, -4, 0), new Vector3(5, 4, 5));
    }

    public override void ClientSpawn()
    {
        // Create particle system on Deck creation.
        base.ClientSpawn();
        CardStackParticles = Particles.Create("particles/cards/card_stack.vpcf");
    }

    [Event.Tick.Client]
    public void OnTickClient()
    {
        // Update particle system every tick.
        UpdateParticles();
    }

    /// <summary>
    /// Update Control Points of the CardStackParticles.
    /// </summary>
    protected virtual void UpdateParticles()
    {
        CardStackParticles.SetPosition(0, Position);
        CardStackParticles.SetPositionComponent(1, 0, Count);
    }

    [Event.Tick.Server]
    public virtual void OnTickServer()
    {
        var deck = GameManager.Current.PlayingDeck;
        if(deck == null) return;

        Count = deck.Count;
    }

    /// <summary>
    /// Called on Client when Count variable has changed. Used to update CardStackParticles to display current Count size.
    /// As card_stack.vpcf uses an Instant Emitter, the particle system has to be recreated to repopulate it with a new Count.
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    public void OnCountChanged(int oldValue, int newValue)
    {
        if(oldValue == newValue) return;

        if(CardStackParticles != null)
        {
            CardStackParticles.Destroy(true);
            CardStackParticles = Particles.Create("particles/cards/card_stack.vpcf");
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if(Game.IsClient && CardStackParticles != null)
            CardStackParticles.Destroy(true);
    }
}
