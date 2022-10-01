using Sandbox;
using Sandbox.UI;

namespace CrazyEights;

[UseTemplate]
public partial class Hints : Panel
{
    public InputHint InteractHint { get; set; }
    public InputHint EmoteHint { get; set; }
    public InputHint MenuHint { get; set; }

    public Hints()
    {
        InteractHint.BindClass("visible", () => (Local.Pawn as Pawn).GetInteractableEntity() != null);
        EmoteHint.SetClass("visible", true);
        MenuHint.BindClass("visible", () => ConsoleSystem.GetValue("sv_cheats") == "1");
    }
}
