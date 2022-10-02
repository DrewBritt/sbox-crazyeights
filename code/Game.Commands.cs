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
        Pawn player;
        if(!ConsoleSystem.Caller.IsValid())
            player = Current.CurrentPlayer;
        else
            player = ConsoleSystem.Caller.Pawn as Pawn;

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
            Log.Info($"{player}, {Current.CurrentPlayer}, {Current.Players}");
            return;
        }

        // Stop caller if the card they want to play isn't in their hand (or it isn't valid).
        CardEntity cardEnt = player.Hand.Cards.Where(c => c.NetworkIdent == cardIdent).FirstOrDefault();
        if(!cardEnt.IsValid())
        {
            Current.CommandError(To.Single(ConsoleSystem.Caller), "Crazy Eights: That card isn't in your hand (or doesn't exist...)");
            return;
        }

        // Stop caller if the card is not a valid play (wrong suit and rank).
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

        // Action/Wildcard abilities.
        Current.CheckActionCard(cardEnt.Card, selectedWildSuit);

        // Move card from Hand to DiscardPile, and network texture
        player.Hand.RemoveCard(cardEnt);
        Current.DiscardPile.AddCard(cardEnt.Card);
        Current.DiscardPile.TopCardEntity.SetCard(To.Everyone, cardEnt.Card.Rank, cardEnt.Card.Suit);

        // Play animation and card sound
        Current.CurrentPlayer.DoInteractAnimation(To.Everyone);
        Sound.FromEntity("cardplace", Current.DiscardPile);

        // Game Over if player has no cards
        if(player.Hand.Cards.Count == 0)
        {
            Current.CurrentState = new GameOverState();
            return;
        }

        // Next player's turn.
        Current.SetNewCurrentPlayer();
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
        Pawn player;
        if(!ConsoleSystem.Caller.IsValid())
            player = Current.CurrentPlayer;
        else
            player = ConsoleSystem.Caller.Pawn as Pawn;

        // Stop caller if they're not the current player.
        if(player != Current.CurrentPlayer)
        {
            Current.CommandError(To.Single(ConsoleSystem.Caller), "Crazy Eights: You are not the current player!");
            return;
        }

        // Add 1 card from top of pile to player hand.
        player.Hand.AddCard(Current.PlayingDeck.GrabTopCard());

        // Play player interact animation
        Current.CurrentPlayer.DoInteractAnimation(To.Everyone);

        // Next player's turn.
        Current.SetNewCurrentPlayer();
    }

    /// <summary>
    /// Updates Game.CurrentPlayerIndex
    /// </summary>
    private void SetNewCurrentPlayer()
    {
        // Increment to next player regardless
        Current.CurrentPlayerIndex = Current.GetNextPlayerIndex();
        
        // If we should skip, notify our new "current player" and get the next-next player, then reset flag
        if(ShouldSkip == 1)
        {
            NotifyPlayerOfSkip(To.Single(CurrentPlayer.Client));
            Current.CurrentPlayerIndex = Current.GetNextPlayerIndex();
            ShouldSkip = 0;
        }

        // Notify actual new player
        (Current.CurrentState as PlayingState).TurnStarted = 0;
        NotifyPlayerOfTurn(To.Single(CurrentPlayer.Client));
    }
    #endregion

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
        if(Current.FindActiveCamera() is Camera)
        {
            var camera = Current.FindActiveCamera() as Camera;
            camera.SetVignetteColor(new Color(0f, 1f, 0f, 1f));
            camera.SetVignetteIntensity(.25f);
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
        if(Current.FindActiveCamera() is Camera)
        {
            var camera = Current.FindActiveCamera() as Camera;
            camera.SetVignetteColor(new Color(1f, 0f, 0f, 1f));
            camera.SetVignetteIntensity(.25f);
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
}
