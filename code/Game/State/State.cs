﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using static CrazyEights.GameManager;

namespace CrazyEights;

public class BaseState
{
    public virtual string StateName() => GetType().ToString();

    public BaseState()
    {
        Initialize();
    }

    protected virtual void Initialize()
    {

    }

    public virtual void Tick()
    {

    }

    public void SetState(BaseState state)
    {
        Current.CurrentState = state;
    }
}

/// <summary>
/// Waiting for more players to connect before beginning game.
/// </summary>
public class WaitingForPlayersState : BaseState
{
    public override string StateName() => "Waiting for Players";

    public override void Tick()
    {
        if(Game.Clients.Count > 1)
            SetState(new StagingState());
    }
}

/// <summary>
/// Brief intermission before beginning game.
/// </summary>
public class StagingState : BaseState
{
    public override string StateName() => "Staging";

    TimeUntil startGame = 10;
    public override void Tick()
    {
        // Don't start a game if we're by ourselves.
        if(Game.Clients.Count == 1)
            SetState(new WaitingForPlayersState());

        // Give players time to join and load in.
        if(startGame <= 0)
            SetState(new PlayingState());
    }
}

/// <summary>
/// Players are currently playing cards.
/// </summary>
public class PlayingState : BaseState
{
    public Deck PlayingDeck { get; private set; }
    public Pile DiscardPile { get; private set; }
    public List<Tuple<Player, Hand>> Hands { get; private set; }
    public int CurrentPlayerIndex { get; private set; } = 0;
    private Player CurrentPlayer => Hands[CurrentPlayerIndex].Item1;

    /// <summary>
    /// If we pass in game data on state creation, we know we're continuing a game. If it's all null, initialize a new game.
    /// </summary>
    /// <param name="playingDeck"></param>
    /// <param name="discardPile"></param>
    /// <param name="hands"></param>
    /// <param name="playerIndex"></param>
    public PlayingState(Deck playingDeck = null, Pile discardPile = null, List<Tuple<Player, Hand>> hands = null, int playerIndex = 0)
    {
        PlayingDeck = playingDeck;
        DiscardPile = discardPile;
        Hands = hands;
        CurrentPlayerIndex = playerIndex;

        Initialize();

        if(IsWinner())
        {
            SetState(new GameOverState());
            return;
        }

        GameManager.Current.CurrentPlayer = CurrentPlayer;
        GameManager.Current.NotifyPlayerOfTurn(To.Single(CurrentPlayer.Client));
    }

    private void Initialize()
    {
        if(PlayingDeck != null) return;

        // Generate deck to be used in play.
        PlayingDeck = new Deck();
        PlayingDeck.Shuffle();

        // Distribute cards to players (7 each by default)
        Hands = new List<Tuple<Player, Hand>>();
        List<IClient> playerClients = Game.Clients.Where(p => p.Pawn is Player).ToList();
        foreach(IClient playerClient in playerClients)
        {
            var player = playerClient.Pawn as Player;

            // Populate hand with cards from deck.
            Hand hand = new Hand(player);
            for(int j = 0; j < 7; j++)
                hand.AddCard(PlayingDeck.GrabTopCard());
            Hands.Add(new Tuple<Player, Hand>(player, hand));
        }

        // Game plays starting card from top of deck onto discard pile.
        // Shuffles until first card is a valid starting card (not wild)
        DiscardPile = new Pile();
        while(PlayingDeck.GetTopCard().Suit == CardSuit.Wild)
            PlayingDeck.Shuffle();
        var firstCard = PlayingDeck.GrabTopCard();
        DiscardPile.AddCard(firstCard);

        // Update last played card for clients
        //Current.DiscardPileEntity.TopCardEntity.SetCard(To.Everyone, firstCard.Suit, firstCard.Rank);

        Sound.FromScreen(To.Everyone, "gamestart");
    }

    private bool IsWinner()
    {
        var winner = Hands.Where(h => h.Item2.Count == 0).FirstOrDefault();
        if(winner != null)
            return true;
        return false;
    }

    TimeSince turnStarted = 0;
    public override void Tick()
    {
        // Don't keep playing if we're by ourselves.
        if(Game.Clients.Count == 1)
        {
            // CLEANUP
            SetState(new WaitingForPlayersState());
        }

        if(turnStarted > GameManager.MaxTurnTime)
        {
            CurrentPlayer.ForcePlayCard();
        }
    }
}

/// <summary>
/// A player has won and game is over. Loops back to WaitingForPlayersState to reset game and begin a new one.
/// </summary>
public class GameOverState : BaseState
{
    public override string StateName() => "Game Over";

    protected override void Initialize()
    {
        Current.NotifyPlayerOfGameOver(To.Everyone);
    }

    TimeUntil resetGame = 8;
    public override void Tick()
    {
        if(resetGame <= 0)
            SetState(new WaitingForPlayersState());
    }
}
