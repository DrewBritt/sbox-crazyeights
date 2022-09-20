using Sandbox;

namespace CrazyEights;

public partial class CardEntity : ModelEntity
{
    public Card Card { get; set; }

    private Texture tex;
    private Material mat;

    public CardRank Rank => Card.Rank;
    public CardSuit Suit => Card.Suit;
    public bool IsPlayable() => Card.IsPlayable();

    public override void Spawn()
    {
        base.Spawn();

        SetModel("models/card/card.vmdl");
        Transmit = TransmitType.Always;
    }

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
        if(tex?.IsLoaded ?? false)
            mat.OverrideTexture("Color", tex);
    }
}
