using Godot;
using System;

public partial class SplineMovement : Node
{
	[Export] public PlayerController Player;
    [Export] public ActionGrind ActionGrind;
    [Export] public ActionHoming ActionHoming;
    [Export] public bool ReleaseOnJump;
    [Export] public bool ReleaseOnEnds = false;
    [Export] public float ReleaseOnDistanceFromPath;	// Values <=0 mean that it's unused
    [Export] public Path3D Path;

    public override void _Ready()
    {
		if (ActionGrind == null)
		{
			ActionGrind = this.GetNodeOrNull<ActionGrind>("../Actions/ActionGrind");
        }
        if (ActionHoming == null)
        {
            ActionHoming = this.GetNodeOrNull<ActionHoming>("../Actions/ActionHoming");
        }
    }

    public override void _PhysicsProcess(double delta)
	{
		if (Path == null) return;
		if (ReleaseOnJump && !Player.Grounded)
		{
			Path = null;
			Player.PlayerInput.TwodimensionalMode = false;

			return;
		}

		if (ActionGrind.IsGrinding() || ActionHoming.IsHoming) return;

		var nearestOffset = Path.Curve.GetClosestOffset(Player.GlobalPosition - Path.GlobalPosition);

		if (ReleaseOnEnds && (nearestOffset <= 0 || nearestOffset >= 1))
		{
            Path = null;
            Player.PlayerInput.TwodimensionalMode = false;

            return;
        }

		var sample = Path.Curve.SampleBakedWithRotation(nearestOffset);

        if (ReleaseOnDistanceFromPath > 0)
        {
			var distance = Path.ToGlobal(sample.Origin).DistanceTo(Player.GlobalPosition);

            if (distance > ReleaseOnDistanceFromPath)
			{
				Path = null;
				Player.PlayerInput.TwodimensionalMode = false;

				return;
			}

        }

        var depthOffset = Vector3Utils.Project((sample.Origin + Path.GlobalPosition) - Player.GlobalPosition, sample.Basis.X);

		var originalVelocity = Player.VelocityXZ.Length();

		Player.VelocityXZ = Vector3Utils.ProjectOnPlane(Player.VelocityXZ, sample.Basis.X.Normalized());
        Player.VelocityXZ += depthOffset * originalVelocity / 10f;
        Player.VelocityXZ = Vector3Utils.ProjectOnPlane(Player.VelocityXZ, Player.GroundNormal).Normalized() * originalVelocity;

        Player.PlayerInput.Plane2DControlsX = -sample.Basis.Z;

		if (Player.PlayerInput.LockedInput)
		{
			Player.PlayerInput.LeftInput3D = Player.PlayerInput.LeftInput3D.ProjectOnPlane(sample.Basis.X).Normalized() * Player.PlayerInput.LeftInput3D.Length();
		}
	}
}
