using System.Linq;
using Sandbox;

namespace CrazyEights;

public partial class Player
{
    /// <summary>
    /// Displays cards the player currently has in their hand.
    /// </summary>
    [BindComponent] public HandDisplayComponent HandDisplay { get; }

    /// <summary>
    /// Tries to grab game state representation of player's hand.
    /// </summary>
    /// <returns></returns>
    public Hand Hand()
    {
        var tuple = (GameManager.Current.CurrentState as PlayingState).Hands.Where(c => c.Item1 == this).FirstOrDefault();
        if(tuple != null)
            return tuple.Item2;
        return null;
    }

    /// <summary>
    /// Tries to hide the SuitSelectionEntity of the called client.
    /// </summary>
    [ClientRpc]
    public void HideSuitSelection()
    {
        Controller.HideSuitSelection();
    }

    /// <summary>
    /// Calculates and force-plays a card from this pawn's hand.
    /// Used for bots/AFK behavior.
    /// Calls Play/Draw commands directly to avoid setting Caller, as calling as a command
    /// on the server defaults to the Host's client.
    /// Uses CardEntities rather than Player.Hand() in order to transmit NetworkIdent for specific card we want to play.
    /// </summary>
    public void ForcePlayCard()
    {
        // Now lets calculate what card to play
        var playable = HandDisplay.PlayableCards();

        // Draw if no cards are currently playable
        if(!playable.Any())
        {
            GameManager.DrawCard();
            return;
        }

        // Try to play an action card first
        var actionCards = playable.Where(c => c.Rank == CardRank.Draw2 || c.Rank == CardRank.Reverse || c.Rank == CardRank.Skip);
        var numberCards = playable.Where(c => c.Rank < (CardRank)10);
        if(actionCards.Any())
        {
            // Play an action card 33% of the time, or if there are no number cards in the hand
            int rand = Game.Random.Int(1, 3);
            if(rand == 1 || !numberCards.Any())
            {
                // Get random action card
                var cards = actionCards.ToList();
                int index = Game.Random.Int(0, cards.Count - 1);
                var card = cards[index];

                GameManager.PlayCard(card.NetworkIdent);
                return;
            }
        }

        // Then try to play a wild card
        var wildCards = playable.Where(c => c.Suit == CardSuit.Wild);
        if(wildCards.Any())
        {
            // Play a wild 20% of the time, or if there are only wild cards in the hand (no numbers/action as determined above)
            int rand = Game.Random.Int(1, 5);
            if(rand == 1 || wildCards.SequenceEqual(playable))
            {
                // Get random wild card
                var cards = wildCards.ToList();
                int index = Game.Random.Int(0, cards.Count - 1);
                var card = cards[index];

                GameManager.PlayCard(card.NetworkIdent, HandDisplay.GetMostPrevelantSuit());
                return;
            }
        }

        // Otherwise, just play a regular number card
        var numberList = numberCards.ToList();
        int randIndex = Game.Random.Int(0, numberList.Count - 1);
        var cardToPlay = numberList[randIndex];

        GameManager.PlayCard(cardToPlay.NetworkIdent);
    }
}
