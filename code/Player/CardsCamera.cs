using Sandbox;

namespace CrazyEights;

public partial class CardsCamera : CameraMode
{
    private float FOV = 90f;
    private float fac = 1.0f;

    public override void Activated()
    {
        var pawn = Local.Pawn;
        if(pawn == null) return;

        Position = pawn.EyePosition;
        Rotation = pawn.EyeRotation;
    }

    public override void Update()
    {
        var pawn = Local.Pawn;
        if(pawn == null) return;

        ZNear = 1;
        ZFar = 5000;

        float targetFOV;
        Vector3 targetPosition;
        Rotation targetRotation;

        targetPosition = pawn.EyePosition;
        targetRotation = pawn.EyeRotation;
        targetFOV = FOV;

        fac = fac.LerpTo(0.0f, .5f * Time.Delta);

        Viewer = pawn;

        Position = Position.LerpTo(targetPosition, 10f * Time.Delta);
        Rotation = Rotation.Lerp(Rotation, targetRotation, 10f * Time.Delta);

        Position = Position.LerpTo(pawn.EyePosition, 1.0f - fac);
        Rotation = Rotation.Lerp(Rotation, pawn.EyeRotation, 1.0f - fac);

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
