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
        {
            Bot.All[0].Client.Kick();
        }
    }

    public override void BuildInput(InputBuilder input)
    {
        base.BuildInput(input);

        input.SetButton(InputButton.PrimaryAttack, true);
    }

    public override void Tick()
    {
        base.Tick();
    }
}
