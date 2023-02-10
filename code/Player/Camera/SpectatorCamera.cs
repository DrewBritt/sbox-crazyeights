using Sandbox;

namespace CrazyEights;

public partial class SpectatorCamera : EntityComponent<Spectator>, ISingletonComponent
{
    private float FOV;

    protected override void OnActivate()
    {
        if(Game.IsServer) return;

        Camera.ZNear = .1f;
        Camera.ZFar = 5000;

        FOV = Screen.CreateVerticalFieldOfView(Game.Preferences.FieldOfView);
    }

    public virtual void BuildInput()
    {
        if(Input.StopProcessing)
            return;

        FOV -= Input.MouseWheel * 10f;
        FOV = FOV.Clamp(45, 130);
    }

    public virtual void FrameSimulate(IClient cl)
    {
        var player = Entity;

        Camera.Position = player.Position;
        Camera.Rotation = player.Controller.EyeRotation;
        Camera.FirstPersonViewer = player;

        float targetFOV = FOV;
        Camera.Main.FieldOfView = Camera.Main.FieldOfView.LerpTo(targetFOV, 10f * Time.Delta);
    }
}
