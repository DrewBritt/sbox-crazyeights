using System.Linq;
using Sandbox;

namespace CrazyEights;

public partial class Bot : Sandbox.Bot
{
    [ConCmd.Admin("ce_addbot", Help = "Spawns a Crazy Eights bot (will play with you!)")]
    internal static void AddBot()
    {
        Host.AssertServer();
        if(Client.All.Count == ConsoleSystem.GetValue("maxplayers").ToInt())
        {
            Game.Current.CommandError(To.Single(ConsoleSystem.Caller), "Crazy Eights: Game is full! (Too many players)");
            return;
        }
        _ = new Bot();
    }

    [ConCmd.Admin("ce_kickallbots", Help = "Kicks all bots currently connected")]
    internal static void KickAllBots()
    {
        Host.AssertServer();
        while(Bot.All.Count > 0)
            Bot.All[0].Client.Kick();
    }

    TimeUntil toggleDab;
    public override void BuildInput(InputBuilder input)
    {
        base.BuildInput(input);

        if(toggleDab > 0) return;
        input.SetButton(InputButton.PrimaryAttack, !input.Down(InputButton.PrimaryAttack));
        toggleDab = Rand.Int(3, 19);
    }

    TimeUntil playCardDelay;
    public override void Tick()
    {
        base.Tick();

        if(!Game.Current.CurrentPlayer.IsValid() || Game.Current.CurrentPlayer != Client.Pawn) return;

        // Once above guard-clause passes, set playCardDelay only if it hasn't been set recently (within this turn)
        if(playCardDelay < -1)
            playCardDelay = Rand.Int(2, 4);

        // Wait for delay
        if(playCardDelay > 0) return;

        PlayCard();

        // Ensure bot doesn't play again after turn
        playCardDelay = -2;
    }

    /// <summary>
    /// Calculates and plays a card from the bot's hand.
    /// </summary>
    private void PlayCard()
    {
        // Now lets calculate what card to play
        var pawn = Client.Pawn as Pawn;
        var playable = pawn.Hand.PlayableCards();

        // Draw if no cards are currently playable
        if(!playable.Any())
        {
            ConsoleSystem.Run($"ce_drawcard {true}");
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

                ConsoleSystem.Run($"ce_playcard {card.NetworkIdent} 0 {true}");
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

                ConsoleSystem.Run($"ce_playcard {card.NetworkIdent} {pawn.Hand.GetMostPrevelantSuit()} {true}");
                return;
            }
        }

        // Otherwise, just play a regular number card
        var numberList = numberCards.ToList();
        int randIndex = Rand.Int(0, numberList.Count - 1);
        var cardToPlay = numberList[randIndex];
        ConsoleSystem.Run($"ce_playcard {cardToPlay.NetworkIdent} 0 {true}");
    }
}
