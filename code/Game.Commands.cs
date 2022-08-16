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
    [ConCmd.Server("crazyeights_playcard", Help = "Play a card from your hand")]
    public static void PlayCard(int cardIdent)
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

        // Remove card from player's hand, and play it onto PlayingPile
        player.Hand.RemoveCard(card);
        Current.PlayingPile.AddCard(card);

        Current.PrintPlay(To.Everyone);

        // TODO: Action/Wildcard abilities

        // Next player's turn (modulo to wrap)
        Current.CurrentPlayerIndex = (Current.CurrentPlayerIndex + 1) % Current.Players.Count;

        Current.PrintCards(To.Everyone);
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
