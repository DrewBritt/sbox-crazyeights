namespace CrazyEights;

/// <summary>
/// Server/Game logic representation of a card. Generated in Decks and distributed by State logic.
/// </summary>
public partial class Card
{
    /// <summary>
    /// Suit/Color value of the card.
    /// </summary>
    public CardSuit Suit { get; set; }
    /// <summary>
    /// Rank/Number/Action value of the card.
    /// </summary>
    public CardRank Rank { get; set; }
    /// <summary>
    /// FileName of the texture for the given suit/rank combo.
    /// </summary>
    public string FileName => $"ui/cards/{Suit}_{Rank}.png".ToLower();

    /// <summary>
    /// Checks if this card is playable given the DiscardPile.GetTopCard().
    /// </summary>
    /// <returns></returns>
    public bool IsPlayable()
    {
        if(Suit == CardSuit.Wild)
            return true;

        var lastPlayed = GameManager.Current.DiscardPile?.GetTopCard();
        if(Suit == lastPlayed.Suit || Rank == lastPlayed.Rank)
            return true;

        return false;
    }
}

public enum CardSuit
{
    Red,
    Yellow,
    Blue,
    Green,
    Wild
}

public enum CardRank
{
    Zero,
    One,
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Draw2,
    Reverse,
    Skip,
    ChangeColor,
    Draw4
}
