using System;
using System.Linq;
using CrazyEights.UI;
using Sandbox;

namespace CrazyEights;

public partial class GameManager : Sandbox.GameManager
{
    public static new GameManager Current => Sandbox.GameManager.Current as GameManager;
    public Hud Hud { get; set; }

    public GameManager() : base()
    {
        if(Game.IsServer) return;

        Hud = new Hud();
        Log.Info(Game.ServerSteamId);
    }

    public override void ClientJoined(IClient client)
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
            pawn = new Player(client);
            randomChair.SeatPlayer(pawn as Player);

            client.Pawn = pawn;
        }
        else
        {
            /*pawn = new SpectatorPawn()
            {
                CameraMode = new DevCamera()
            };*/
        }

    }

    public override void ClientDisconnect(IClient cl, NetworkDisconnectionReason reason)
    {
        // If client was a Player, free their chair.
        if(cl.Pawn is Player player)
        {
            var chair = Entity.All.OfType<PlayerChair>().Where(x => x.HasPlayer && x.Player == player).FirstOrDefault();
            chair.RemovePlayer();
        }

        base.ClientDisconnect(cl, reason);
    }

    public override void DoPlayerDevCam(IClient client)
    {
        Game.AssertServer();

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
