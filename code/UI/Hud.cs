﻿using Sandbox;
using Sandbox.UI;

namespace CrazyEights;

[UseTemplate]
public partial class Hud : RootPanel
{
    private SuitSelectionOverlay SuitSelectionOverlay { get; set; }
    private TurnTimer TurnTimer { get; set; }
    private Crosshair Crosshair { get; set; }

    /// <summary>
    /// Opens suit selection overlay for draw 4 and wild cards.
    /// </summary>
    /// <param name="c"></param>
    public void OpenSuitSelection(CardEntity c)
    {
        SuitSelectionOverlay.OpenPanel(c);
    }

    /// <summary>
    /// Opens turn notification panel.
    /// </summary>
    public void ResetTurnTimer()
    {
        TurnTimer.ResetTurnTimer();
    }

    /// <summary>
    /// Resets Crosshair's timeout timer, causing it to appear.
    /// </summary>
    public void EnableCrosshair()
    {
        Crosshair.Hovered();
    }
}
