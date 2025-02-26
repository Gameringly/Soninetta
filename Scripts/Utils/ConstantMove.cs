using Godot;
using System;

public partial class ConstantMove : Node3D
{
	[Export] public Vector3 LinearVelocity;

	public override void _PhysicsProcess(double delta)
	{
		this.Translate(this.LinearVelocity * (float)delta);
	}
}
