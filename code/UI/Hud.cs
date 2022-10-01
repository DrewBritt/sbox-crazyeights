using Sandbox;
using Sandbox.UI;

namespace CrazyEights;

[UseTemplate]
public partial class Hud : RootPanel
{
    private SuitSelectionOverlay SuitSelectionOverlay { get; set; }
    private TurnTimer TurnTimer { get; set; }
    private Crosshair Crosshair { get; set; }

    /// <summary>
    /// Activates suit selection overlay for draw 4 and wild cards.
    /// </summary>
    /// <param name="c"></param>
    public void ActivateSuitSelection(CardEntity c)
    {
        SuitSelectionOverlay.Activate(c);
    }

    /// <summary>
    /// Activates turn notification panel.
    /// </summary>
    public void ActivateTurnTimer()
    {
        TurnTimer.ActivateTurnTimer();
    }

    /// <summary>
    /// Activates crosshair, fading it in.
    /// </summary>
    public void ActivateCrosshair()
    {
        Crosshair.Activate();
    }
    }
}
