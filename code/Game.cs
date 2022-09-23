using System;
using System.Linq;
using Sandbox;

namespace CrazyEights;

public partial class Game : Sandbox.Game
{
    public static new Game Current => Sandbox.Game.Current as Game;
    public Hud Hud { get; set; }

    public Game() : base()
    {
        if(IsServer) return;

        // Initialize HUD
        Hud = new Hud();

        Log.Info(Global.ServerSteamId);
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
            //return;
        }

        // Otherwise let's pawn for this client to play with.
        var pawn = new Pawn(client)
        {
            CameraMode = new CardsCamera(),
            Animator = new PawnAnimator()
        };
        client.Pawn = pawn;

        // Get un-occupied chair and spawn pawn at said chair.
        var chairs = Entity.All.OfType<PlayerChair>();
        var randomChair = chairs.OrderBy(x => Guid.NewGuid()).Where(x => !x.HasPlayer).FirstOrDefault();
        if(randomChair.IsValid())
        {
            randomChair.SeatPlayer(pawn);
        }
    }

    public override void ClientDisconnect(Client cl, NetworkDisconnectionReason reason)
    {
        // Free player's chair up for use before disconnecting
        var pawn = (cl.Pawn as Pawn);
        pawn.PlayerChair.RemovePlayer();

        base.ClientDisconnect(cl, reason);
    }

    public override void DoPlayerDevCam(Client client)
    {
        Host.AssertServer();

        var camera = client.Components.Get<DevCamera>(true);

        if(camera == null)
        {
            camera = new DevCamera();
            client.Components.Add(camera);
            return;
        }

        camera.Enabled = !camera.Enabled;
    }
}
