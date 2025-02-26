using Godot;
using System;

/* 	This is basically anything where player loses control and plays an animation,
	For example, it can be used for pulleys, Speed Highway rockets or vehicles.*/ 
public partial class ActionAutoGimmick : Node
{
	[Export] public PlayerController Player;
    private Node3D copyPos = null;

    public override void _PhysicsProcess(double delta)
    {
		if (copyPos == null) return;
		Player.GlobalTransform = copyPos.GlobalTransform;
    }

    public void StartAutoGimmick(string AnimationName, Node3D copyPos = null)
	{
        if (!(Player.PlayerSkinController.SkipRankWaitAnimation && AnimationName == "Rank Wait"))
        {
            Player.PlayerSkinController.TravelAnimation(AnimationName);
        }
        Player.Enabled = false;
		Player.LinearVelocity = Vector3.Zero;
		Player.PlayerInput.LockedInput = true;
        Player.SetPhysicsProcess(false);
		this.copyPos = copyPos;


        var homing = Player.GetNodeOrNull<ActionHoming>("./PlayerControl/Actions/ActionHoming");
		if (homing != null)
		{
			homing.IsHoming = false;
		}
        var rolling = Player.GetNodeOrNull<ActionRoll>("./PlayerControl/Actions/ActionRoll");
        if (rolling != null)
        {
            rolling.isRolling = false;
        }
    }

	public void EndAutoGimmick(string AnimationName)
    {
        Player.PlayerSkinController.TravelAnimation(AnimationName);
        Player.Enabled = true;
		copyPos = null;
        Player.PlayerInput.LockedInput = false;
        Player.SetPhysicsProcess(true);
    }
}
