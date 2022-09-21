using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace CrazyEights;

/// <summary>
/// A deck/list of cards with deck management functionality to be used for a variety of purposes.
/// </summary>
public partial class Deck : ModelEntity
{
    /// <summary>
    /// All cards in this deck.
    /// </summary>
    public IList<Card> Cards { get; set; }
    public Particles CardStackParticles { get; set; }
    [Net, Change] public int Count { get; set; }

    public Deck()
    {
        Transmit = TransmitType.Always;
    }

    public override void Spawn()
    {
        GenerateDeck();

        // Position this entity relative to the GameTable entity placed on the map.
        var spawn = Entity.All.OfType<DeckSpawn>().Where(d => d.SpawnTarget == SpawnTarget.Deck).FirstOrDefault();
        if(spawn.IsValid())
            Transform = spawn.Transform;

        Tags.Add("deck");
        EnableAllCollisions = true;
        SetupPhysicsFromAABB(PhysicsMotionType.Keyframed, new Vector3(-5, -4, 0), new Vector3(5, 4, 5));
    }

    public override void ClientSpawn()
    {
        // Create particle system on Deck creation.
        base.ClientSpawn();
        CardStackParticles = Particles.Create("particles/cards/card_stack.vpcf");
    }

    [Event.Tick.Client]
    public void OnTickClient()
    {
        // Update particle system every tick.
        UpdateParticles();
    }

    [Event.Tick.Server]
    public virtual void OnTickServer()
    {
        // Update Count every tick.
        Count = Cards.Count;
    }

    /// <summary>
    /// Called on Client when Count variable has changed. Used to update CardStackParticles to display current Count size.
    /// As card_stack.vpcf uses an Instant Emitter, the particle system has to be recreated to repopulate it with a new Count.
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    public void OnCountChanged(int oldValue, int newValue)
    {
        if(oldValue == newValue) return;

        if(CardStackParticles != null)
        {
            CardStackParticles.Destroy(true);
            CardStackParticles = Particles.Create("particles/cards/card_stack.vpcf");
        }
    }

    /// <summary>
    /// Update Control Points of the CardStackParticles.
    /// </summary>
    protected virtual void UpdateParticles()
    {
        CardStackParticles.SetPosition(0, Position);
        CardStackParticles.SetPositionComponent(1, 0, Count);
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

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if(IsClient && CardStackParticles != null)
            CardStackParticles.Destroy(true);
    }
}
