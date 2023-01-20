using CrazyEights.UI;
using Sandbox;

namespace CrazyEights;

public partial class Player : AnimatedEntity
{
    [BindComponent] public PlayerController Controller { get; }
    [BindComponent] public PlayerAnimator Animator { get; }
    public PlayerCamera PlayerCamera { get; protected set; }
    public WorldNameplate Nameplate { get; protected set; }

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

        Components.Create<PlayerController>();
        Components.Create<PlayerAnimator>();

        EnableDrawing = true;
        EnableAllCollisions = true;
        EnableHitboxes = true;
        EnableHideInFirstPerson = false;

        base.Spawn();
    }

    public override void ClientSpawn()
    {
        base.ClientSpawn();

        LookInput = Rotation.Angles();

        // Initialize client side systems for player (camera for local, nameplate for every other)        
        if(Game.LocalPawn == this)
        {
            PlayerCamera = new PlayerCamera();
        }
        else
        {
            Nameplate = new(this);
        }
    }

    public override void Simulate(IClient cl)
    {
        base.Simulate(cl);

        Controller?.Simulate(cl);
        Animator?.Simulate();
    }

    public override void FrameSimulate(IClient cl)
    {
        base.FrameSimulate(cl);

        Controller?.FrameSimulate(cl);
        PlayerCamera?.Update(this);

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
