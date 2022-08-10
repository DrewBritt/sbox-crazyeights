using System.Collections.Generic;
using Sandbox;

namespace CrazyEights;

public partial class Deck : Entity
{
    private IList<Card> cards;

    public Deck()
    {
        Transmit = TransmitType.Always;
    }

    public override void Spawn()
    {
        base.Spawn();

        GenerateDeck();
    }

    /// <summary>
    /// Generate initial deck of cards to be used in play
    /// </summary>
    private void GenerateDeck()
    {
        cards = new List<Card>();

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
                    cards.Add(card);
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
                cards.Add(wildCard);
            }
    }
}
