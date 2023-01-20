using System.Linq;
using Sandbox;
using Sandbox.Component;
using CrazyEights.UI;

namespace CrazyEights;

public partial class Player : AnimatedEntity
{
    [BindComponent] public PlayerController Controller { get; }
    [BindComponent] public PlayerAnimator Animator { get; }
    public PlayerCamera PlayerCamera { get; protected set; }
    public WorldNameplate Nameplate;

    public Player() { }
    public Player(IClient cl) : this()
    {
        if(cl.IsBot)
            Clothing.LoadRandomClothes();
        else
            Clothing.LoadFromClient(cl);

        Clothing.DressEntity(this);
    }

    public override void Spawn()
    {
        Tags.Add("player");

        SetModel("models/citizen/crazyeights_citizen.vmdl");

        Components.Create<PlayerAnimator>();
        Components.Create<PlayerController>();

        EnableDrawing = true;
        EnableAllCollisions = true;
        EnableHitboxes = true;
        EnableHideInFirstPerson = false;

        base.Spawn();
    }

    public override void ClientSpawn()
    {
        base.ClientSpawn();

        // Spawn nameplate if not local player's pawn
        if(Game.LocalPawn != this)
            Nameplate = new(this);

        // Initialize client side systems for local pawn
        if(Game.LocalPawn == this)
        {
            PlayerCamera = new PlayerCamera();
        }
    }

    public override void Simulate(IClient cl)
    {
        base.Simulate(cl);

        Animator?.Simulate();
    }

    public override void FrameSimulate(IClient cl)
    {
        base.FrameSimulate(cl);

        PlayerCamera?.Update(this);
        Controller?.FrameSimulate(cl);

        UpdateBodyGroups();
    }

    private void UpdateBodyGroups()
    {
        if(Game.IsClient && IsLocalPawn)
            SetBodyGroup("Head", 1);
        else
            SetBodyGroup("Head", 0);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Nameplate?.Delete();
    }
}
