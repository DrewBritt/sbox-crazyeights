using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace CrazyEights;

/// <summary>
/// Encapsulates a player's current hand of cards.
/// </summary>
public partial class PlayerHand : BaseNetworkable
{
    /// <summary>
    /// Pawn in which this Hand belongs to.
    /// </summary>
    [Net] public Pawn Owner { get; set; }
    /// <summary>
    /// CardEntity's (and therefore Cards) in this hand.
    /// </summary>
    [Net] public IList<CardEntity> Cards { get; set; }

    /// <summary>
    /// Spawn a CardEntity into the world, set its value on the appropriate client, and add it to this Hand.
    /// </summary>
    /// <param name="card"></param>
    public void AddCard(Card card)
    {
        CardEntity cardEnt = new CardEntity();

        // Net cardEnt's existence to client, then set on server
        Cards.Add(cardEnt);
        cardEnt.Card = card;
        
        // Then net card texture
        cardEnt.SetCard(To.Single(Owner.Client), card.Rank, card.Suit);

        // Position card ent in hand
        cardEnt.SetParent(Owner, "handStartPos");
        var attachment = Owner.GetAttachment("handStartPos").GetValueOrDefault();
        cardEnt.LocalPosition = (-.5f + Vector3.Forward * Cards.Count) + (Vector3.Right * Cards.Count * .25f) + (Vector3.Up * 1.5f);
        cardEnt.LocalRotation = Rotation.FromPitch(86).RotateAroundAxis(Vector3.Forward, -80f);

        UpdateCardPositions();
    }

    /// <summary>
    /// Spawn and add a list of CardEntity's and add it to this Hand.
    /// </summary>
    /// <param name="cards"></param>
    public void AddCards(IList<Card> cards)
    {
        foreach(var c in cards)
            AddCard(c);

        UpdateCardPositions();
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
            card.LocalPosition = (-.75f + Vector3.Forward * (i + 1)) + (Vector3.Right * (.25f * (i+1))) + (Vector3.Up * 3f);
        }
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
