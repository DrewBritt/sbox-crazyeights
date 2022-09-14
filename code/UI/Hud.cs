using Sandbox;
using Sandbox.UI;

namespace CrazyEights;

[UseTemplate]
public partial class Hud : RootPanel
{
    private Notifications Notifications { get; set; }
    private TableCards TableCards { get; set; }
    private SuitSelectionOverlay SuitSelectionOverlay { get; set; }

    /// <summary>
    /// Updates last played card with appropriate card.
    /// </summary>
    public void UpdatePlayedCard()
    {
        //TableCards.SetPlayCard(Game.Current.PlayingPile.GetTopCard());
    }

    /// <summary>
    /// Opens suit selection overlay for draw 4 and wild cards.
    /// </summary>
    /// <param name="c"></param>
    public void OpenSuitSelection(Card c)
    {
        SuitSelectionOverlay.OpenPanel(c);
    }

    /// <summary>
    /// Notification popup shows what card last player played.
    /// </summary>
    /// <param name="lastPlayedCl"></param>
    public void PlayedCardNotification(Client lastPlayedCl)
    {
        Notifications.PlayedCardPopup(lastPlayedCl);
    }

    /// <summary>
    /// Notification popup shows that player drew a card.
    /// </summary>
    /// <param name="lastPlayedCl"></param>
    public void DrewCardNotification(Client lastPlayedCl)
    {
        Notifications.DrewCardPopup(lastPlayedCl);
    }
}
