using System.Linq;
using Sandbox;

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
    private Texture texture;
    private Material material;

    public bool IsPlayable()
    {
        if(Card.Suit == CardSuit.Wild) return true;

        var lastPlayed = GameManager.Current.DiscardPileEntity.TopCardEntity;
        if(Card.Suit == lastPlayed.Suit || Card.Rank == lastPlayed.Rank)
            return true;
        return false;
    }

    public override void Spawn()
    {
        base.Spawn();

        SetModel("models/card/cecard.vmdl");
        Transmit = TransmitType.Always;
        EnableTraceAndQueries = true;
        SetupPhysicsFromModel(PhysicsMotionType.Keyframed);
        Tags.Add("card");

        // Spawn opaque, lerped to 1 in Event.Frame
        RenderColor = new Color(1f, 1f, 1f, 0f);
    }

    /// <summary>
    /// Set Card value on a client and display correct texture.
    /// </summary>
    /// <param name="suit"></param>
    /// <param name="rank"></param>
    [ClientRpc]
    public void SetCard(CardSuit suit, CardRank rank)
    {
        Card = new Card()
        {
            Suit = suit,
            Rank = rank
        };

        texture = Texture.Load(FileSystem.Mounted, Card.FileName);
        material = Material.Load("materials/card/cecard_face.vmat").CreateCopy();
        IsMaterialSet = false;
        SetMaterialOverride(material, "isTarget");
    }

    public bool IsMaterialSet = false;
    [GameEvent.Client.Frame]
    public void OnFrame()
    {
        // Lerp alpha to 1
        if(RenderColor != Color.White)
        {
            var alpha = RenderColor.a.LerpTo(1f, 6f * Time.Delta);
            RenderColor = Color.White.WithAlpha(alpha);
        }

        if(texture != null && texture.IsLoaded && !IsMaterialSet)
        {
            IsMaterialSet = true;
            material.Set("Color", texture);
        }
    }
}
