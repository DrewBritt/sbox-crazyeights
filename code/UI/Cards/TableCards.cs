using Sandbox;
using Sandbox.UI;

namespace CrazyEights;

public partial class TableCards : Panel
{
    public Panel DrawPile { get; set; }
    public Panel PlayPile { get; set; }

    private CardPanel playCard;

    public TableCards()
    {
        SetTemplate("Code/UI/Cards/TableCards.html");

        DrawPile.AddChild(new CardPanel(true));

        playCard = new CardPanel(false);
        PlayPile.AddChild(playCard);
    }

    public void SetPlayCard(Card c)
    {
        playCard.SetCard(c);
    }
}
