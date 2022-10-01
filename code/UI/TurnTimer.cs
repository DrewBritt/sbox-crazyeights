using System;
using Sandbox;
using Sandbox.UI;
using static CrazyEights.Game;

namespace CrazyEights;

[UseTemplate]
public partial class TurnTimer : Panel
{
    public TimeUntil turnOver { get; set; } = -1;
    public string Time => TimeString();
    public Label TimerText { get; set; }

    public TurnTimer()
    {
        BindClass("active", () => Game.Current.CurrentPlayer == Local.Client.Pawn && turnOver > 0 && turnOver < 10);
        Bind("Time", this, "TimerText.Text");
    }

    TimeSince soundPlayed;
    public override void Tick()
    {
        if(Game.Current.CurrentPlayer != Local.Pawn) return;

        // Play tick sound every second when panel is active
        if(turnOver < 10 && turnOver > 0 && soundPlayed > 1)
        {
            soundPlayed = 0;
            Sound.FromScreen("countdowntick");
        }
    }

    /// <summary>
    /// Hacky function workaround to bind formatted timer text to TimerText.Text.
    /// </summary>
    /// <returns></returns>
    private string TimeString()
    {
        var span = TimeSpan.FromSeconds(turnOver);
        return $"{span.Minutes:D1}:{span.Seconds:D2}";
    }

    /// <summary>
    /// Sets turnOver to Max Turn Time, priming the panel to appear.
    /// </summary>
    public void Activate()
    {
        turnOver = Game.MaxTurnTime;
    }
}
