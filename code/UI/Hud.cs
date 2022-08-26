using Sandbox;
using Sandbox.UI;

namespace CrazyEights;

[UseTemplate]
public partial class Hud : RootPanel
{
    public TableCards TableCards { get; set; }

    public void UpdatePlayedCard()
    {
        TableCards.SetPlayCard(Game.Current.PlayingPile.GetTopCard());
    }
}
