﻿using Sandbox;

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
            cards[i].Parent = this;
            cards[i].LocalPosition = GetInitPos(i);
            cards[i].LocalRotation = GetInitRot(i);
            cards[i].SetCard((CardSuit)i, rank);
            cards[i].Tags.Add("suitselection");
        }

        // Todo: particles and sounds and shit
    }

    /// <summary>
    /// Gets appropriate initial LocalPosition for SuitSelection card.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private Vector3 GetInitPos(int index)
    {
        // First 2 cards are on left of origin, last 2 are on right
        int sign = 1;
        if(index < 2)
            sign *= -1;

        if(index == 0 || index == 3)
            return new Vector3(5, 20 * sign, 0);
        else
            return new Vector3(0, 10 * sign, 0);
    }

    /// <summary>
    /// Gets appropriate initial LocalRotation for SuitSelection card.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private Rotation GetInitRot(int index)
    {
        // First 2 cards are on left of origin, last 2 are on right
        int sign = 1;
        if(index < 2)
            sign *= -1;

        Rotation rot = Rotation.FromPitch(90f);
        float rotAmount;
        if(index == 0 || index == 3)
            rotAmount = 30f;
        else
            rotAmount = 15f;

        return rot.RotateAroundAxis(Vector3.Forward, rotAmount * sign);
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
                cards[i].DeleteAsync(.1f);
    }

    ~SuitSelectionEntity()
    {
        Clear();
    }
}
