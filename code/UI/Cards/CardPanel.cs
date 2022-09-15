using System;
using Sandbox;
using Sandbox.UI;

namespace CrazyEights;

/// <summary>
/// Displays a card in UI
/// </summary>
public partial class CardPanel : Panel
{
    public CardPanel()
    {
        StyleSheet.Load("/UI/Cards/CardPanel.scss");
    }

    /// <summary>
    /// Creates a CardPanel for use in Hand
    /// </summary>
    /// <param name="c"></param>
    public CardPanel(Card c)
    {
        StyleSheet.Load("/UI/Cards/CardPanel.scss");
        SetCard(c);
    }

    /// <summary>
    /// Set card texture to match argument
    /// </summary>
    /// <param name="c"></param>
    public void SetCard(Card c)
    {
        Style.BackgroundImage = Texture.Load(FileSystem.Mounted, c.FileName);
    }
}
