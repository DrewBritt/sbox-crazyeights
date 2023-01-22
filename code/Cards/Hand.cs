using System.Collections.Generic;
using System.Linq;

namespace CrazyEights;

public class Hand : Deck
{
    // Don't generate a complete deck on Hand instantiation.
    public Hand() { }

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
