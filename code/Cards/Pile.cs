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

    [Net] public CardEntity TopCardEntity { get; set; }

    public override void Spawn()
    {
        Cards = new List<Card>();
        var table = Entity.All.OfType<GameTable>().FirstOrDefault();
        if(table.IsValid())
        {
            Transform = table.Transform;
            Position = Position.WithZ(55);
            Position = Position.WithX(Position.x + 10);
        }

        TopCardEntity = new CardEntity();
        TopCardEntity.Transform = table.Transform;
        TopCardEntity.Position = TopCardEntity.Position.WithZ(55).WithX(Position.x);
        TopCardEntity.Rotation = TopCardEntity.Rotation.RotateAroundAxis(Vector3.Right, 180).RotateAroundAxis(Vector3.Up, 90);
    }

    protected override void UpdateParticles()
    {
        base.UpdateParticles();
    }

    public override void OnTickServer()
    {
        base.OnTickServer();
        TopCardEntity.Position = TopCardEntity.Position.WithZ(55 + (0.05f * Count));
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
