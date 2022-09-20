using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace CrazyEights;

/// <summary>
/// Encapsulates a player's current hand of cards
/// </summary>
public partial class PlayerHand : BaseNetworkable
{
    [Net] public Pawn Owner { get; set; }
    [Net] public IList<CardEntity> Cards { get; set; }

    public void AddCard(Card card)
    {
        CardEntity cardEnt = new CardEntity();
        cardEnt.Transform = Owner.Transform;
        cardEnt.Card = card;
        cardEnt.SetCard(To.Single(Owner.Client), card.Rank, card.Suit);
        Cards.Add(cardEnt);
    }

    public void AddCards(IList<Card> cards)
    {
        foreach(var c in cards)
            AddCard(c);
    }

    public void RemoveCard(CardEntity card)
    {
        if(card.IsValid())
        {
            Cards.Remove(card);
            card.Delete();
        }
    }

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
