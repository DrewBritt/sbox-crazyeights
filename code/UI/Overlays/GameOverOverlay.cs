using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace CrazyEights;

[UseTemplate]
public partial class GameOverOverlay : Panel
{
    private Image Avatar { get; set; }
    private Label Text { get; set; }

    public GameOverOverlay()
    {
        BindClass("active", () => activated < 5f);
    }

    TimeSince activated = 6f;
    public void Activate(Client winner)
    {
        Avatar.SetTexture($"avatarbig:{winner.PlayerId}");
        Text.Text = $"{winner.Name} has won the game!";
        activated = 0;
    }
}
