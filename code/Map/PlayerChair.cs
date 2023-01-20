﻿using System.ComponentModel.DataAnnotations;
using Editor;
using Sandbox;

namespace CrazyEights;

[Model]
[SupportsSolid]
[Library("ce_chair")]
[Display(Name = "Crazy Eights Player Chair", Description = "A chair in which a player's pawn will spawn and sit in.")]
[HammerEntity]
public partial class PlayerChair : ModelEntity
{
    private Player player;
    public bool HasPlayer => player != null;

    public override void Spawn()
    {
        base.Spawn();

        SetupPhysicsFromModel(PhysicsMotionType.Static);
        EnableAllCollisions = false;
        EnableDrawing = false;
    }

    /// <summary>
    /// Seats player in the chair and makes it visible
    /// </summary>
    /// <param name="player"></param>
    public void SeatPlayer(Player player)
    {
        EnableDrawing = true;

        this.player = player;
        player.Transform = this.Transform;
        player.Position += (this.Transform.Rotation.Forward * 2f) + (Vector3.Up * 4f);
        player.PlayerChair = this;
    }

    /// <summary>
    /// Frees chair from use and makes it invisible
    /// </summary>
    public void RemovePlayer()
    {
        if(!HasPlayer) return;

        EnableDrawing = false;
        player.PlayerChair = null;
    }
}
