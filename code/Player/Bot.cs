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

    Vector3 lookAt;
    public override void BuildInput()
    {
        if(Client.Pawn is not Player) return;

        acquireLookTarget();

        var player = Client.Pawn as Player;
        player.Controller.LookInput = Rotation.LookAt(lookAt - player.Controller.EyePosition, Vector3.Up).Angles().Normal;
    }

    TimeUntil newTargetWait = 0;
    Entity lookAtEntity;
    /// <summary>
    /// Calculates a random LookAt target for the bot every 3-9 seconds, and updates the LookAt 
    /// </summary>
    private void acquireLookTarget()
    {
        Player player = Client.Pawn as Player;

        if(newTargetWait < 0)
        {
            newTargetWait = Game.Random.Next(3, 10);

            // Determine what we want to look at.
            LookAtAction lookAction = (LookAtAction) Game.Random.Next(5);
            switch(lookAction)
            {
                case LookAtAction.None: lookAtEntity = null; break;
                case LookAtAction.HandDisplay: lookAtEntity = player.HandDisplay?.Cards.FirstOrDefault() ?? null; break;
                case LookAtAction.DiscardPile: lookAtEntity = GameManager.Current.DiscardPileEntity ?? null; break;
                case LookAtAction.Deck: lookAtEntity = GameManager.Current.PlayingDeckEntity ?? null; break;
                case LookAtAction.Player: lookAtEntity = Entity.All.OfType<Player>().OrderBy(p => Guid.NewGuid()).Where(p => p != player).FirstOrDefault(); break;
            }
        }

        // Update position of lookAt constantly in case target (player) is moving around.
        if(lookAtEntity.IsValid() && lookAtEntity is Player p)
            lookAt = p.Controller.EyePosition;
        else
            lookAt = lookAtEntity.IsValid() ? lookAtEntity.Position : player.Position * player.Rotation * 100;
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
            pawn.Animator.PlayEmote((PlayerEmote)Game.Random.Next(1, 3));

        // Ensure bot doesn't play again after turn
        playCardDelay = -2;
    }

    private enum LookAtAction
    {
        None,
        HandDisplay,
        DiscardPile,
        Deck,
        Player
    }
}
