using Sandbox;
using Sandbox.UI;

namespace CrazyEights;

public partial class Crosshair : Panel
{
    TimeSince hoveredOverInteractable;
    public Crosshair()
    {
        StyleSheet.Load("/ui/Crosshair.scss");

        BindClass("active", () => hoveredOverInteractable < .1f);
    }

    public void Activate()
    {
        hoveredOverInteractable = 0;
    }
}
