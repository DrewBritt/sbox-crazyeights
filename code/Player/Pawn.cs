using Sandbox;

namespace CrazyEights;

public partial class Pawn : AnimatedEntity
{
    /// <summary>
    /// Dresses the pawn
    /// </summary>
    public ClothingContainer Clothing = new();

    /// <summary>
    /// Player's current Hand of cards.
    /// </summary>
    [Net, Local] public Hand Hand { get; set; }

    public Pawn() { }

    public Pawn(Client cl) : this()
    {
        Clothing.LoadFromClient(cl);
        Clothing.DressEntity(this);
    }


    /// <summary>
    /// Called when the entity is first created.
    /// </summary>
    public override void Spawn()
    {
        SetModel("models/citizen/citizen.vmdl");

        EnableDrawing = true;
        EnableHideInFirstPerson = true;
        EnableShadowInFirstPerson = true;

        base.Spawn();
    }
}
