﻿using Sandbox;
using Sandbox.UI;

namespace CrazyEights;

[UseTemplate]
public partial class SuitSelectionOverlay : Panel
{
    private CardEntity CardToPlay;

    public SuitSelectionOverlay()
    {
        // Close panel when clicking off of cards
        AddEventListener("onclick", () => Deactivate());

        // Otherwise, play card we're clicking on
        foreach(var panel in Children)
            panel.AddEventListener("onclick", () => SelectSuit(CardToPlay.NetworkIdent, GetChildIndex(panel)));
    }

    public void Activate(CardEntity c)
    {
        CardToPlay = c;

        // Iterate through each card and set class of whatever card we're playing
        foreach(var panel in Children)
        {
            panel.RemoveClass("changecolor");
            panel.RemoveClass("draw4");

            panel.AddClass(c.Rank.ToString());
        }

        AddClass("active");
    }

    public void Deactivate() => RemoveClass("active");

    private void SelectSuit(int networkIdent, int suitIndex)
    {
        ConsoleSystem.Run($"ce_playcard {networkIdent} {suitIndex} 0"); 
        Deactivate();
    }
}
