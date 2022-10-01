using Sandbox;

namespace CrazyEights;

public class PawnAnimator : Sandbox.PawnAnimator
{
    int sitPose = 0;
    Pawn Player => Pawn as Pawn;

    public PawnAnimator() : base()
    {
        //sitPose = Rand.Int(0, 3);
    }

    private TimeSince TimeSinceGameAction;
    public override void Simulate()
    {
        if(Pawn.LifeState != LifeState.Alive)
            return;

        SetAnimParameter("sit_pose", sitPose);
        SetAnimParameter("hasCards", Player.Hand?.Cards.Count > 0);

        // Blend between card hold poses
        int cards = 0;
        if(Player.Hand != null)
            cards = Player.Hand.Cards.Count;
        SetAnimParameter("hold_blend_weight", cards / 14f);

        // Play Interact animation
        if(TimeSinceGameAction < 1)
            SetAnimParameter("game_action", 1);
        else
            SetAnimParameter("game_action", 0);

        Vector3 lookPos = Pawn.EyePosition + EyeRotation.Forward * 512;
        SetLookAt("aim_head", lookPos);
        SetAnimParameter("aim_head_weight", 1.0f);
    }

    /// <summary>
    /// Resets TimeSince GameAction to let Simulate know to play an animation.
    /// Called from ClientRPC called in PlayCard/DrawCard commands (as they're called from player UI as well as FrameSimulate).
    /// </summary>
    public void DidAction()
    {
        TimeSinceGameAction = 0;
    }
}
