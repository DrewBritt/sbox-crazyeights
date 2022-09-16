using System.Collections.Generic;
using System.Linq;

namespace CrazyEights;

/// <summary>
/// Encapsulates a player's current hand of cards, using Deck functionality.
/// </summary>
public partial class Hand : Deck
{
    public Hand() : base() { }

    public override void Spawn()
    {
        // Do nothing on spawn, as we simply want an empty list of cards to add to.
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
