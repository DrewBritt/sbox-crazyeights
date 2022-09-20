﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace CrazyEights;

/// <summary>
/// A deck/list of cards with deck management functionality to be used for a variety of purposes.
/// </summary>
public partial class Deck : Entity
{
    /// <summary>
    /// All cards in this deck.
    /// </summary>
    public IList<Card> Cards { get; set; }

    public Particles CardStackParticles { get; set; }
    [Net] public int Count { get; set; }

    public Deck()
    {
        Transmit = TransmitType.Always;
    }

    public override void Spawn()
    {
        GenerateDeck();
    }

    public override void ClientSpawn()
    {
        base.ClientSpawn();
        UpdateParticles();
    }

    [Event.Tick.Client]
    public void OnTickClient()
    {
        UpdateParticles();
    }

    [Event.Tick.Server]
    public void OnTickServer()
    {
        Count = Cards.Count;
    }

    private void UpdateParticles()
    {
        if(CardStackParticles == null)
        {
            CardStackParticles = Particles.Create("particles/cards/card_stack.vpcf");
            CardStackParticles.SetEntity(0, this, true);
            return;
        }

        CardStackParticles.SetPositionComponent(1, 0, Count);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if(IsClient && CardStackParticles != null)
            CardStackParticles.Destroy(true);
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
        foreach(var c in cards)
            Cards.Add(c);
    }

    /// <summary>
    /// Remove a card from the deck.
    /// </summary>
    /// <param name="card"></param>
    public void RemoveCard(Card card)
    {
        Cards.Remove(card);
    }

    /// <summary>
    /// Clear (AND DELETE) cards in deck.
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
        Card c = GetTopCard();
        Cards.Remove(c);
        return c;
    }

    /// <summary>
    /// Generate initial deck of cards to be used in play.
    /// </summary>
    private void GenerateDeck()
    {
        Cards = new List<Card>();

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
        for(int i = Cards.Count-1; i > 1; i--)
        {
            int rand = random.Next(i + 1);
            var temp = Cards[rand];
            Cards[rand] = Cards[i];
            Cards[i] = temp;
        }
    }
}
