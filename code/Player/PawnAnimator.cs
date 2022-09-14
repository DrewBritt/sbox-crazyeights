﻿using Sandbox;

namespace CrazyEights;

public class PawnAnimator : Sandbox.PawnAnimator
{
    int sitPose = 0;

    public PawnAnimator() : base()
    {
        sitPose = Rand.Int(0, 3);
    }

    public override void Simulate()
    {
        if(Pawn.LifeState != LifeState.Alive)
            return;

        if(Input.Down(InputButton.Use))
            SetAnimParameter("game_action", 2);
        else
            SetAnimParameter("game_action", 0);

        SetAnimParameter("sit_pose", sitPose);

        Vector3 lookPos = Pawn.EyePosition + EyeRotation.Forward * 512;
        SetLookAt("aim_head", lookPos);
        SetAnimParameter("aim_head_weight", 1.0f);
    }
}
