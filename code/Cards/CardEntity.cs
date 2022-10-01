﻿using Sandbox;

namespace CrazyEights;

/// <summary>
/// World Model representation of a Card, spawned on Server and replicated on Client.
/// Card value is set on Server, and manually networked to Clients via RPC to avoid networking every single Card.
/// </summary>
public partial class CardEntity : ModelEntity
{
    /// <summary>
    /// Underlying Card value.
    /// </summary>
    public Card Card { get; set; }
    public CardRank Rank => Card.Rank;
    public CardSuit Suit => Card.Suit;
    public bool IsPlayable() => Card.IsPlayable();

    private Texture tex;
    private Material mat;

    public override void Spawn()
    {
        base.Spawn();

        SetModel("models/card/card.vmdl");
        Transmit = TransmitType.Always;
        EnableTraceAndQueries = true;
        SetupPhysicsFromModel(PhysicsMotionType.Keyframed);
        Tags.Add("card");

        // Spawn opaque, lerped to 1 in Event.Frame
        RenderColor = new Color(1f, 1f, 1f, 0f);
    }

    /// <summary>
    /// Set Card value on the Client.
    /// TODO: Look at passing Card, error was throwing earlier regarding not being able to pass Card as a type.
    /// I call bullshit?
    /// </summary>
    /// <param name="rank"></param>
    /// <param name="suit"></param>
    [ClientRpc]
    public void SetCard(CardRank rank, CardSuit suit)
    {
        Card = new Card()
        {
            Rank = rank,
            Suit = suit
        };
        tex = Texture.Load(FileSystem.Mounted, Card.FileName);
        mat = Material.Load("materials/card/card.vmat").CreateCopy();
        SetMaterialOverride(mat, "isTarget");
    }

    [Event.Frame]
    public void OnFrame()
    {
        // Lerp alpha to 1
        if(RenderColor != Color.White)
        {
            var alpha = RenderColor.a.LerpTo(1f, 6f * Time.Delta);
            RenderColor = Color.White.WithAlpha(alpha);
        }

        if(tex?.IsLoaded ?? false)
            mat.OverrideTexture("Color", tex);
    }
}
