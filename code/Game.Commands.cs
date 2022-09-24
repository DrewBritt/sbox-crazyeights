using Sandbox;
using System.Linq;

namespace CrazyEights;

public partial class Game
{
    /// <summary>
    /// Generic handler for invalid command arguments.
    /// </summary>
    /// <param name="errormessage"></param>
    [ClientRpc]
    public void CommandError(string errormessage)
    {
        Log.Error(errormessage);
    }

    #region Card Playing

    /// <summary>
    /// Player wishes to play a card.
    /// </summary>
    /// <param name="cardIdent">NetworkIdent of the Card to be played.</param>
    /// <param name="selectedWildSuit">Selected Wildcard color/suit. Only checked if cardIdent's card is a Wild or Draw4.</param>
    /// <param name="isBot">Is the caller a bot? (Bots can't have a ConsoleSystem.Caller)</param>
    [ConCmd.Server("ce_playcard", Help = "Play a card from your hand")]
    public static void PlayCard(int cardIdent, CardSuit selectedWildSuit = 0, bool isBot = false)
    {
        // As bots can't call a command and have a ConsoleSystem.Caller,
        // we have to manually track if a bots calling this command.
        // Assume the bot is the current player (this gets verified)
        Pawn player;
        if(!isBot)
            player = ConsoleSystem.Caller.Pawn as Pawn;
        else
            player = Current.CurrentPlayer;

        // Stop player if not in playing state.
        if(Current.CurrentState is not PlayingState)
        {
            Current.CommandError(To.Single(ConsoleSystem.Caller), "Crazy Eights: You're not currently playing!");
            return;
        }

        // Stop player if they're not the current player.
        if(player != Current.CurrentPlayer)
        {
            Current.CommandError(To.Single(ConsoleSystem.Caller), "Crazy Eights: You are not the current player!");
            return;
        }

        // Stop player if the card they want to play isn't in their hand (or it isn't valid).
        CardEntity cardEnt = player.Hand.Cards.Where(c => c.NetworkIdent == cardIdent).FirstOrDefault();
        if(!cardEnt.IsValid())
        {
            Current.CommandError(To.Single(ConsoleSystem.Caller), "Crazy Eights: That card isn't in your hand (or doesn't exist...)");
            return;
        }

        // Stop player if the card is not a valid play (wrong suit and rank).
        Card topCard = Current.DiscardPile.GetTopCard();
        if(cardEnt.Suit != CardSuit.Wild)
            if(cardEnt.Suit != topCard.Suit && cardEnt.Rank != topCard.Rank)
            {
                Current.CommandError(To.Single(ConsoleSystem.Caller), "Crazy Eights: That card is not a legal play! (Wrong suit and rank)");
                return;
            }

        // Selected suit cannot be wild and therefore is an illegal play.
        if(selectedWildSuit == CardSuit.Wild)
        {
            Current.CommandError(To.Single(ConsoleSystem.Caller), "Crazy Eights: You cannot select Wild as a color! (Illegal play)");
            return;
        }

        // Remove card from player's hand, and play it onto DiscardPile.
        player.Hand.RemoveCard(cardEnt);
        Current.DiscardPile.AddCard(cardEnt.Card);

        // Update everyone's play pile
        Current.DiscardPile.TopCardEntity.SetCard(To.Everyone, cardEnt.Card.Rank, cardEnt.Card.Suit);

        // Play Interact animation.
        Current.CurrentPlayer.DoInteractAnimation(To.Everyone);

        // Check if player has won.
        if(player.Hand.Cards.Count == 0)
        {
            Current.CurrentState = new GameOverState();
            return;
        }

        // Action/Wildcard abilities.
        Current.CheckActionCard(cardEnt.Card, selectedWildSuit);

        // Next player's turn.
        Current.CurrentPlayerIndex = Current.GetNextPlayerIndex();
        (Current.CurrentState as PlayingState).TurnStarted = 0;
    }

    /// <summary>
    /// Player wishes to draw a card from the persistent deck. This will end their turn.
    /// </summary>
    [ConCmd.Server("ce_drawcard", Help = "Draw a card from the playing deck")]
    public static void DrawCard(bool isBot = false)
    {
        // As bots can't call a command and have a ConsoleSystem.Caller,
        // we have to manually track if a bots calling this command.
        // Assume the bot is the current player (this gets verified)
        Pawn player;
        if(!isBot)
            player = ConsoleSystem.Caller.Pawn as Pawn;
        else
            player = Current.CurrentPlayer;

        // Stop player if not in playing state.
        if(Current.CurrentState is not PlayingState)
        {
            Current.CommandError(To.Single(ConsoleSystem.Caller), "Crazy Eights: You're not currently playing!");
            return;
        }

        // Stop player if they're not the current player.
        if(player != Current.CurrentPlayer)
        {
            Current.CommandError(To.Single(ConsoleSystem.Caller), "Crazy Eights: You are not the current player!");
            return;
        }

        // Check if PlayingDeck is empty, and if so, swap it with DiscardPile (except for last played card)
        var deck = Current.PlayingDeck;
        var discard = Current.DiscardPile;
        if(!deck.Cards.Any())
        {
            // Add discard (except for top card) to playing deck
            var discardTop = discard.GrabTopCard();
            deck.AddCards(discard.Cards);
            deck.Shuffle();

            // Clear played Wild cards back to Wild (instead of their played color)
            var wilds = deck.Cards.Where(c => c.Rank > (CardRank)12).ToList();
            foreach(var c in wilds)
                c.Suit = CardSuit.Wild;

            // Clear discard and add old top card back
            discard.Cards.Clear();
            discard.AddCard(discardTop);
        }

        // Add 1 card from top of pile to player hand.
        player.Hand.AddCard(Current.PlayingDeck.GrabTopCard());

        // Play Interact animation.
        Current.CurrentPlayer.DoInteractAnimation(To.Everyone);

        // Next player's turn.
        Current.CurrentPlayerIndex = Current.GetNextPlayerIndex();
        (Current.CurrentState as PlayingState).TurnStarted = 0;
    }
    #endregion
}
