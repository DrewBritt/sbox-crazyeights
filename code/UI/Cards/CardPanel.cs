using System;
using Sandbox;
using Sandbox.UI;

namespace CrazyEights;

/// <summary>
/// Displays a card in UI
/// </summary>
public partial class CardPanel : Panel
{
    public CardPanel(bool clickable = false)
    {
        StyleSheet.Load("/UI/Cards/CardPanel.scss");

        // Setup OnClick if clickable
        if(!clickable) return;

        // No card = draw pile card
        AddEventListener("onclick", () => ConsoleSystem.Run("crazyeights_drawcard"));
    }

    public CardPanel(Card c, bool clickable = false)
    {
        StyleSheet.Load("/UI/Cards/CardPanel.scss");
        SetCard(c);

        // Setup OnClick if clickable
        if(!clickable) return;

        /*if(card.Suit == CardSuit.Wild)
         * AddEventListener("onclick", () => ToggleWildSelection()); 
           return;  */

        AddEventListener("onclick", () => ConsoleSystem.Run($"crazyeights_playcard {c.NetworkIdent}"));
    }

    public void SetCard(Card c)
    {
        Style.BackgroundImage = Texture.Load(FileSystem.Mounted, c.FileName);
    }
}
