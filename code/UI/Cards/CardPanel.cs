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

        // Open suit selection overlay if card is a wildcard
        if(c.Suit == CardSuit.Wild)
        { 
           AddEventListener("onclick", () => OpenSuitSelection(c)); 
           return;
        }

        // Otherwise, play the card
        AddEventListener("onclick", () => ConsoleSystem.Run($"crazyeights_playcard {c.NetworkIdent}"));
    }

    /// <summary>
    /// Set card texture to match argument
    /// </summary>
    /// <param name="c"></param>
    public void SetCard(Card c)
    {
        Style.BackgroundImage = Texture.Load(FileSystem.Mounted, c.FileName);
    }

    private void OpenSuitSelection(Card c)
    {
        Game.Current.Hud.OpenSuitSelection(c);
    }
}
