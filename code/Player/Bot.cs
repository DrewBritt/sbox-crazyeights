using System.Linq;
using Sandbox;
using static CrazyEights.GameManager;

namespace CrazyEights;

public partial class Bot : Sandbox.Bot
{
    [ConCmd.Admin("ce_addbot", Help = "Spawns a Crazy Eights bot (will play with you!)")]
    internal static void AddBot()
    {
        Game.AssertServer();
        if(Game.Clients.Count == ConsoleSystem.GetValue("maxplayers").ToInt())
        {
            GameManager.Current.CommandError(To.Single(ConsoleSystem.Caller), "Crazy Eights: Game is full! (Too many players)");
            return;
        }
        _ = new Bot();
    }

    [ConCmd.Admin("ce_kickallbots", Help = "Kicks all bots currently connected")]
    internal static void KickAllBots()
    {
        Game.AssertServer();
        while(Bot.All.Count > 0)
            Bot.All[0].Client.Kick();
    }

    public override void BuildInput(InputBuilder input)
    {
        base.BuildInput(input);
    }

    TimeUntil playCardDelay;
    public override void Tick()
    {
        base.Tick();

        if(GameManager.Current.CurrentState is not PlayingState) return;

        if(!GameManager.Current.CurrentPlayer.IsValid() || GameManager.Current.CurrentPlayer != Client.Pawn) return;

        // Once above guard-clause passes, set playCardDelay only if it hasn't been set recently (within this turn)
        if(playCardDelay < -1)
            playCardDelay = Game.Random.Int(2, 4);

        // Wait for delay
        if(playCardDelay > 0) return;

        // Then play card
        var pawn = Client.Pawn as Pawn;
        pawn.ForcePlayCard();

        // Ensure bot doesn't play again after turn
        playCardDelay = -2;
    }
}
