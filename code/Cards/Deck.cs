using System;
using System.Collections.Generic;
using Sandbox;

namespace CrazyEights;

/// <summary>
/// A deck/list of cards with deck management functionality to be used for a variety of purposes.
/// </summary>
public partial class Deck : Entity
{
    /// <summary>
    /// All cards in this deck
    /// </summary>
    [Net, Local] public IList<Card> Cards { get; set; }

    public Deck()
    {
        Transmit = TransmitType.Always;
    }

    public override void Spawn()
    {
        GenerateDeck();
    }

    /// <summary>
    /// Add a card into the deck
    /// </summary>
    /// <param name="card"></param>
    public virtual void AddCard(Card card)
    {
        Cards.Add(card);
    }

    /// <summary>
    /// Add a range of cards into the deck
    /// </summary>
    /// <param name="cards"></param>
    public virtual void AddCards(IList<Card> cards)
    {
        foreach(var c in cards)
            Cards.Add(c);
    }

    /// <summary>
    /// Remove a card from the deck
    /// </summary>
    /// <param name="card"></param>
    public void RemoveCard(Card card)
    {
        Cards.Remove(card);
    }

    /// <summary>
    /// Remove a range of cards from the deck
    /// </summary>
    /// <param name="cards"></param>
    public void RemoveCards(IList<Card> cards)
    {
        foreach(var c in cards)
            Cards.Remove(c);
    }

    /// <summary>
    /// Returns card from top/"front" of pile
    /// </summary>
    /// <returns></returns>
    public Card GetTopCard() => Cards[0];

    /// <summary>
    /// Returns (AND REMOVES FROM DECK) card from top/"front" of pile
    /// </summary>
    /// <returns></returns>
    public Card GrabTopCard()
    {
        Card c = GetTopCard();
        Cards.Remove(c);
        return c;
    }

    /// <summary>
    /// Generate initial deck of cards to be used in play
    /// </summary>
    private void GenerateDeck()
    {
        Cards = new List<Card>();

        // 112 cards: 80 numbered, 24 action, 8 wild
        // First, colored cards
        for(int suit = 0; suit < 4; suit++)
        {
            // Generate two of each colored card
            for(int i = 0; i < 2; i++)
            {
                // Generate one of every number + action card
                for(int num = 0; num < 13; num++)
                {
                    Card card = new Card()
                    {
                        Suit = (CardSuit)suit,
                        Rank = (CardRank)num
                    };
                    Cards.Add(card);
                }
            }
        }

        // Then, 4 of each wild card
        for(int i = 0; i < 4; i++)
            for(int rank = 13; rank < 15; rank++)
            {
                Card wildCard = new Card()
                {
                    Suit = CardSuit.Wild,
                    Rank = (CardRank)rank
                };
                Cards.Add(wildCard);
            }
    }

    /// <summary>
    /// Shuffles the cards in this deck for randomization
    /// </summary>
    public void Shuffle()
    {
        // Fisher-Yates bitch!
        Random random = new();
        for(int i = Cards.Count-1; i > 1; i--)
        {
            int rand = random.Next(i + 1);
            var temp = Cards[rand];
            Cards[rand] = Cards[i];
            Cards[i] = temp;
        }
    }
}
