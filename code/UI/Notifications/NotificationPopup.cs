using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace CrazyEights;

[UseTemplate]
public partial class NotificationPopup : Panel
{
    public Image Avatar { get; set; }
    public Label Text { get; set; }

    private TimeSince opened;

    public void OpenPopup(long playerId, string text)
    {
        Avatar.Texture = Texture.LoadAvatar(playerId);
        Text.Text = text;
        AddClass("open");
        opened = 0;
    }

    public void ClosePopup() => RemoveClass("open");

    public override void Tick()
    {
        // Close popup after 3 seconds
        if(opened > 3)
            ClosePopup();
    }
}
