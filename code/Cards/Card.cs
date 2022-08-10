using Sandbox;

namespace CrazyEights;

public partial class Card : Entity
{
    [Net, Local] public CardSuit Suit { get; set; }
    [Net, Local] public CardRank Rank { get; set; }

    public override void Spawn()
    {
        base.Spawn();

        Transmit = TransmitType.Always;
    }
}

public enum CardSuit
{
    Red,
    Yellow,
    Green,
    Blue,
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
