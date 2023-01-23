using System.Linq;
using Sandbox;

namespace CrazyEights;

public partial class GameManager
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

    #region Game Management

    [ConCmd.Admin("ce_resetgame", Help = "Reset the game to its initial state")]
    public static void ResetGame()
    {
        if(Current.CurrentState is PlayingState)
            Current.CurrentState = new WaitingForPlayersState();
    }

    [ConCmd.Admin("ce_forceturn", Help = "Force the current player's turn")]
    public static void ForceTurn()
    {
        if(Current.CurrentState is PlayingState)
            Current.CurrentPlayer.ForcePlayCard();
    }

    #endregion

    #region Card Playing

    /// <summary>
    /// Player wishes to play a card.
    /// </summary>
    /// <param name="cardIdent">NetworkIdent of the Card to be played.</param>
    /// <param name="selectedWildSuit">Selected Wildcard color/suit. Only checked if cardIdent's card is a Wild or Draw4.</param>
    [ConCmd.Server("ce_playcard", Help = "Play a card from your hand")]
    public static void PlayCard(int cardIdent, CardSuit selectedWildSuit = 0)
    {
        // Calling a ConCmd from console normally passes the Caller, client, unless its ran from the server,
        // in which cases it passes the host's client. Instead, we call the static method directly from server
        // scenarios (bots/AFK behaviour) which avoids setting ConsoleSystem.Caller
        Player player;
        if(!ConsoleSystem.Caller.IsValid())
            player = Current.CurrentPlayer;
        else
            player = ConsoleSystem.Caller.Pawn as Player;

        // Stop caller if not in playing state.
        if(Current.CurrentState is not PlayingState)
        {
            Current.CommandError(To.Single(ConsoleSystem.Caller), "Crazy Eights: You're not currently playing!");
            return;
        }

        // Stop caller if they're not the current player.
        if(player != Current.CurrentPlayer)
        {
            Current.CommandError(To.Single(ConsoleSystem.Caller), "Crazy Eights: You are not the current player!");
            return;
        }

        // Stop caller if the card they want to play isn't in their hand (or it isn't valid).
        CardEntity cardEnt = player.HandDisplay.Cards.Where(c => c.NetworkIdent == cardIdent).FirstOrDefault();
        if(!cardEnt.IsValid())
        {
            Current.CommandError(To.Single(ConsoleSystem.Caller), "Crazy Eights: That card isn't in your hand (or doesn't exist...)");
            return;
        }

        // Stop caller if the card is not a valid play (wrong suit and rank).
        Card topCard = Current.DiscardPile.GetTopCard();
        if(cardEnt.Suit != CardSuit.Wild)
        {
            if(cardEnt.Suit != topCard.Suit && cardEnt.Rank != topCard.Rank)
            {
                Current.CommandError(To.Single(ConsoleSystem.Caller), "Crazy Eights: That card is not a legal play! (Wrong suit and rank)");
                return;
            }
        }

        // Selected suit cannot be wild and therefore is an illegal play.
        if(selectedWildSuit == CardSuit.Wild)
        {
            Current.CommandError(To.Single(ConsoleSystem.Caller), "Crazy Eights: You cannot select Wild as a color! (Illegal play)");
            return;
        }

        // Action/Wildcard abilities.
        Current.CheckActionCard(cardEnt.Card, selectedWildSuit);

        // Move card from Hand to DiscardPile
        player.Hand().RemoveCard(cardEnt.Card);
        Current.DiscardPile.AddCard(cardEnt.Card);

        // Play animation and card sound
        player.DoInteractAnimation(To.Everyone);
        Sound.FromEntity("cardplace", Current.DiscardPileEntity);

        // Hide clientside SuitSelectionEntity if it's visible
        player.HideSuitSelection(To.Single(player.Client));

        // Game Over if player has no cards
        Log.Info(player.Hand());
        if(player.Hand().Count == 0)
        {
            Current.CurrentState.SetState(new GameOverState());
            return;
        }

        // Next player's turn.
        Current.AdvancePlayingState();
    }

    /// <summary>
    /// Player wishes to draw a card from the persistent deck. This will end their turn.
    /// </summary>
    [ConCmd.Server("ce_drawcard", Help = "Draw a card from the playing deck")]
    public static void DrawCard()
    {
        // Calling a ConCmd from console normally passes the Caller, client, unless its ran from the server,
        // in which cases it passes the host's client. Instead, we call the static method directly from server
        // scenarios (bots/AFK behaviour) which avoids setting ConsoleSystem.Caller
        Player player;
        if(!ConsoleSystem.Caller.IsValid())
            player = Current.CurrentPlayer;
        else
            player = ConsoleSystem.Caller.Pawn as Player;

        // Stop caller if not in playing state.
        if(Current.CurrentState is not PlayingState)
        {
            Current.CommandError(To.Single(ConsoleSystem.Caller), "Crazy Eights: You're not currently playing!");
            return;
        }

        // Stop caller if they're not the current player.
        if(player != Current.CurrentPlayer)
        {
            Current.CommandError(To.Single(ConsoleSystem.Caller), "Crazy Eights: You are not the current player!");
            return;
        }

        // Add 1 card from top of pile to player hand.
        Card card = Current.PlayingDeck.GrabTopCard();
        player.Hand().AddCard(card);

        // Play player interact animation
        player.DoInteractAnimation(To.Everyone);

        // Hide clientside SuitSelectionEntity if it's visible
        player.HideSuitSelection(To.Single(player.Client));

        // Next player's turn.
        Current.AdvancePlayingState();
    }

    /// <summary>
    /// Advances to next PlayingState/next player's turn.
    /// </summary>
    private void AdvancePlayingState()
    {
        // Increment to next player regardless
        int nextIndex = Current.GetNextPlayerIndex(Current.CurrentPlayerIndex);

        // If we should skip, notify our new "current player" and get the next-next player, then reset flag
        if(ShouldSkip == 1)
        {
            NotifyPlayerOfSkip(To.Single(Current.Hands[nextIndex].Item1.Client));
            nextIndex = Current.GetNextPlayerIndex(nextIndex);
            ShouldSkip = 0;
        }

        // Notify actual new player
        Current.CurrentState.SetState(new PlayingState(Current.PlayingDeck, Current.DiscardPile, Current.Hands, nextIndex));
    }
    #endregion

    #region Notify Players

    /// <summary>
    /// Plays various effects on screen to notify player that it is their turn.
    /// </summary>
    [ClientRpc]
    public void NotifyPlayerOfTurn()
    {
        // Turn timer tells player when their turn is nearly up
        Current.Hud.ActivateTurnTimer();
        Sound.FromScreen("playerturn");

        // Green vignette sting
        Player localPawn = Game.LocalPawn as Player;
        if(localPawn.PlayerCamera is not null)
        {
            localPawn.PlayerCamera.SetVignetteColor(new Color(0f, 1f, 0f, 1f));
            localPawn.PlayerCamera.SetVignetteIntensity(.25f);
        }
    }

    /// <summary>
    /// Plays various effects on screen to notify player that they've been skipped (skip/wildcard).
    /// </summary>
    [ClientRpc]
    public void NotifyPlayerOfSkip()
    {
        Sound.FromScreen("playerskip");

        //Red vignette sting
        Player localPawn = Game.LocalPawn as Player;
        if(localPawn.PlayerCamera is not null)
        {
            localPawn.PlayerCamera.SetVignetteColor(new Color(1f, 0f, 0f, 1f));
            localPawn.PlayerCamera.SetVignetteIntensity(.25f);
        }
    }

    /// <summary>
    /// Displays winner of game to all players.
    /// </summary>
    [ClientRpc]
    public void NotifyPlayerOfGameOver()
    {
        Sound.FromScreen("gameover");
        Current.Hud.ActivateGameOverOverlay(Current.CurrentPlayer.Client);
    }
    #endregion
}
