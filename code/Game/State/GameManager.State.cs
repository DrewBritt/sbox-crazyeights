using System;
using System.Collections.Generic;
using Sandbox;

namespace CrazyEights;

public partial class GameManager
{
    [ConVar.Replicated("ce_maxturntime", Help = "How long players have to play a card before their turn is forced.", Min = 1, Max = 60)]
    public static int MaxTurnTime { get; set; }

    [ConVar.Replicated("ce_fillbots", Help = "Should we fill the rest of the seats with bots on game start?")]
    public static bool FillBots { get; set; }

    #region State Management
    public BaseState CurrentState { get; set; } = new WaitingForPlayersState();

    [GameEvent.Tick.Server]
    public void OnTick()
    {
        if(Game.IsClient) return;

        CurrentState.Tick();
    }

    /// <summary>
    /// Game data playing deck/draw pile for this state.
    /// </summary>
    public Deck PlayingDeck => (CurrentState as PlayingState)?.PlayingDeck;

    /// <summary>
    /// Game data discard pile for this state.
    /// </summary>
    public Pile DiscardPile => (CurrentState as PlayingState)?.DiscardPile;

    /// <summary>
    /// Game data connection between players and their hands.
    /// </summary>
    public List<Tuple<Player, Hand>> Hands => (CurrentState as PlayingState).Hands;

    #endregion

    #region Player Management
    /// <summary>
    /// Player that should play the next card.
    /// </summary>
    [Net] public Player CurrentPlayer { get; set; }

    public int CurrentPlayerIndex => (CurrentState as PlayingState).CurrentPlayerIndex;

    /// <summary>
    /// Calculates the next player's index, needed for counter-clockwise direction play (going from index 0 to index Players.Count).
    /// </summary>
    /// <returns></returns>
    private int GetNextPlayerIndex(int curIndex)
    {
        // If next index is -1, wrap to last index in Players.
        if(curIndex + DirectionValue == -1)
            return Hands.Count - 1;

        // Else just wrap with modulo (or no wrapping is needed).
        return (curIndex + DirectionValue) % Hands.Count;
    }

    #endregion
}
