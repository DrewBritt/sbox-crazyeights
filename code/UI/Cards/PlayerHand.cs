using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;

namespace CrazyEights;

public partial class PlayerHand : Panel
{
    /// <summary>
    /// Local pawn's hand of cards
    /// </summary>
    private Hand Hand => (Local.Pawn as Pawn).Hand;

    /// <summary>
    /// Dictionary mapping Cards in pawn's hand to their on screen CardPanel
    /// </summary>
    private Dictionary<Card, CardPanel> CardPanels = new Dictionary<Card, CardPanel>();

    public PlayerHand()
    {
        StyleSheet.Load("/UI/Cards/PlayerHand.scss");
    }

    public override void Tick()
    {
        if(!Hand.IsValid()) return;

        // Don't check if vars are (seemingly) still matching
        if(Hand.Cards.Count == CardPanels.Count) return;

        // Remove invalid entries (cards no longer in player's hand)
        foreach(var entry in CardPanels)
        {
            if(Hand.Cards.Contains(entry.Key)) continue;

            entry.Value.Delete();
            CardPanels.Remove(entry.Key);
        }

        // Add newly added cards
        foreach(var c in Hand.Cards)
        {
            if(CardPanels.ContainsKey(c)) continue;

            var panel = new CardPanel(c);
            AddChild(panel);
            CardPanels.Add(c, panel);
        }
    }
}
