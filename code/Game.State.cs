using System.Collections.Generic;
using Sandbox;

namespace CrazyEights;

public partial class Game
{
    #region States
    public class BaseState
    {
        public virtual string StateName() => GetType().ToString();

        public virtual void Tick()
        {
            if(Client.All.Count == 1)
                SetState(new WaitingForPlayersState());
        }

        protected void SetState(BaseState state)
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

        public WaitingForPlayersState() : base() 
        {
            // Don't run on game startup (current returns null always).
            // Should only run when players disconnect and Client.All.Count <= 1.
            if(Current == null) return;

            Current.PlayingDeck.ClearCards();
            Current.PlayingDeck.Delete();

            Current.PlayingPile.ClearCards();
            Current.PlayingPile.Delete();

            foreach(var p in Current.Players)
            {
                p.Hand.ClearCards();
                p.Hand.Delete();
            }

            Current.Players.Clear();
        }

        public override void Tick()
        {
            if(Client.All.Count > 1)
            {
                SetState(new PlayingState());
            }
        }
    }

    /// <summary>
    /// Players are currently playing cards.
    /// </summary>
    public class PlayingState : BaseState
    {
        public override string StateName() => "Playing";

        public PlayingState() : base()
        {
            // Generate deck to be used in play.
            Current.PlayingDeck = new Deck();
            Current.PlayingDeck.Shuffle();

            // Distribute cards to players.
            for(int i = 0; i < Client.All.Count; i++)
            {
                Pawn player = Client.All[i].Pawn as Pawn;
                Current.Players.Add(player);
                player.Hand = new Hand();

                // 7 cards for each player.
                for(int j = 0; j < 7; j++)
                    player.Hand.AddCard(Current.PlayingDeck.GrabTopCard());
            }

            // Game plays starting card from top of deck.
            Current.PlayingPile = new Pile();
            Current.PlayingPile.AddCard(Current.PlayingDeck.GrabTopCard());

            Current.PrintPlay(To.Everyone, Current.Players[0].Client);

            Current.PrintCards(To.Everyone);
        }

        public override void Tick()
        {
            base.Tick();
        }
    }

    /// <summary>
    /// A player has won and game is over. Loops back to WaitingForPlayersState to reset game and begin a new one.
    /// </summary>
    public class GameOverState : BaseState
    {
        public override string StateName() => "Game Over";
        private RealTimeSince stateStart { get; }

        public GameOverState() : base()
        {
            stateStart = 0;
        }

        public override void Tick()
        {
            if(stateStart > 10)
                SetState(new WaitingForPlayersState());
        }
    }

    #endregion

    #region State Management

    public BaseState CurrentState { get; set; } = new WaitingForPlayersState();

    [Event.Tick]
    public void OnTick()
    {
        if(Host.IsClient) return;

        CurrentState.Tick();
    }

    #endregion

    #region Card Management

    /// <summary>
    /// Persistent deck used for drawing cards.
    /// </summary>
    public Deck PlayingDeck { get; set; }

    /// <summary>
    /// Pile in which player's cards are played onto.
    /// </summary>
    [Net] public Pile PlayingPile { get; set; }

    #endregion

    #region Player Management
    
    /// <summary>
    /// All players in this round. Separate from Client.All as players are not dealt in if they join after round start.
    /// </summary>
    [Net] public IList<Pawn> Players { get; set; }
    [Net] private int CurrentPlayerIndex { get; set; } = 0;

    /// <summary>
    /// Calculates the next player's index, needed for counter-clockwise direction play (going from index 0 to index Players.Count).
    /// </summary>
    /// <returns></returns>
    private int GetNextPlayerIndex()
    {
        // If next index is -1, wrap to last index in Players.
        if(CurrentPlayerIndex + DirectionValue == -1)
            return Players.Count - 1;

        // Else just wrap with modulo (or no wrapping is needed).
        return (CurrentPlayerIndex + DirectionValue) % Players.Count;
    }

    /// <summary>
    /// Player that should play the next card.
    /// </summary>
    public Pawn CurrentPlayer => Players[CurrentPlayerIndex];

    #endregion
}
