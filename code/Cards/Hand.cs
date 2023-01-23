using System.Collections.Generic;
using System.Linq;

namespace CrazyEights;

public class Hand : Deck
{
    /// <summary>
    /// Associated display component for rendering CardEntities.
    /// </summary>
    private HandDisplayComponent HandDisplay { get; set; }

    public Hand(Player player)
    {
        Initialize(player);
    }

    private void Initialize(Player player)
    {
        HandDisplay = new HandDisplayComponent();
        player.Components.Add(HandDisplay);
    }

    protected override void Initialize() { }

    public override void AddCard(Card card)
    {
        base.AddCard(card);

        if(HandDisplay != null)
            HandDisplay.AddCard(card);
    }

    public override void RemoveCard(Card card)
    {
        base.RemoveCard(card);

        if(HandDisplay != null)
            HandDisplay.RemoveCard(card);
    }

    public override void ClearCards()
    {
        base.ClearCards();

        if(HandDisplay != null)
            HandDisplay.ClearCards();
    }

    #region Hand Analysis (for bots/AFK players)

    /// <summary>
    /// Returns an IEnumerable of all currently playable cards.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Card> PlayableCards()
    {
        var playableCards = Cards.OrderBy(c => c.Suit)
            .ThenBy(c => c.Rank)
            .Where(c => c.IsPlayable());
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
