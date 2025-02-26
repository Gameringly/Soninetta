using Godot;
using System;

public partial class Spline2DTrigger : Area3D
{
	[Export] public Path3D Path;
	[Export] public bool ReleaseOnJump = false;
	[Export] public bool TwoDimensionalMode = false;
	[Export] public bool ReleaseOnExit; // Note: Keep in mind it doesn't work with ConcavePolygonShape.
	[Export] public float ReleaseOnDistanceFromPath = -1; // This is a backup for ConcavePolygonShape.
	[Export] public bool ReleaseOnEnds = false;

	public override void _Ready()
	{
		if (GetSignalConnectionList("body_entered").Count == 0)
		{
			this.BodyEntered += this.OnBodyEnter;
		}

		if (GetSignalConnectionList("body_exited").Count == 0)
		{
			this.BodyExited += this.OnBodyExit;
		}
	}


	public void OnBodyEnter(Node3D other)
	{

		var player = other.GetNodeOrNull<PlayerController>(".");
		if (player != null)
		{
			var splineMovement = other.GetNodeOrNull<SplineMovement>("./PlayerControl/SplineMovement");
			if (splineMovement != null)
			{
				splineMovement.Path = this.Path;
				splineMovement.ReleaseOnJump = this.ReleaseOnJump;
				splineMovement.ReleaseOnEnds = this.ReleaseOnEnds;
				splineMovement.ReleaseOnDistanceFromPath = this.ReleaseOnDistanceFromPath;

				player.PlayerInput.TwodimensionalMode = TwoDimensionalMode;
			}
		}

		for (int i = 0; i < other.GetChildCount(); i++)
		{
			var npsm = other.GetChild(i).GetNodeOrNull<NonPlayerSplineMovement>(".");
			if (npsm != null)
			{
				npsm.Path = this.Path;
			}
		}
		

	}

	public void OnBodyExit(Node3D other)
	{
		if (!ReleaseOnExit) return;

		var player = other.GetNodeOrNull<PlayerController>(".");
		if (player != null)
		{
			var splineMovement = other.GetNodeOrNull<SplineMovement>("./PlayerControl/SplineMovement");
			if (splineMovement != null)
			{
				splineMovement.Path = null;
				splineMovement.ReleaseOnJump = false;
				player.PlayerInput.TwodimensionalMode = false;
			}
		}

		for (int i = 0; i < other.GetChildCount(); i++)
		{
			var npsm = other.GetChild(i).GetNodeOrNull<NonPlayerSplineMovement>(".");
			if (npsm != null && npsm.Path == this.Path)
			{
				npsm.Path = null;
			}
		}

	}
}
