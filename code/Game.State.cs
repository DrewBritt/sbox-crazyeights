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
            Deck deck = new Deck();

            deck.Shuffle();

            // Distribute cards to players
            for(int i = 0; i < Client.All.Count; i++)
            {
                Pawn player = Client.All[i].Pawn as Pawn;

                // 7 cards for each player
                for(int j = 0; j < 7; j++)
                    player.Hand.AddCard(deck.GetTopCard());
            }

            Log.Info(deck.Cards.Count);

            // Game plays starting card from top of deck
            // SetState(PlayingState());
        }

        public override void Tick()
        {
            base.Tick();
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
}
