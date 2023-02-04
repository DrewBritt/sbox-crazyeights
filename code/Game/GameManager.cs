using System;
using System.Collections.Generic;
using System.Linq;
using CrazyEights.UI;
using Sandbox;

namespace CrazyEights;

public partial class GameManager : Sandbox.GameManager
{
    public static new GameManager Current => Sandbox.GameManager.Current as GameManager;
    public Hud Hud { get; set; }
    [Net] public IList<IClient> SpectatorSwapQueue { get; set; }

    public GameManager() : base()
    {
        if(Game.IsServer) return;

        Hud = new Hud();
        Log.Info(Game.ServerSteamId);
    }

    public override void ClientJoined(IClient client)
    {
        base.ClientJoined(client);

        // Find random PlayerChair map entity.
        var chairs = Entity.All.OfType<PlayerChair>();
        var randomChair = chairs.OrderBy(c => Guid.NewGuid()).Where(c => !c.HasPlayer).FirstOrDefault();

        // If we find a valid and empty random chair, create a Player pawn and seat said pawn.
        // Otherwise, create a spectator pawn.
        Entity pawn;
        if(randomChair.IsValid())
        {
            pawn = new Player(client);
            randomChair.SeatPlayer(pawn as Player);
        }
        else
        {
            pawn = new Spectator();
            pawn.Position = chairs.First()?.Position ?? Vector3.Zero;
        }

        client.Pawn = pawn;
    }

    public override void ClientDisconnect(IClient cl, NetworkDisconnectionReason reason)
    {
        if(cl.Pawn is Player)
        {
            TransferOrCleanupPlayer(cl);
        }
        else if(cl.Pawn is Spectator)
        {
            Current.SpectatorSwapQueue.Remove(cl);
        }

        base.ClientDisconnect(cl, reason);
    }

    public override void DoPlayerDevCam(IClient client)
    {
        Game.AssertServer();

        // Spectators should always be in devcam, which is initialized in their Spawn().
        if(client.Pawn is Spectator) return;

        var camera = client.Components.Get<DevCamera>(true);
        if(camera == null)
        {
            camera = new DevCamera();
            client.Components.Add(camera);
            return;
        }

        camera.Enabled = !camera.Enabled;
    }

    /// <summary>
    /// playerClient wishes to be a disconnect/swap to spectators.
    /// This function either gives this pawn to the first spectator in the swap-queue,
    /// or cleans up the pawn and game data.
    /// </summary>
    /// <param name="playerClient">Client wishing to give their Player pawn up.</param>
    public void TransferOrCleanupPlayer(IClient playerClient)
    {
        // Check swap queue for any players to take this one's place.
        if(Current.SpectatorSwapQueue.Count > 0)
        {
            var spectatorClient = Current.SpectatorSwapQueue.First();
            Current.SpectatorSwapQueue.Remove(spectatorClient);
            spectatorClient.Pawn.Delete();

            var playerPawn = playerClient.Pawn as Player;
            playerClient.Pawn = null;
            spectatorClient.Pawn = playerPawn;

            // Network player's card info + tell player to create local ScreenEffects/SuitSelectionEntity (temporary workaround for OnClientActive)
            playerPawn.HandDisplay?.ClearCards();
            playerPawn.HandDisplay?.AddCards(playerPawn.Hand().Cards);
            playerPawn.PlayerCamera.CreateScreenEffects(To.Single(spectatorClient));
            playerPawn.Controller.CreateSuitSelection(To.Single(spectatorClient));
        }
        else // Otherwise, clean their mess up and adjust state as needed.
        {
            var player = playerClient.Pawn as Player;
            if(Current.CurrentState is PlayingState state && player.Hand() != null)
            {
                state.PlayingDeck.AddCards(player.Hand().Cards);

                // Advance game state if swapping player is current player.
                if(Current.CurrentPlayer == player)
                {
                    int nextIndex = 0;
                    if(state.Hands.Last().Item1 == player)
                    {
                        // If player is last, we must calculate index before removing them to ensure index wraps clockwise.
                        nextIndex = Current.GetNextPlayerIndex(Current.CurrentPlayerIndex);
                        state.Hands.Remove(state.Hands.Where(h => h.Item1 == player).First());
                    }
                    else
                    {
                        // Otherwise, remove player from state THEN determine new index.
                        state.Hands.Remove(state.Hands.Where(h => h.Item1 == player).First());

                        // Clockwise -> index stays in place.
                        // Counter-Clockwise -> index will go left one, or wrap to (new) end.
                        if(Current.DirectionIsClockwise)
                            nextIndex = Current.CurrentPlayerIndex;
                        else
                            nextIndex = Current.GetNextPlayerIndex(Current.CurrentPlayerIndex);
                    }

                    state.SetState(new PlayingState(state.PlayingDeck, state.DiscardPile, state.Hands, nextIndex));
                }
                else
                {
                    // Ensure we remove hand from state if player isn't CurrentPlayer.
                    state.Hands.Remove(state.Hands.Where(h => h.Item1 == player).First());
                }
            }

            var chair = Entity.All.OfType<PlayerChair>().Where(c => c.Player == player).FirstOrDefault();
            chair.RemovePlayer();
            player.Delete();
        }
    }
}
