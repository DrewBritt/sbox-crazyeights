using Sandbox;
using Sandbox.UI;

namespace CrazyEights;

[UseTemplate]
public partial class Notifications : Panel
{
    private NotificationPopup Popup { get; set; }

    public void PlayedCardPopup(Client lastPlayedCl)
    {
        long playerId = lastPlayedCl.PlayerId;
        Card card = Game.Current.PlayingPile.GetTopCard();
        string message = $"{lastPlayedCl.Name} has played a {card.Suit} {card.Rank}";

        Popup.OpenPopup(playerId, message);
    }

    public void DrewCardPopup(Client lastPlayedCl)
    {
        long playerId = lastPlayedCl.PlayerId;
        string message = $"{lastPlayedCl.Name.Truncate(15, "...")} has drawn a card.";

        Popup.OpenPopup(playerId, message);
    }
}
