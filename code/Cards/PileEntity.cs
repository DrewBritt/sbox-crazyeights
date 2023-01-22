﻿using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace CrazyEights;

/// <summary>
/// Renders and provides collision for the pile of cards the players discard their cards into.
/// </summary>
public partial class PileEntity : DeckEntity
{
    /// <summary>
    /// Displays the last played card/top card on the pile.
    /// </summary>
    [Net] public CardEntity TopCardEntity { get; set; }

    public override void Spawn()
    {
        // Position relative to table.
        var spawn = Entity.All.OfType<DeckSpawn>().Where(d => d.SpawnTarget == SpawnTarget.Pile).FirstOrDefault();
        if(spawn.IsValid())
            Transform = spawn.Transform;

        // Position/Rotate TopCardEntity relative to pile and particle system.
        TopCardEntity = new CardEntity();
        TopCardEntity.Transform = spawn.Transform;
        TopCardEntity.Rotation = TopCardEntity.Rotation.RotateAroundAxis(Vector3.Up, 90);
    }

    protected override void UpdateParticles()
    {
        CardStackParticles.SetPosition(0, Position);

        // Count - 1 as the top card is handled by TopCardEntity
        CardStackParticles.SetPositionComponent(1, 0, Count - 1);
    }

    public override void OnTickServer()
    {
        base.OnTickServer();

        // Position TopCardEntity on top of pile particle.
        TopCardEntity.Position = TopCardEntity.Position.WithZ(Position.z + (0.05f * Count));
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();

        if(Game.IsServer)
            TopCardEntity.Delete();
    }
}