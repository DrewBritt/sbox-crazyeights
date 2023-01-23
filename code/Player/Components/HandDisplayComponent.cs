using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace CrazyEights;

/// <summary>
/// Encapsulates a player's current hand of cards in textured CardEntities.
/// Cards are added by passing them to AddCard, which will stick them in a Queue.
/// On a timer, a CardEntity is spawned and added to the PlayerHand.
/// This lets us "animate" them spawning in quick succession rather than all at once.
/// </summary>
public partial class HandDisplayComponent : EntityComponent<Player>, ISingletonComponent
{
    /// <summary>
    /// Cards currently in the player's hand.
    /// </summary>
    [Net] public IList<CardEntity> Cards { get; set; }

    /// <summary>
    /// Adds paramater card to a Queue to be spawned as a CardEntity.
    /// </summary>
    /// <param name="card">Card to be spawned as a CardEntity</param>
    public void AddCard(Card card) => cardsToAdd.Enqueue(card);

    /// <summary>
    /// Adds cards to a Queue to be spawned as CardEntities.
    /// </summary>
    /// <param name="cards"></param>
    public void AddCards(IList<Card> cards)
    {
        cards.OrderBy(c => c.Suit).ThenBy(c => c.Rank);
        foreach(var c in cards)
            AddCard(c);
    }

    /// <summary>
    /// Spawns a CardEntity representation of param card.
    /// </summary>
    /// <param name="card">Card to be spawned as a CardEntity</param>
    public void SpawnCard(Card card)
    {
        CardEntity cardEnt = new CardEntity();

        // Net cardEnt's existence to client, then set on server
        Cards.Add(cardEnt);
        cardEnt.Card = card;

        // TODO: Revert this whenever whatever the fuck causes
        // textures to fail creation on remote clients is fixed
        //cardEnt.SetCard(To.Single(Owner.Client), Rank, Suit);
        cardEnt.Owner = Entity;

        // Position card ent in hand
        cardEnt.SetParent(Entity, "handStartPos");
        var attachment = Entity.GetAttachment("handStartPos").GetValueOrDefault();
        cardEnt.LocalRotation = Rotation.FromPitch(90).RotateAroundAxis(Vector3.Forward, -60f) * Rotation.FromRoll(180);
        cardEnt.LocalPosition = (Vector3.Forward * (Cards.Count)) + (Vector3.Right * (.65f * (Cards.Count))) + (Vector3.Up * 1.25f);

        Sound.FromEntity("cardaddedtohand", cardEnt);

        Cards = Cards.OrderBy(c => c.Suit).ThenBy(c => c.Rank).ToList();
        UpdateCardPositions();
    }

    /// <summary>
    /// Cards to be added to player's hand, on a timer checked in OnTickServer (so they animate in nicely).
    /// </summary>
    private Queue<Card> cardsToAdd = new Queue<Card>();

    TimeSince cardSpawned = 0;
    /// <summary>
    /// Called every tick to check if any cards must be spawned as CardEntities.
    /// Waits cardSpawned seconds before spawning a CardEntity.
    /// </summary>
    [Event.Tick.Server]
    public void OnTickServer()
    {
        if(!cardsToAdd.Any()) return;

        if(cardSpawned < .2) return;

        Card cardToAdd = cardsToAdd.Dequeue();
        SpawnCard(cardToAdd);
        cardSpawned = 0;
    }

    /// <summary>
    /// Despawn and remove a CardEntity from this Hand.
    /// </summary>
    /// <param name="card"></param>
    public void RemoveCard(CardEntity card)
    {
        if(card.IsValid())
        {
            Cards.Remove(card);
            card.Delete();
        }

        UpdateCardPositions();
    }

    /// <summary>
    /// Try to find and despawn/remove a corresponding CardEntity from input Card.
    /// </summary>
    /// <param name="card"></param>
    public void RemoveCard(Card card)
    {
        // Find and blindly call RemoveCard, as the IsValid check will determine if it's null.
        var cardEnt = Cards.Where(c => c.Rank == card.Rank && c.Suit == card.Suit).FirstOrDefault();
        RemoveCard(cardEnt);
    }

    /// <summary>
    /// Despawn and remove all CardEntity's from this Hand.
    /// </summary>
    public void ClearCards()
    {
        foreach(var c in Cards)
            c.Delete();
        Cards.Clear();
    }

    private void UpdateCardPositions()
    {
        for(int i = 0; i < Cards.Count; i++)
        {
            var card = Cards[i];
            card.LocalPosition = (Vector3.Forward * (i + 1)) + (Vector3.Right * (.65f * (i + 1))) + (Vector3.Up * 1.25f);
        }
    }

    protected override void OnDeactivate()
    {
        // Cleanup CardEntity's when we remove the player's hand.
        if(Game.IsClient) return;

        ClearCards();
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
