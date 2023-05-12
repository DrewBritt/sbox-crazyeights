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

    /// <summary>
    /// Material cache so we're not creating 80 unique materials when we have a finite number of textures.
    /// </summary>
    //private static Dictionary<Tuple<CardSuit, CardRank>, Material> CardMaterialCache = new();

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

        // Try to pull from cache, otherwise create a new copy and add it.
        //bool success = CardMaterialCache.TryGetValue(new Tuple<CardSuit, CardRank>(suit, rank), out material);
        //if(!success)
        //{

        //CardMaterialCache.Add(new Tuple<CardSuit,CardRank>(suit, rank), material);
        //}

        material = Material.Load("materials/card/card_face.vmat").CreateCopy();
        IsMaterialSet = false;
        SetMaterialOverride(material, "isTarget");
    }

    // WHAT THE FUCK
    // Networking the card texture immediately on spawn
    // SOMETIMES will cause remote clients to not load the texture
    // (even though the values are successfully networked?)
    // so instead, we wait 1/20 of a second before asking the server
    // to send us the card.
    bool IsCardNetworked = false;
    [GameEvent.Tick.Client]
    public void OnTickClient()
    {
        if(IsCardNetworked) return;

        SendCard(this.NetworkIdent);
        IsCardNetworked = true;
    }

    [ConCmd.Server]
    private static void SendCard(int networkIdent)
    {
        CardEntity card = Entity.All.OfType<CardEntity>().Where(c => c.NetworkIdent == networkIdent).FirstOrDefault();
        if(card == null || card.Owner != ConsoleSystem.Caller.Pawn) return;

        card.SetCard(To.Single(card.Owner.Client), card.Suit, card.Rank);
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
