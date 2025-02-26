using Godot;
using System;
using System.Linq;

public partial class RemoteVictoryEnable : Node
{
	[Export] public GoalRing GoalRing;

	public override void _PhysicsProcess(double delta)
	{
		GoalRing.Visible = false;
		GoalRing.ProcessMode = ProcessModeEnum.Inherit;
		GoalRing.OnCollissionEnter(PlayerController.Instances.First());

		this.QueueFree();
	}
}
