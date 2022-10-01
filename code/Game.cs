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
        Local.Hud = Hud;

        Log.Info(Global.ServerSteamId);
    }

    public override void ClientJoined(Client client)
    {
        base.ClientJoined(client);

        // Find random PlayerChair map entity
        var chairs = Entity.All.OfType<PlayerChair>();
        var randomChair = chairs.OrderBy(x => Guid.NewGuid()).Where(x => !x.HasPlayer).FirstOrDefault();

        // If we find a valid and empty random chair, create a Player pawn and seat said pawn.
        // Otherwise, create a spectator pawn.
        Entity pawn;
        if(randomChair.IsValid())
        {
            pawn = new Pawn(client)
            {
                CameraMode = new Camera(),
                Animator = new PawnAnimator()
            };
            randomChair.SeatPlayer(pawn as Pawn);
        }
        else
        {
            pawn = new SpectatorPawn()
            {
                CameraMode = new DevCamera()
            };
        }
        client.Pawn = pawn;
    }

    public override void ClientDisconnect(Client cl, NetworkDisconnectionReason reason)
    {
        // Free player's chair if they were a pawn
        if(cl.Pawn is Pawn)
        {
            var pawn = cl.Pawn as Pawn;
            pawn.PlayerChair.RemovePlayer();
        }
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
