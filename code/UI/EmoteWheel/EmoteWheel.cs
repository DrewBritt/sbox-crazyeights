using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;

namespace CrazyEights.UI;

[StyleSheet("/UI/EmoteWheel/EmoteWheel.scss")]
public partial class EmoteWheel : RadialMenu
{
    public static EmoteWheel Current { get; private set; }

    public override InputButton Button => InputButton.SecondaryAttack;

    public EmoteWheel()
    {
        Current = this;
    }

    public override void Populate()
    {
        var emotes = new List<PlayerEmote>
        {
            PlayerEmote.ThumbsUp,
            PlayerEmote.ThumbsDown
        };

        var player = Game.LocalPawn as Player;

        foreach(var emote in emotes)
        {
            var name = emote;
            //var title = emote.ToString();
            //if(title.Length > 5) title = title.Substring(0, 6) + " " + title.Substring(6, title.Length - 6);

            var icon = $"ui/{emote.ToString().ToLower()}.png";
            var description = "";
            var item = AddItem("", description, icon, () => Select(name));
        }

        base.Populate();
    }

    protected override bool ShouldOpen() => Game.LocalPawn is Player;

    private void Select(PlayerEmote emote)
    {
        var player = Game.LocalPawn as Player;
        GameManager.Emote(emote);
        player.Animator.PlayEmote(emote);
    }
}
