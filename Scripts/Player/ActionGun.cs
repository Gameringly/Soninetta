using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ActionGun : Node
{
    [Export] public float Range = 200f;
    [Export] public double TargettingCooldown = 0.1f;
    private double targettingTimer = 0;
    [Export] public double FireCooldown = 0.1f;
    private double fireTimer = 0;
    [Export] public AudioStreamPlayer3D AudioPlayer;
    [Export] public AudioStream TargettingBeepSound;
    [Export] public AudioStream GunFireSound;

    [Export] public Vector3 MuzzleOffset = new Vector3(0, 1f, -2);
    //[Export] public PackedScene ExplosionPrefab;
    [Export] public PackedScene BulletPrefab;
    [Export] public PackedScene ComboTargetPrefab;

    public PlayerController Player;
    private ActionRoll ActionRoll;
    public List<HomingTarget> CurrentTargets = new List<HomingTarget>();
    private List<ComboTarget> CurrentComboTargets = new List<ComboTarget>();
    public GunStateEnum GunState;

    public enum GunStateEnum
    {
        IDLE,
        TARGETING,
        FIRING
    }

    public override void _Ready()
    {
        Player = GetNodeOrNull<PlayerController>("../../..");
        ActionRoll = this.GetNodeOrNull<ActionRoll>("../ActionRoll");

    }

    public override void _Input(InputEvent @event)
    {
        const string INPUT_ACTION = "actionhoming";
        if (Player.PlayerInput.IsActionPressed(@event, INPUT_ACTION) && GunState == GunStateEnum.IDLE)
        {
            GunState = GunStateEnum.TARGETING;
            targettingTimer = 0;
            
        }
        if (Player.PlayerInput.IsActionReleased(@event, INPUT_ACTION) && GunState == GunStateEnum.TARGETING)
        {
            if (CurrentTargets.Count == 0)
            {
                GunState = GunStateEnum.IDLE;
            }
            else
            {
                GunState = GunStateEnum.FIRING;
                fireTimer = 0;
                if (ActionRoll != null)
                {
                    ActionRoll.isRolling = false;
                }
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        switch(GunState)
        {
            case GunStateEnum.IDLE:
                return;
            case GunStateEnum.TARGETING:
                TargettingProcess(delta);
                break;
            case GunStateEnum.FIRING:
                FiringProcess(delta);
                break;
        }
    }

    private void TargettingProcess(double delta)
    {
        targettingTimer += delta;
        if (targettingTimer < TargettingCooldown) return;

        var sortedPotentialTargets = HomingTarget.Instances.OrderBy(c => c.GlobalPosition.DistanceSquaredTo(Player.GlobalPosition));
        foreach (var potentialTarget in sortedPotentialTargets)
        {
            if (potentialTarget.GlobalPosition.DistanceTo(Player.GlobalPosition) > Range) continue;
            if (CurrentTargets.Contains(potentialTarget)) continue;
            if (!potentialTarget.Visible) continue;
            if (!potentialTarget.TargetableByGuns) continue;
            if (!PlayerCamera.Instances[Player.PlayerID].IsPositionInFrustum(potentialTarget.GlobalPosition)) continue;

            CurrentTargets.Add(potentialTarget);
            targettingTimer = 0;
            AudioPlayer.Stream = TargettingBeepSound;
            AudioPlayer.Play(0);

            var comboTarget = ComboTargetPrefab.Instantiate<ComboTarget>();
            GetTree().Root.AddChild(comboTarget);
            comboTarget.SetComboCount(CurrentTargets.Count);
            comboTarget.GlobalPosition = potentialTarget.GlobalPosition;
            comboTarget.CopyPositionFrom = potentialTarget;
            comboTarget.Camera = PlayerCamera.Instances[Player.PlayerID];
            CurrentComboTargets.Add(comboTarget);

            break;
        }
    }

    private void FiringProcess(double delta)
    {
        
        if (CurrentTargets.Count == 0)
        {
            GunState = GunStateEnum.IDLE;
            return;
        }
        fireTimer += delta;
        if (fireTimer < FireCooldown) return;

        if (ActionRoll != null)
        {
            ActionRoll.isRolling = false;
        }

        Player.PlayerSkinController.TravelAnimation("Gun Fire");

        var nextTarget = CurrentTargets[0];
        if (nextTarget == null || !IsInstanceValid(nextTarget) || !nextTarget.IsInsideTree() )
        {
            // Happens if enemy dies before getting shot.
        }
        else
        {
            
            AudioPlayer.Stream = GunFireSound;
            AudioPlayer.Play(0);

            var bullet = BulletPrefab.Instantiate<HomingMissile>();
            GetTree().Root.AddChild(bullet);
            bullet.GlobalPosition = Player.GlobalPosition + Player.GlobalBasis.GetRotationQuaternion() * MuzzleOffset;
            bullet.LookAt(nextTarget.GlobalPosition);
            bullet.OwnerPlayerActionGun = this;
            bullet.Target = nextTarget;

            


            var lookTarget = nextTarget.GlobalPosition.ProjectOnPlane(Player.GlobalBasis.Y) + Player.GlobalPosition.Project(Player.GlobalBasis.Y);
            Player.LookAt(lookTarget, Player.GlobalBasis.Y);
            Player.TimedLockRotation = Mathf.Max(Player.TimedLockRotation, 0.1f);
            Player.TimedLerpedRotation = Mathf.Max(Player.TimedLerpedRotation, 0.2f);


            CurrentComboTargets[0].QueueFree();
        }



        CurrentTargets.RemoveAt(0);
        CurrentComboTargets.RemoveAt(0);
        fireTimer = 0;
    }

   public void CancelAimingAndFiring()
    {
        if (GunState == GunStateEnum.IDLE) return;
        CurrentTargets.Clear();
        foreach(var comboTarget in CurrentComboTargets)
        {
            comboTarget.QueueFree();
        }
        CurrentComboTargets.Clear();
        GunState = GunStateEnum.IDLE;
    }

    public void OnSuccessfulHit(int combo)
    {
    }
}
