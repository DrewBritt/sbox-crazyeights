using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace CrazyEights;

/// <summary>
/// Encapsulates a pile/stack of cards, using Deck functionality and overriding where necessary.
/// </summary>
public partial class Pile : Deck
{
    // Don't generate a deck on instantiation, instead we'll add cards ourselves (discard pile).
    public Pile() { }

    /// <summary>
    /// Add card on "top" of pile (beginning of list).
    /// </summary>
    /// <param name="card"></param>
    public override void AddCard(Card card)
    {
        Cards.Insert(0, card);
    }

    /// <summary>
    /// Add cards on "top" of pile (beginning of list).
    /// </summary>
    /// <param name="cards"></param>
    public override void AddCards(IList<Card> cards)
    {
        foreach(var c in cards)
            Cards.Insert(0, c);
    }
}
