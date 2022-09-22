using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace CrazyEights;

/// <summary>
/// Encapsulates a player's current hand of cards.
/// </summary>
public partial class PlayerHand : BaseNetworkable
{
    /// <summary>
    /// Pawn in which this Hand belongs to.
    /// </summary>
    [Net] public Pawn Owner { get; set; }
    /// <summary>
    /// CardEntity's (and therefore Cards) in this hand.
    /// </summary>
    [Net] public IList<CardEntity> Cards { get; set; }

    public Entity HandStartPos;

    public void Initialize()
    {
        HandStartPos = new();
        HandStartPos.SetParent(Owner, "handStartPos");
    }

    /// <summary>
    /// Spawn a CardEntity into the world, set its value on the appropriate client, and add it to this Hand.
    /// </summary>
    /// <param name="card"></param>
    public void AddCard(Card card)
    {
        CardEntity cardEnt = new CardEntity();
        cardEnt.SetParent(Owner, "handStartPos");
        var attachment = Owner.GetAttachment("handStartPos").GetValueOrDefault();
        cardEnt.LocalPosition = Vector3.Forward * Cards.Count;
        cardEnt.LocalRotation = Rotation.LookAt(attachment.Position - (Owner.Position + Vector3.Up * 43), Vector3.Up);
        cardEnt.Card = card; // Set on server
        Cards.Add(cardEnt);
        cardEnt.SetCard(To.Single(Owner.Client), card.Rank, card.Suit); // Then on client
    }

    /// <summary>
    /// Spawn and add a list of CardEntity's and add it to this Hand.
    /// </summary>
    /// <param name="cards"></param>
    public void AddCards(IList<Card> cards)
    {
        foreach(var c in cards)
            AddCard(c);
    }

    /// <summary>
    /// Despawn and remove a CardEntity from this Hand.
    /// </summary>
    /// <param name="card"></param>
    public void RemoveCard(CardEntity card)
    {
        if(card.IsValid())
        {
            Cards.Remove(card);
            card.Delete();
        }
    }

    /// <summary>
    /// Despawn and remove all CardEntity's from this Hand.
    /// </summary>
    public void ClearCards()
    {
        foreach(var c in Cards)
            c.Delete();
        Cards.Clear();
    }

    #region Hand Analysis (for bots/AFK players)

    /// <summary>
    /// Returns an IEnumerable of all currently playable cards.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<CardEntity> PlayableCards()
    {
        var playableCards = Cards.OrderBy(c => c.Card.Suit)
            .ThenBy(c => c.Card.Rank)
            .Where(c => c.Card.IsPlayable());
        return playableCards;
    }

    /// <summary>
    /// Gets the most prevelant suit in this hand (except for WILD)
    /// </summary>
    /// <returns></returns>
    public CardSuit GetMostPrevelantSuit()
    {
        var mostPrevSuit = Cards.GroupBy(c => c.Suit)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Where(s => s != CardSuit.Wild)
            .FirstOrDefault();
        return mostPrevSuit;
    }

    #endregion
}
