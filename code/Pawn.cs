﻿using Sandbox;

namespace CrazyEights;

partial class Pawn : AnimatedEntity
{
    /// <summary>
    /// Player's current Hand of cards
    /// </summary>
    [Net, Local] public Hand Hand { get; set; }

    /// <summary>
    /// Called when the entity is first created 
    /// </summary>
    public override void Spawn()
    {
        base.Spawn();

        SetModel("models/citizen/citizen.vmdl");

        EnableDrawing = true;
        EnableHideInFirstPerson = true;
        EnableShadowInFirstPerson = true;
    }
}
