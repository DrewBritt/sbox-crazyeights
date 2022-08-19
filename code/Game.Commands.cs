using Sandbox;
using System.Linq;

namespace CrazyEights;

public partial class Game
{
    /// <summary>
    /// Generic handler for invalid command arguments
    /// </summary>
    /// <param name="errormessage"></param>
    [ClientRpc]
    public void CommandError(string errormessage)
    {
        Log.Error(errormessage);
    }

    #region Card Playing

    /// <summary>
    /// Player wishes to play a card
    /// </summary>
    /// <param name="cardIdent">NetworkIdent of the Card to be played</param>
    /// <param name="selectedWildSuit">Selected Wildcard color/suit. Only checked if cardIdent's card is a Wild or Draw4</param>
    [ConCmd.Server("crazyeights_playcard", Help = "Play a card from your hand")]
    public static void PlayCard(int cardIdent, CardSuit selectedWildSuit = 0)
    {
        Pawn player = ConsoleSystem.Caller.Pawn as Pawn;

        // Stop player if not in playing state
        if(Current.CurrentState is not PlayingState)
        {
            Log.Info(Current.CurrentState);
            Current.CommandError(To.Single(ConsoleSystem.Caller), "Crazy Eights: You're not currently playing!");
            return;
        }

        // Stop player if they're not the current player
        if(player != Current.CurrentPlayer)
        {
            Current.CommandError(To.Single(ConsoleSystem.Caller), "Crazy Eights: You are not the current player!");
            return;
        }

        // Stop player if the card they want to play isn't in their hand (or it isn't valid)
        Card card = player.Hand.Cards.Where(c => c.NetworkIdent == cardIdent).FirstOrDefault();
        if(!card.IsValid())
        {
            Current.CommandError(To.Single(ConsoleSystem.Caller), "Crazy Eights: That card isn't in your hand (or doesn't exist...)");
            return;
        }

        // Stop player if the card is not a valid play (wrong suit and rank)
        Card topCard = Current.PlayingPile.GetTopCard();
        if(card.Suit != CardSuit.Wild)
            if(card.Suit != topCard.Suit && card.Rank != topCard.Rank)
            {
                Current.CommandError(To.Single(ConsoleSystem.Caller), "Crazy Eights: That card is not a legal play! (Wrong suit and rank)");
                return;
            }

        // Selected suit cannot be wild and therefore is an illegal play
        if(selectedWildSuit == CardSuit.Wild)
        {
            Current.CommandError(To.Single(ConsoleSystem.Caller), "Crazy Eights: You cannot select Wild as a color! (Illegal play)");
            return;
        }

        // Remove card from player's hand, and play it onto PlayingPile
        player.Hand.RemoveCard(card);
        Current.PlayingPile.AddCard(card);

        Current.PrintPlay(To.Everyone);

        // Action/Wildcard abilities
        Current.CheckActionCard(card, selectedWildSuit);

        // Next player's turn
        Current.CurrentPlayerIndex = Current.GetNextPlayerIndex();

        Current.PrintCards(To.Everyone);
    }

    /// <summary>
    /// Player wishes to draw a card from the persistent deck. This will end their turn.
    /// </summary>
    [ConCmd.Server("crazyeights_drawcard", Help = "Draw a card from the playing deck")]
    public static void DrawCard()
    {
        // Add 1 card from top of pile to player hand, and end their turn
        Pawn player = ConsoleSystem.Caller.Pawn as Pawn;
        player.Hand.AddCard(Current.PlayingDeck.GrabTopCard());

        Current.PrintDraw(To.Everyone);

        Current.CurrentPlayerIndex = Current.GetNextPlayerIndex();

        Current.PrintCards(To.Everyone);
    }

    [ClientRpc]
    public void PrintDraw()
    {
        Log.Info($"{Current.CurrentPlayer} drew a card and ended their turn");
    }

    [ClientRpc]
    public void PrintPlay()
    {
        var lastCard = Current.PlayingPile.GetTopCard();
        Log.Info($"{Current.CurrentPlayer} played {lastCard.Suit} {lastCard.Rank}");
    }

    [ClientRpc]
    public void PrintCards()
    {
        var p = Local.Pawn as Pawn;
        foreach(var c in p.Hand.Cards)
            Log.Info($"{c.Suit} {c.Rank} ({c.NetworkIdent})");
    }

    #endregion
}
