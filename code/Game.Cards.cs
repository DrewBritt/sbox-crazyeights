using System.Collections.Generic;
using Sandbox;

namespace CrazyEights;

public partial class Game
{
    /// <summary>
    /// Current play direction. Default is clockwise/true.
    /// </summary>
    private bool DirectionIsClockwise = true;

    /// <summary>
    /// Value to be added to CurrentPlayerIndex, returns 1 if DirectionIsClockwise and -1 if !DirectionIsClockwise.
    /// </summary>
    private int DirectionValue => DirectionIsClockwise ? 1 : -1;

    /// <summary>
    /// Check if card is a action/wild card, and if so, run appropriate functions.
    /// </summary>
    /// <param name="card">Card to be checked.</param>
    /// <param name="selectedWildSuit">New suit to be used if played card is a wild card.</param>
    private void CheckActionCard(Card card, CardSuit selectedWildSuit)
    {
        switch(card.Rank)
        {
            case CardRank.Draw2: Draw2Action(); break;
            case CardRank.Reverse: ReverseAction(); break;
            case CardRank.Skip: SkipAction(); break;
            case CardRank.ChangeColor: ChangeColorAction(card, selectedWildSuit); break;
            case CardRank.Draw4: Draw4Action(card, selectedWildSuit); break;
        }
    }

    /// <summary>
    /// Next player will draw 2 cards and be skipped.
    /// </summary>
    private void Draw2Action()
    {
        Pawn nextPlayer = Players[GetNextPlayerIndex()];

        // Grab 2 cards from persistent deck to give to next player.
        List<Card> cards = new List<Card>();
        for(int i = 0; i < 2; i++)
            cards.Add(PlayingDeck.GrabTopCard());
        nextPlayer.Hand.AddCards(cards);

        // Skip next player as well
        SkipAction();
    }

    /// <summary>
    /// Play direction will be reversed and the player in the opposite direction plays next.
    /// </summary>
    private void ReverseAction()
    {
        // Toggle direction.
        DirectionIsClockwise = !DirectionIsClockwise;
    }

    /// <summary>
    /// Next player will be skipped.
    /// </summary>
    private void SkipAction()
    {
        // Currently just change CurrentPlayerIndex an extra time.
        CurrentPlayerIndex = GetNextPlayerIndex();
    }

    /// <summary>
    /// Card suit, and therefore play color, will be switched to selectedWildSuit.
    /// </summary>
    /// <param name="selectedWildSuit">New color to be used.</param>
    /// <param name="card">Played card, suit is set to set play color.</param>
    private void ChangeColorAction(Card card, CardSuit selectedWildSuit)
    {
        // Set play color by changing suit on card (play checks top card's suit, plus we can make cool colored Wild and Draw4 textures).
        card.Suit = selectedWildSuit;
    }

    /// <summary>
    /// Next player will draw 4 cards, be skipped, and the play color will be switched to selectedWildSuit.
    /// </summary>
    /// <param name="selectedWildSuit">New color to be used.</param>
    /// <param name="card">Played card, suit is set to set play color.</param>
    private void Draw4Action(Card card, CardSuit selectedWildSuit)
    {
        Pawn nextPlayer = Players[GetNextPlayerIndex()];

        // Grab 4 cards from persistent deck to give to next player.
        List<Card> cards = new List<Card>();
        for(int i = 0; i < 4; i++)
            cards.Add(PlayingDeck.GrabTopCard());
        nextPlayer.Hand.AddCards(cards);

        // Set play color.
        ChangeColorAction(card, selectedWildSuit);

        // Skip next player as well.
        SkipAction();
    }
}
