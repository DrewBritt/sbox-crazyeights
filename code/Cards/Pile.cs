namespace CrazyEights;

/// <summary>
/// Encapsulates a pile/stack of cards, using Deck functionality and overriding where necessary.
/// </summary>
public partial class Pile : Deck
{
    private PileEntity DiscardPile => GameManager.Current.DiscardPileEntity;

    // Don't generate a deck on instantiation, instead we'll add cards ourselves (discard pile).
    protected override void Initialize()
    {
        if(DiscardPile == null)
            GameManager.Current.DiscardPileEntity = new PileEntity();
    }

    /// <summary>
    /// Add card on "top" of pile (beginning of list).
    /// </summary>
    /// <param name="card"></param>
    public override void AddCard(Card card)
    {
        Cards.Insert(0, card);

        if(DiscardPile != null)
            DiscardPile.SetTopCard(card);
    }
}
