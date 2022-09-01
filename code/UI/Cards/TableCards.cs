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

        BindClass("open", () => (Local.Pawn as Pawn).Hand.Cards.Count > 0);

        var drawCard = new CardPanel();
        drawCard.AddEventListener("onclick", () => ConsoleSystem.Run("crazyeights_drawcard"));
        DrawPile.AddChild(drawCard);

        playCard = new CardPanel();
        PlayPile.AddChild(playCard);
    }

    public void SetPlayCard(Card c)
    {
        playCard.SetCard(c);
    }
}
