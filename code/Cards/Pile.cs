﻿using System.Collections.Generic;
using Sandbox;

namespace CrazyEights;

/// <summary>
/// Encapsulates a pile/stack of cards, using Deck functionality.
/// </summary>
public partial class Pile : Deck
{
    // Separate property in order to network to entire lobby (Deck has [Net, Local])
    [Net] public new IList<Card> Cards { get; set; }

    public Pile() : base() { }

    public override void Spawn()
    {
        // Do nothing on spawn, as we simply want an empty list of cards to add to.
    }

    /// <summary>
    /// Put card on "top" of pile (beginning of list)
    /// </summary>
    /// <param name="card"></param>
    public override void AddCard(Card card)
    {
        Cards.Insert(0, card);
    }

    /// <summary>
    /// Put cards on "top" of pile (beginning of list)
    /// </summary>
    /// <param name="cards"></param>
    public override void AddCards(IList<Card> cards)
    {
        foreach(var c in cards)
            Cards.Insert(0, c);
    }
}
