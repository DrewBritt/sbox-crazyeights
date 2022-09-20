using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace CrazyEights;

/// <summary>
/// Encapsulates a pile/stack of cards, using Deck functionality.
/// </summary>
public partial class Pile : Deck
{
    public Pile() : base() { }

    /// <summary>
    /// Displays the last played card/the top card on the pile.
    /// </summary>
    [Net] public CardEntity TopCardEntity { get; set; }

    public override void Spawn()
    {
        // Pile should generate empty, not with a fresh deck.
        Cards = new List<Card>();

        // Position relative to table.
        var spawn = Entity.All.OfType<DeckSpawn>().Where(d => d.SpawnTarget == SpawnTarget.Pile).FirstOrDefault();
        if(spawn.IsValid())
            Transform = spawn.Transform;

        // Position/Rotate TopCardEntity relative to pile and particle system
        TopCardEntity = new CardEntity();
        TopCardEntity.Transform = spawn.Transform;
        TopCardEntity.Rotation = TopCardEntity.Rotation.RotateAroundAxis(Vector3.Right, 180).RotateAroundAxis(Vector3.Up, 90);
    }

    protected override void UpdateParticles()
    {
        CardStackParticles.SetPosition(0, Position);

        // Count - 1 as the top card is handled by TopCardEntity
        CardStackParticles.SetPositionComponent(1, 0, Count-1);
    }

    public override void OnTickServer()
    {
        base.OnTickServer();
        TopCardEntity.Position = TopCardEntity.Position.WithZ(Position.z + (0.05f * Count));
    }

    /// <summary>
    /// Put card on "top" of pile (beginning of list).
    /// </summary>
    /// <param name="card"></param>
    public override void AddCard(Card card)
    {
        Cards.Insert(0, card);
    }

    /// <summary>
    /// Put cards on "top" of pile (beginning of list).
    /// </summary>
    /// <param name="cards"></param>
    public override void AddCards(IList<Card> cards)
    {
        foreach(var c in cards)
            Cards.Insert(0, c);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if(IsServer)
            TopCardEntity.Delete();
    }
}
