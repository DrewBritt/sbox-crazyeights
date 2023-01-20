using System.ComponentModel.DataAnnotations;
using Sandbox;
using Editor;

namespace CrazyEights;

[Model(Model = "models/card/card.vmdl")]
[SupportsSolid]
[Library("ce_deckspawn")]
[Display(Name = "Crazy Eights Deck Spawnpoint", Description = "The position where the deck/pile will spawn (set property)")]
[HammerEntity]
public partial class DeckSpawn : Entity
{
    [Property("spawn_target", Title = "Spawn Target")]
    public SpawnTarget SpawnTarget { get; set; } = SpawnTarget.Deck;
}

public enum SpawnTarget
{
    Deck = 0,
    Pile = 1
}
