using Sandbox;
using Sandbox.UI;

namespace CrazyEights;

[UseTemplate]
public partial class Hud : RootPanel
{
    private TableCards TableCards { get; set; }
    private SuitSelectionOverlay SuitSelectionOverlay { get; set; }

    public void UpdatePlayedCard()
    {
        TableCards.SetPlayCard(Game.Current.PlayingPile.GetTopCard());
    }

    public void OpenSuitSelection(Card c)
    {
        SuitSelectionOverlay.OpenPanel(c);
    }
}
