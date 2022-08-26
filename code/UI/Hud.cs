using Sandbox;
using Sandbox.UI;

namespace CrazyEights;

//[UseTemplate] - Doesn't work?
public partial class Hud : HudEntity<RootPanel>
{
    public Hud()
    {
        if(IsClient)
            RootPanel.SetTemplate("Code/UI/Hud.html");
    }
}
