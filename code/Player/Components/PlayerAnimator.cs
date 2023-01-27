using Sandbox;

namespace CrazyEights;

public class PlayerAnimator : EntityComponent<Player>, ISingletonComponent
{
    private TimeSince TimeSinceGameAction;
    public virtual void Simulate(IClient cl)
    {
        var player = Entity;

        if(player.LifeState != LifeState.Alive)
            return;

        player.SetAnimParameter("sit_pose", 0);
        player.SetAnimParameter("hasCards", player.HandDisplay?.Cards.Count > 0);

        // Blend between card hold poses
        int cards = 0;
        if(player.HandDisplay != null)
            cards = player.HandDisplay.Cards.Count;
        player.SetAnimParameter("hold_blend_weight", cards / 14f);

        // Play Interact animation
        if(TimeSinceGameAction < 1)
            player.SetAnimParameter("game_action", 1);
        else
            player.SetAnimParameter("game_action", 0);

        Vector3 lookPos = player.AimRay.Position + player.AimRay.Forward * 512;
        player.SetAnimLookAt("aim_head", player.Controller.EyePosition, lookPos);
        player.SetAnimParameter("aim_head_weight", 1.0f);
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
