namespace CrazyEights;

/// <summary>
/// Encapsulates a player's current hand of cards, using Deck functionality
/// </summary>
public partial class Hand : Deck
{
    public Hand() : base() { }

    public override void Spawn()
    {
        // Do nothing on spawn, as we simply want an empty list of cards to add to.
    }
}
