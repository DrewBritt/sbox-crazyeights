using Sandbox;
using Sandbox.UI;

namespace CrazyEights;

public partial class CardPanel : Panel
{
    public CardPanel(Card card)
    {
        StyleSheet.Load("/UI/Cards/CardPanel.scss");
        Style.BackgroundImage = Texture.Load(FileSystem.Mounted, card.FileName);
    }
}
