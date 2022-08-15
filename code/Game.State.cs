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
    /// Waiting for more players to connect before beginning game
    /// </summary>
    public class WaitingForPlayersState : BaseState
    {
        public override string StateName() => "Waiting for Players";

        public WaitingForPlayersState() : base() { }

        public override void Tick()
        {
            if(Client.All.Count > 1)
            {
                SetState(new StagingState());
            }
        }
    }

    /// <summary>
    /// Preparing the game to be played
    /// </summary>
    public class StagingState : BaseState
    {
        public override string StateName() => "Staging";

        public StagingState() : base()
        {
            // Generate deck to be used in play
            Current.PlayingDeck = new Deck();
            Current.PlayingDeck.Shuffle();

            // Distribute cards to players
            for(int i = 0; i < Client.All.Count; i++)
            {
                Pawn player = Client.All[i].Pawn as Pawn;

                // 7 cards for each player
                for(int j = 0; j < 7; j++)
                    player.Hand.AddCard(Current.PlayingDeck.GetTopCard());
            }

            // Game plays starting card from top of deck
            Current.PlayingPile = new Pile();
            Current.PlayingPile.AddCard(Current.PlayingDeck.GetTopCard());

            SetState(new PlayingState());
        }

        public override void Tick()
        {
            base.Tick();
        }
    }

    public class PlayingState : BaseState
    {
        public override string StateName() => "Playing";

        public PlayingState() : base()
        {

        }
    }

    #endregion

    #region State Management

    public BaseState CurrentState = new WaitingForPlayersState();

    [Event.Tick]
    public void OnTick()
    {
        if(Host.IsClient) return;

        CurrentState.Tick();
    }

    #endregion

    #region Card Management

    /// <summary>
    /// Persistent deck used for drawing cards
    /// </summary>
    public Deck PlayingDeck { get; set; }
    /// <summary>
    /// Pile in which player's cards are played onto
    /// </summary>
    [Net] public Pile PlayingPile { get; set; }

    #endregion

    #region Player Management

    [Net] public Pawn CurrentPlayer { get; set; }

    #endregion
}
