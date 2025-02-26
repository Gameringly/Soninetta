using Godot;
using System;

public partial class CameraTrigger : Area3D
{
	[Export] PlayerCamera.CameraMode Mode = PlayerCamera.CameraMode.Free;
	[Export] Quaternion ConstantCameraRotation;
	[Export] Path3D CameraCurve = null;
	[Export] public Node3D CameraPoint;
	[Export] bool DisableRaycast = false;
	[Export] float CameraDistance = -1; // Negative values mean don't change
	[Export] public bool CardinalDirectionBias = false;
	[Export] public bool FreeOnExit = false; // Keep in mind it doesn't work with ConcavePolygonShape.
	[Export] public float EnterLerpTime = 0f;
	[Export] public float ExitLerpTime = 0f;	// Only applicable for FreeOnExit

	public override void _Ready()
	{
		if (GetSignalConnectionList("body_entered").Count == 0)
		{
			this.BodyEntered += this.OnBodyEnter;
		}

		if (GetSignalConnectionList("body_exited").Count == 0)
		{
			this.BodyExited += this.OnBodyExited;
		}

		const int PLAYER_LAYER = 2;
		this.SetCollisionMaskValue(PLAYER_LAYER, true);
	}

	public void OnBodyEnter(Node3D other)
	{
		var player = other.GetNodeOrNull<PlayerController>(".");
		if (player != null)
		{
			if (PlayerCamera.Instances.ContainsKey(player.PlayerID))
			{
				var camera = PlayerCamera.Instances[player.PlayerID];
				SetCamera(camera);

				if (EnterLerpTime > 0)
				{
					camera.StartLerp(EnterLerpTime);
				}
			}
		}
	}

	public void OnBodyExited(Node3D other)
	{
		if (!FreeOnExit) return;
		var player = other.GetNodeOrNull<PlayerController>(".");
		if (player != null)
		{
			if (PlayerCamera.Instances.ContainsKey(player.PlayerID))
			{
				var camera = PlayerCamera.Instances[player.PlayerID];

				camera.Mode = PlayerCamera.CameraMode.Free;
				camera.Spline = null;
				camera.Point = null;

				if (ExitLerpTime > 0)
				{
					camera.StartLerp(ExitLerpTime);
				}
			}
		}
	}

	void SetCamera(PlayerCamera cam)
	{
		cam.Mode = this.Mode; 
		cam.Spline = null;
		cam.Point = null;
		if (cam.Mode == PlayerCamera.CameraMode.Constant)
		{
			cam.ConstantCameraDirection = this.ConstantCameraRotation.Normalized();
		}
		else if (cam.Mode == PlayerCamera.CameraMode.Spline3D || cam.Mode == PlayerCamera.CameraMode.Spline3DStrict || cam.Mode == PlayerCamera.CameraMode.Spline2D)
		{
			cam.Spline = this.CameraCurve;
		} else  if (cam.Mode == PlayerCamera.CameraMode.Point || cam.Mode == PlayerCamera.CameraMode.LookFromPoint)
		{
			cam.Point = this.CameraPoint;
		}
		if (this.CameraDistance >= 0)
		{
			cam.MaxCameraDistance = this.CameraDistance;
		}
		cam.DisableRaycast = this.DisableRaycast;
		cam.CardinalDirectionBias = this.CardinalDirectionBias;

	}
}
