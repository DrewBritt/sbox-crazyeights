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
        {
            Current.CurrentState.Cleanup();
            Current.CurrentState.SetState(new WaitingForPlayersState());
        }
    }

    [ConCmd.Admin("ce_forceturn", Help = "Force the current player's turn")]
    public static void ForceTurn()
    {
        if(Current.CurrentState is PlayingState)
            Current.CurrentPlayer.ForcePlayCard();
    }

    [ConCmd.Server("ce_swapteam", Help = "Swap client to opposite team")]
    public static void SwapTeam(int clientNetworkIdent)
    {
        var clientToSwap = Game.Clients.Where(c => c.NetworkIdent == clientNetworkIdent).FirstOrDefault();

        // Don't let non-admin swap anyone other than themselves.
        if(!ConsoleSystem.Caller.IsListenServerHost && ConsoleSystem.Caller != clientToSwap)
            return;

        // Spectator -> Player
        if(clientToSwap.Pawn is Spectator)
        {
            // If player makes a 2nd request while in the swap queue, let's cancel their swap.
            if(Current.SpectatorSwapQueue.Contains(clientToSwap))
            {
                Current.SpectatorSwapQueue.Remove(clientToSwap);
                return;
            }

            // If there's room, set client up with a new pawn.
            var chair = Entity.All.OfType<PlayerChair>().Where(c => !c.HasPlayer).FirstOrDefault();
            if(chair.IsValid())
            {

                var playerPawn = new Player(clientToSwap);
                chair.SeatPlayer(playerPawn);

                var spectatorPawn = clientToSwap.Pawn;
                clientToSwap.Pawn = playerPawn;
                spectatorPawn.Delete();
            }
            else
            {
                Current.SpectatorSwapQueue.Add(clientToSwap);
            }
        }
        else if(clientToSwap.Pawn is Player) // Player -> Spectator
        {
            Current.TransferOrCleanupPlayer(clientToSwap);
            clientToSwap.Pawn = new Spectator();
        }
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
        player.Animator.DidAction();
        player.Animator.PlayFacialPose(PlayerFacialPose.Positive);
        Sound.FromEntity("cardplace", Current.DiscardPileEntity);

        // Hide clientside SuitSelectionEntity if it's visible
        player.HideSuitSelection(To.Single(player.Client));

        Current.NotifyPlayerOfLastPlayed(To.Everyone, GameManager.Current.CurrentPlayer.Client);

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

        // Network animations
        player.Animator.DidAction();
        player.Animator.PlayFacialPose(PlayerFacialPose.Negative);

        // Hide clientside SuitSelectionEntity if it's visible
        player.HideSuitSelection(To.Single(player.Client));

        Current.NotifyPlayerOfLastPlayed(To.Everyone, GameManager.Current.CurrentPlayer.Client, true);

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
            var player = Current.Hands[nextIndex].Item1;
            NotifyPlayerOfSkip(To.Single(player.Client));
            player.Animator.PlayFacialPose(PlayerFacialPose.Negative);
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
    }

    /// <summary>
    /// Plays various effects on screen to notify player that they've been skipped (skip/wildcard).
    /// </summary>
    [ClientRpc]
    public void NotifyPlayerOfSkip()
    {
        Sound.FromScreen("playerskip");

        //Red vignette sting
        Player player = Game.LocalPawn as Player;
        player.Animator.PlayFacialPose(PlayerFacialPose.Negative);
        if(player.PlayerCamera is not null)
        {
            player.PlayerCamera.SetVignetteColor(new Color(1f, 0f, 0f, 1f));
            player.PlayerCamera.SetVignetteIntensity(.2f);
        }
    }

    [ClientRpc]
    public void NotifyPlayerOfLastPlayed(IClient player, bool drewCard = false)
    {
        Current.Hud.ActivateLastPlayed(player, drewCard);
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

    [ConCmd.Server("ce_emote", Help = "Perform an emote as a player")]
    public static void Emote(PlayerEmote emote)
    {
        if(ConsoleSystem.Caller.Pawn is not Player player)
            return;

        player.Animator.PlayEmote(emote);
    }
}
