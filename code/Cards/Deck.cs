using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace CrazyEights;

/// <summary>
/// A deck/list of cards with deck management functionality to be used for a variety of purposes.
/// </summary>
public class Deck
{
    public IList<Card> Cards { get; protected set; } = new List<Card>();
    public int Count => Cards.Count;

    private DeckEntity PlayingDeck => GameManager.Current.PlayingDeckEntity;

    public Deck()
    {
        Initialize();
    }
    
    protected virtual void Initialize()
    {
        if(PlayingDeck == null)
            GameManager.Current.PlayingDeckEntity = new DeckEntity();

        GenerateDeck();
    }

    /// <summary>
    /// Add a card into the deck.
    /// </summary>
    /// <param name="card"></param>
    public virtual void AddCard(Card card)
    {
        Cards.Add(card);
    }

    /// <summary>
    /// Add a range of cards into the deck.
    /// </summary>
    /// <param name="cards"></param>
    public virtual void AddCards(IList<Card> cards)
    {
        foreach(var card in cards)
            AddCard(card);
    }

    /// <summary>
    /// Remove a card from the deck.
    /// </summary>
    /// <param name="card"></param>
    public virtual void RemoveCard(Card card)
    {
        Cards.Remove(card);
    }

    /// <summary>
    /// Clear cards in deck.
    /// </summary>
    public virtual void ClearCards()
    {
        Cards.Clear();
    }

    /// <summary>
    /// Returns card from top/"front" of pile.
    /// </summary>
    /// <returns></returns>
    public virtual Card? GetTopCard() => Cards.FirstOrDefault();

    /// <summary>
    /// Returns (AND REMOVES FROM DECK) card from top/"front" of pile.
    /// </summary>
    /// <returns></returns>
    public virtual Card GrabTopCard()
    {
        Card card = GetTopCard();
        RemoveCard(card);

        // Refill Deck with cards from discard pile if deck is now empty
        if(!Cards.Any())
        {
            // Add discard (except for top card)
            var discard = GameManager.Current.DiscardPile;
            var discardTop = discard.GrabTopCard();
            AddCards(discard.Cards);
            Shuffle();

            // Clear played Wild cards back to Wild (instead of their played color)
            var wilds = Cards.Where(c => c.Rank > (CardRank)12).ToList();
            foreach(var c in wilds)
                c.Suit = CardSuit.Wild;

            // Clear discard and add old top card back
            discard.Cards.Clear();
            discard.AddCard(discardTop);
        }

        return card;
    }

    /// <summary>
    /// Generate initial deck of cards to be used in play.
    /// </summary>
    private void GenerateDeck()
    {
        // 112 cards: 80 numbered, 24 action, 8 wild.
        // First, colored cards.
        for(int suit = 0; suit < 4; suit++)
        {
            // Generate two of each colored card.
            for(int i = 0; i < 2; i++)
            {
                // Generate one of every number + action card.
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

        // Then, 4 of each wild card.
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
    /// Shuffles the cards in this deck for randomization.
    /// </summary>
    public virtual void Shuffle()
    {
        // Fisher-Yates bitch!
        Random random = new();
        for(int i = Cards.Count - 1; i > 1; i--)
        {
            int rand = random.Next(i + 1);
            var temp = Cards[rand];
            Cards[rand] = Cards[i];
            Cards[i] = temp;
        }
    }
}
