using Godot;
using System;

public partial class Laser : RayCast3D
{
	[Export] public MeshInstance3D[] BeamMeshes = new MeshInstance3D[0];
	[Export] public Hitbox Hitbox;
	[Export] public CollisionObject3D[] CollissionExceptions = new CollisionObject3D[0];
	[Export] public CollisionShape3D HitboxShape;
	[Export] public Vector3 UvScroll;
	[Export] public GpuParticles3D EndParticles;
	[Export] public bool LaserActive = true;

	public override void _Ready()
	{
		foreach (var ex in CollissionExceptions)
		{
			this.AddExceptionRid(ex.GetRid());
		}

		
		SetLaserActive(LaserActive);
	}

	public void SetLaserActive(bool active)
	{
		LaserActive = active;
		foreach (var BeamMesh in BeamMeshes)
		{
			if (!LaserActive)
			{
				BeamMesh.Visible = false;
			}
			else
			{
				BeamMesh.Visible = true;
			}
		}
		if (!LaserActive)
		{
			Hitbox.ProcessMode = ProcessModeEnum.Disabled;
		}
		else
		{
			Hitbox.ProcessMode = ProcessModeEnum.Inherit;
		}
	}

	public override void _Process(double delta)
	{
		if (this.IsColliding())
		{
			var hitPoint = this.GetCollisionPoint();
			var localHitPoint = this.ToLocal(hitPoint);
			var hitNormal = this.GetCollisionNormal();

			foreach (var BeamMesh in BeamMeshes)
			{
				BeamMesh.Mesh.Set("height", Mathf.Abs(localHitPoint.Y));
				BeamMesh.Position = localHitPoint / 2;
				BeamMesh.GetActiveMaterial(0).Set("uv1_scale", new Vector3(0, 1f / Mathf.Abs(localHitPoint.Y), 0));
				var currentOffset = BeamMesh.GetActiveMaterial(0).Get("uv1_offset").AsVector3();
				BeamMesh.GetActiveMaterial(0).Set("uv1_offset", currentOffset + UvScroll * (float)delta);
			}

			HitboxShape.Shape.Set("height", Mathf.Abs(localHitPoint.Y));
			HitboxShape.Position = localHitPoint / 2;

			EndParticles.GlobalPosition = hitPoint;
		}
	}
}
