using Sandbox;
using Sandbox.UI;

namespace CrazyEights;

[UseTemplate]
public partial class Settings : NavPanel
{
    public Settings()
    {
        Navigate("settings/gamesettings");
    }

    [Event.BuildInput]
    public void BuildInput(InputBuilder b)
    {
        if(b.Pressed(InputButton.Menu) && ConsoleSystem.GetValue("sv_cheats") == "1")
            SetClass("open", !HasClass("open"));
    }
}
