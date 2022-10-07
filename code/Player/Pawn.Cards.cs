using System.Linq;
using Sandbox;

namespace CrazyEights;

public partial class Pawn
{
    /// <summary>
    /// Stores cards the player is currently playing with.
    /// </summary>
    [Net] public PlayerHand Hand { get; set; }
    /// <summary>
    /// The chair the player's transform will be locked to.
    /// </summary>
    public PlayerChair PlayerChair { get; set; }
    /// <summary>
    /// Displays CardEntities in front of the player that allow suit selection when playing a Wild/Draw4.
    /// </summary>
    private SuitSelectionEntity SuitSelection { get; set; }

    /// <summary>
    /// Player has attempted to interact with a card/the discard pile.
    /// </summary>
    /// <param name="card"></param>
    private void CardInteract(Entity card)
    {
        if(IsServer) return;

        if(card is not Deck && card is not CardEntity) return;

        // Player wishes to draw a card
        if(card is Deck)
            ConsoleSystem.Run($"ce_drawcard {Client.IsBot}");

        if(card is CardEntity)
        {
            var cardEnt = card as CardEntity;
            if(cardEnt.Suit == CardSuit.Wild)
            {
                //Game.Current.Hud.ActivateSuitSelection(cardEnt);
                SuitSelection.Display(cardEnt.NetworkIdent, cardEnt.Rank);
            }
            else if(cardEnt.Tags.Has("suitselection"))
            {
                // SuitSelection holds the NetworkIdent of the original card the player wishes to play,
                // but the trace result (one of the suit selection cards) has the suit they wish to play
                ConsoleSystem.Run($"ce_playcard {SuitSelection.CardNetworkIdent} {cardEnt.Suit}");
            }
            else
                ConsoleSystem.Run($"ce_playcard {cardEnt.NetworkIdent} 0");
        }
    }

    /// <summary>
    /// Sets TimeSinceLastAction in Animator, to let animgraph perform Interact animation.
    /// </summary>
    [ClientRpc]
    public void DoInteractAnimation()
    {
        Animator.DidAction();
    }


    /// <summary>
    /// Tries to hide the SuitSelectionEntity of the called client.
    /// </summary>
    [ClientRpc]
    public void HideSuitSelection()
    {
        if(SuitSelection.IsValid())
            SuitSelection.Hide();
    }

    /// <summary>
    /// Calculates and force-plays a card from this pawn's hand.
    /// Used for bots/AFK behavior.
    /// Calls Play/Draw commands directly to avoid setting Caller, as calling as a command
    /// on the server defaults to the Host's client.
    /// </summary>
    public void ForcePlayCard()
    {
        // Now lets calculate what card to play
        var playable = Hand.PlayableCards();

        // Draw if no cards are currently playable
        if(!playable.Any())
        {
            Game.DrawCard();
            return;
        }

        // Try to play an action card first
        var actionCards = playable.Where(c => c.Rank == CardRank.Draw2 || c.Rank == CardRank.Reverse || c.Rank == CardRank.Skip);
        var numberCards = playable.Where(c => c.Rank < (CardRank)10);
        if(actionCards.Any())
        {
            // Play an action card 33% of the time, or if there are no number cards in the hand
            int rand = Rand.Int(1, 3);
            if(rand == 1 || !numberCards.Any())
            {
                // Get random action card
                var cards = actionCards.ToList();
                int index = Rand.Int(0, cards.Count - 1);
                var card = cards[index];

                Game.PlayCard(card.NetworkIdent);
                return;
            }
        }

        // Then try to play a wild card
        var wildCards = playable.Where(c => c.Suit == CardSuit.Wild);
        if(wildCards.Any())
        {
            // Play a wild 20% of the time, or if there are only wild cards in the hand (no numbers/action as determined above)
            int rand = Rand.Int(1, 5);
            if(rand == 1 || wildCards.SequenceEqual(playable))
            {
                // Get random wild card
                var cards = wildCards.ToList();
                int index = Rand.Int(0, cards.Count - 1);
                var card = cards[index];

                Game.PlayCard(card.NetworkIdent, Hand.GetMostPrevelantSuit());
                return;
            }
        }

        // Otherwise, just play a regular number card
        var numberList = numberCards.ToList();
        int randIndex = Rand.Int(0, numberList.Count - 1);
        var cardToPlay = numberList[randIndex];

        Game.PlayCard(cardToPlay.NetworkIdent);
    }
}
