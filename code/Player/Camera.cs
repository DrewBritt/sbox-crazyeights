using Sandbox;

namespace CrazyEights;

public partial class Camera : CameraMode
{
    private float FOV = 90f;
    private float fac = 1.0f;

    public override void Activated()
    {
        var pawn = Local.Pawn;
        if(pawn == null) return;

        Position = pawn.EyePosition;
        Rotation = Rotation.Random; // Intialize as random because pawn.EyeRotation is (0,0,0) on start, and Lerping 0 with 0 is NaN = weird black screen + 50fps?
        ZNear = .1f;
        ZFar = 5000;
    }

    public override void Update()
    {
        var pawn = Local.Pawn;
        if(pawn == null) return;

        Viewer = pawn;

        float targetFOV = FOV;
        Vector3 targetPosition = pawn.EyePosition;
        Rotation targetRotation = pawn.EyeRotation;

        fac = fac.LerpTo(0.0f, .5f * Time.Delta);

        Position = Position.LerpTo(pawn.EyePosition, 1f - fac);
        Rotation = Rotation.Lerp(Rotation, pawn.EyeRotation, 1f - fac);

        FieldOfView = FieldOfView.LerpTo(targetFOV, 10f * Time.Delta);
    }

    public override void BuildInput(InputBuilder inputBuilder)
    {
        base.BuildInput(inputBuilder);

        if(inputBuilder.StopProcessing)
            return;

        FOV -= inputBuilder.MouseWheel * 10f;
        FOV = FOV.Clamp(10, 90);
    }
}
