using Sandbox;
using Sandbox.UI;

namespace CrazyEights;

[UseTemplate]
[NavTarget("settings/gamesettings")]
public partial class GameSettings : Panel
{
    public TextEntry MaxTurnTime { get; set; }
    public string MaxTurnTimeString => Game.MaxTurnTime.ToString();

    public GameSettings()
    {
        MaxTurnTime.Bind("text", this, "MaxTurnTimeString");
    }

    public void SetMaxTurnTime() => ConsoleSystem.Run($"ce_maxturntime {MaxTurnTime.Text}");
    public void ResetGame() => ConsoleSystem.Run($"ce_resetgame");
    public void ForceTurn() => ConsoleSystem.Run($"ce_forceturn");
}
