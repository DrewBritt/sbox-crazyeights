using System.Collections.Generic;
using System.Linq;
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
            // CLEANUP GAME ENTITIES
            // Won't run on game startup (Current will return null for some reason).
            // Should only run when players disconnect and Client.All.Count == 1.
            if(Current == null) return;

            foreach(var p in Current.Players)
            {
                p.Hand.ClearCards();
                p.Hand = null;
            }

            Current.PlayingDeck?.Delete();
            Current.DiscardPile?.Delete();
            Current.Players?.Clear();
            Current.CurrentPlayerIndex = 0;
        }

        TimeUntil startGame = -1;
        public override void Tick()
        {
            if(Client.All.Count > 1)
            {
                // Set startGame if it hasnt been set recently
                if(startGame <= -1) startGame = 10;

                // After delay, goto next state
                if(startGame <= 0)
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
            List<Client> players = Client.All.Where(p => p.Pawn is Pawn).ToList();
            for(int i = 0; i < players.Count; i++)
            {
                Pawn player = players[i].Pawn as Pawn;
                Current.Players.Add(player);
                player.Hand = new PlayerHand()
                {
                    Owner = player,
                };

                // 7 cards for each player.
                List<Card> cards = new List<Card>();
                for(int j = 0; j < 7; j++)
                    cards.Add(Current.PlayingDeck.GrabTopCard());
                player.Hand.AddCards(cards);
            }

            // Game plays starting card from top of deck onto discard pile.
            // Shuffles until first card is a valid starting card (not wild)
            Current.DiscardPile = new Pile();
            while(Current.PlayingDeck.GetTopCard().Suit == CardSuit.Wild)
                Current.PlayingDeck.Shuffle();
            var firstCard = Current.PlayingDeck.GrabTopCard();
            Current.DiscardPile.AddCard(firstCard);

            // Update last played card for clients
            Current.DiscardPile.TopCardEntity.SetCard(To.Everyone, firstCard.Suit, firstCard.Rank);

            Sound.FromScreen(To.Everyone, "gamestart");
            Current.NotifyPlayerOfTurn(To.Single(Current.CurrentPlayer.Client));
        }

        public TimeSince TurnStarted = 0;
        public override void Tick()
        {
            base.Tick();
            if(TurnStarted > Game.MaxTurnTime)
                Current.CurrentPlayer.ForcePlayCard();
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
            Current.NotifyPlayerOfGameOver(To.Everyone);
        }

        public override void Tick()
        {
            if(stateStart > 8)
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

    [ConVar.Replicated("ce_maxturntime", Help = "How long players have to play a card before their turn is forced.", Min = 1, Max = 60)]
    public static int MaxTurnTime { get; set; }

    #endregion

    #region Card Management

    /// <summary>
    /// Persistent deck used for drawing cards.
    /// </summary>
    public Deck PlayingDeck { get; set; }

    /// <summary>
    /// Pile in which player's cards are played onto.
    /// </summary>
    [Net] public Pile DiscardPile { get; set; }

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
    public Pawn? CurrentPlayer => Players.ElementAtOrDefault(CurrentPlayerIndex);

    #endregion
}
