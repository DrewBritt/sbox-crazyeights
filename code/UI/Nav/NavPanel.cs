using Sandbox.UI;

namespace CrazyEights;

public partial class NavPanel : Panel
{
    public Panel NavContent { get; set; }
    public string ContentURL { get; set; }

    public void Navigate(string url)
    {
        var attribute = NavTargetAttribute.Get(url);
        if(attribute is null)
        {
            Log.Error($"Sketch: NavTarget not found: {url}");
            return;
        }

        var panel = TypeLibrary.Create<Panel>(attribute.TargetType);
        NavContent.DeleteChildren(true);
        panel.Parent = NavContent;
        ContentURL = url;
    }

    /// <summary>
    /// Navigates to the given URL
    /// </summary>
    /// <param name="url">URL of content to load</param>
    [PanelEvent]
    public bool NavigateEvent(string url)
    {
        Navigate(url);
        return false;
    }
}
