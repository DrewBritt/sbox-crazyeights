using System;
using System.Linq;
using Sandbox;

namespace CrazyEights;

public partial class Game : Sandbox.Game
{
    public static new Game Current => Sandbox.Game.Current as Game;

    public Game() : base()
    {
    }

    /// <summary>
    /// A client has joined the server. Make them a pawn to play with
    /// </summary>
    public override void ClientJoined(Client client)
    {
        base.ClientJoined(client);

        // Create a pawn for this client to play with
        var pawn = new Pawn();
        client.Pawn = pawn;

        // Get all of the spawnpoints
        var spawnpoints = Entity.All.OfType<SpawnPoint>();

        // chose a random one
        var randomSpawnPoint = spawnpoints.OrderBy(x => Guid.NewGuid()).FirstOrDefault();

        // if it exists, place the pawn there
        if(randomSpawnPoint != null)
        {
            var tx = randomSpawnPoint.Transform;
            tx.Position = tx.Position + Vector3.Up * 50.0f; // raise it up
            pawn.Transform = tx;
        }
    }
}
