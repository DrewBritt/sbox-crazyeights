﻿using System.Linq;
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
            playCardDelay = Rand.Int(2, 5);

        // Wait for delay
        if(playCardDelay > 0) return;

        // Calculate card to play
        var pawn = Client.Pawn as Pawn;
        var playable = pawn.Hand.PlayableCards().ToList();
        if(playable.Count > 0)
            ConsoleSystem.Run($"ce_playcard {playable[Rand.Int(0, playable.Count-1)].NetworkIdent} {pawn.Hand.GetMostPrevelantSuit()} {true}");
        else
            ConsoleSystem.Run($"ce_drawcard {true}");
    }
}
