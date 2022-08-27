using Sandbox;

namespace CrazyEights;

public partial class Card : Entity
{
    /// <summary>
    /// Suit/Color value of the card.
    /// </summary>
    [Net] public CardSuit Suit { get; set; }
    /// <summary>
    /// Rank/Number/Action value of the card.
    /// </summary>
    [Net] public CardRank Rank { get; set; }

    public override void Spawn()
    {
        base.Spawn();

        Transmit = TransmitType.Always;
    }

    public string FileName => $"ui/cards/{Suit}_{Rank}.png";
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
