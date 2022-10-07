using Sandbox;

namespace CrazyEights;

/// <summary>
/// Entity intended to be spawned clientside to manage SuitSelection CardEntity's (each card is a different wild/draw4 suit).
/// </summary>
public partial class SuitSelectionEntity : Entity
{
    /// <summary>
    /// NetworkIdent of the CardEntity that is to be played following suit selection.
    /// </summary>
    public int CardNetworkIdent { get; private set; }
    /// <summary>
    /// Contains the CardEntities used for suit selection.
    /// </summary>
    private CardEntity[] cards;

    public SuitSelectionEntity()
    {
        cards = new CardEntity[4];
    }

    /// <summary>
    /// Displays Suit Selection cards with appropriate rank.
    /// When a card is interacted with, PlayCard is called with ident and the selected
    /// suit as parameters.
    /// </summary>
    /// <param name="ident"></param>
    /// <param name="rank"></param>
    public void Display(int ident, CardRank rank)
    {
        // Clear previously displayed cards in case we display twice quickly or something (interact with two different wildcards)
        Clear();

        // Populate cards
        CardNetworkIdent = ident;
        for(int i = 0; i < 4; i++)
        {
            cards[i] = new CardEntity();
            cards[i].SetCard((CardSuit)i, rank);
            cards[i].Tags.Add("suitselection");
        }

        // Todo: particles and sounds and shit
    }

    /// <summary>
    /// Hides Suit Selection cards and plays appropriate effects.
    /// </summary>
    public void Hide()
    {
        Clear();
        // Todo: particles and sounds and shit
    }

    /// <summary>
    /// Cleanup CardEntities.
    /// </summary>
    private void Clear()
    {
        for(int i = 0; i < cards.Length; i++)
            if(cards[i].IsValid())
                cards[i].Delete();
    }

    ~SuitSelectionEntity()
    {
        Clear();
    }
}
