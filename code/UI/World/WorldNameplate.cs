using Sandbox;
using Sandbox.UI;

namespace CrazyEights;

[UseTemplate]
public partial class WorldNameplate : WorldPanel
{
    private Pawn pawn;
    private bool hasSet = false;

    public Image Avatar { get; set; }
    public Label Name { get; set; }
    public TimeSince Appeared = 0;

    public WorldNameplate(Pawn pawn)
    {
        this.pawn = pawn;
        var width = 1000;
        var height = 1000;
        PanelBounds = new Rect(-width * .5f, -height * .5f, width, height);

        Position = pawn.EyePosition + Vector3.Up * 16;
        BindClass("active", () => Appeared <= 1);
    }

    [Event.Frame]
    private void OnFrame()
    {
        if(!pawn.IsValid()) return;
        if(!pawn.Client.IsValid()) return;
        
        Rotation = Rotation.LookAt(-Screen.GetDirection(new Vector2(Screen.Width * 0.5f, Screen.Height * 0.5f)));

        // Do this shit once way later instead of in constructor or something reasonable,
        // as pawn.Client is null in pawn.ClientSpawn() and therefore this constructor.
        if(!hasSet)
        {
            Avatar.SetTexture($"avatarbig:{pawn.Client.PlayerId}");
            Name.Text = pawn.Client.Name.Truncate(23, "...");
            hasSet = true;
        }
    }
}
