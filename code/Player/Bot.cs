using System;
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

    TimeUntil acquireNewLookTarget;
    Vector3 lookTarget;
    public override void BuildInput()
    {
        if(acquireNewLookTarget < 0)
            acquireLookTarget();

        var player = Client.Pawn as Player;
        player.Controller.LookInput = Rotation.LookAt(lookTarget - player.Controller.EyePosition, Vector3.Up).Angles().Normal;
    }

    /// <summary>
    /// Calculates a random LookAt target for the bot every 10 seconds.
    /// </summary>
    private void acquireLookTarget()
    {
        acquireNewLookTarget = 10;

        int random = Game.Random.Next(0,3);
        Player player = Client.Pawn as Player;

        if(random == 0) // Find a random player to look at (not themselves)
            lookTarget = Entity.All.OfType<Player>().OrderBy(p => Guid.NewGuid()).Where(p => p != player).FirstOrDefault().Controller.EyePosition;
        else if(random == 1) // Look at discard pile (if it exists)
            lookTarget = GameManager.Current.DiscardPileEntity?.Transform.Position ?? player.Position * player.Rotation * 100f;
        else
            lookTarget = player.HandDisplay.Cards.FirstOrDefault()?.Position ?? player.Position * player.Rotation * 100f;
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
        var pawn = Client.Pawn as Player;
        pawn.ForcePlayCard();

        // Chance for emote
        if(Game.Random.Next(8) == 0)
            pawn.Animator.PlayEmote(PlayerEmote.ThumbsUp);

        // Ensure bot doesn't play again after turn
        playCardDelay = -2;
    }
}
