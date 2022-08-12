using Sandbox;

namespace CrazyEights;

public partial class Game
{
    #region States
    protected class BaseState
    {
        public virtual string StateName() => GetType().ToString();

        public virtual void Tick()
        {
            if(Client.All.Count == 1)
                SetState(new WaitingForPlayersState());
        }

        protected void SetState(BaseState state)
        {
            //Current.CurrentState
        }
    }

    /// <summary>
    /// Waiting for more players to connect before beginning game
    /// </summary>
    private class WaitingForPlayersState : BaseState
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
    private class StagingState : BaseState
    {
        public override string StateName() => "Staging";

        public StagingState() : base()
        {
            // Generate deck to be used in play
            Deck deck = new Deck();

            deck.ShuffleDeck();

            for(int i = 0; i < 10; i++)
                Log.Info(deck.Cards[i]);
            // Distribute cards to players
            // Game plays starting card from top of deck
            // SetState(PlayingState());
        }

        public override void Tick()
        {
            base.Tick();
        }
    }

    #endregion
}
