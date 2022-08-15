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
    [ConCmd.Server]
    public static void PlayCard(int cardIdent)
    {
        Pawn player = ConsoleSystem.Caller.Pawn as Pawn;

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

        // Next player's turn (modulo to wrap)
        Current.CurrentPlayerIndex += 1 % Current.Players.Count;
    }

    #endregion
}
