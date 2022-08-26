using System;
using System.Linq;
using Sandbox;

namespace CrazyEights;

public partial class Game : Sandbox.Game
{
    public static new Game Current => Sandbox.Game.Current as Game;

    public Game() : base()
    {
        if(IsClient) return;

        // Initialize HUD
        _ = new Hud();
    }

    /// <summary>
    /// A client has joined the server. Make them a pawn to play with.
    /// </summary>
    public override void ClientJoined(Client client)
    {
        base.ClientJoined(client);

        // Spawn player as spectator if game is already in session
        if(Current.CurrentState is PlayingState)
        {
            // Spawn spectator pawn
            return;
        }

        // Otherwise let's pawn for this client to play with.
        var pawn = new Pawn(client);
        client.Pawn = pawn;

        // Get un-occupied chair and spawn pawn at said chair.
        var chairs = Entity.All.OfType<PlayerChair>();
        var randomChair = chairs.OrderBy(x => Guid.NewGuid()).Where(x => !x.HasPlayer).FirstOrDefault();
        if(randomChair.IsValid())
        {
            randomChair.SeatPlayer(pawn);
        }
    }

    public override CameraSetup BuildCamera(CameraSetup camSetup)
    {
        var pawnRot = Local.Pawn.Rotation;

        camSetup.Rotation = Rotation.From(new Angles(90f, pawnRot.Angles().yaw, 0f));
        camSetup.Position = new Vector3(0f, 0f, 400f);
        camSetup.FieldOfView = 75;
        camSetup.Ortho = false;
        camSetup.Viewer = null;

        return camSetup;
    }

    public override void DoPlayerDevCam(Client client)
    {
        base.DoPlayerDevCam(client);
    }
}
