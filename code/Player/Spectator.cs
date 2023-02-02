using Sandbox;

namespace CrazyEights;

public partial class Spectator : ModelEntity
{
    [BindComponent] public SpectatorController Controller { get; }
    [BindComponent] public SpectatorCamera Camera { get; }

    public override void Spawn()
    {
        Components.Create<SpectatorController>();
        Components.Create<SpectatorCamera>();
    }

    public override void BuildInput()
    {
        Controller?.BuildInput();
        Camera?.BuildInput();
    }

    public override void Simulate(IClient cl)
    {
        Controller?.Simulate(cl);
    }

    public override void FrameSimulate(IClient cl)
    {
        Controller?.FrameSimulate(cl);
        Camera?.FrameSimulate(cl);
    }
}
