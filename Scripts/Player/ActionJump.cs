using Godot;
using System;

public partial class ActionJump : Node
{
    [Export] public PlayerController Player;
    [Export] public ActionRoll ActionRoll;
    [Export] public int JumpCount = 0;
    [Export] public int MaxJumpCount = 2;

    [Export] public float JumpVelocity = 30f;
    [Export] public float MinJumpRiseTime = 0.3f;
    [Export] public float MaxJumpRiseTime = 1f;

    [Export] public float CoyoteTime = 0.1f;

    [Export] public AudioStreamPlayer3D audioStreamPlayer;
    [Export] public AudioStream audioStream;

    [Export] public bool RollDuringJump = true;

    private bool isJumping;
    private float riseVelocity;
    private double riseTimer;
    private double coyoteTimer;

    //public AudioSource audioSource;
    //public AudioClip groundJumpClip;
    //public AudioClip airJumpClip;

    public const string JUMP_INPUT_NAME = "actionjump";

    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
        if (Player.Grounded && !isJumping)
        {
            JumpCount = 0;
            coyoteTimer = 0;
        } else
        {
            coyoteTimer += delta;
            if (coyoteTimer > CoyoteTime && JumpCount == 0)
            {
                JumpCount = 1;
            }
        }

        if (riseTimer > MaxJumpRiseTime || (riseTimer > MinJumpRiseTime && !Player.PlayerInput.IsActionPressed(JUMP_INPUT_NAME)))
        {
            isJumping = false;
            riseTimer = 0;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if(Player.PlayerInput.IsActionPressed(@event, JUMP_INPUT_NAME) && JumpCount < MaxJumpCount)
        {
            Jump(false);
        }
    }

    public void Jump(bool resetCount)
    {
        if (resetCount) JumpCount = 0;

        JumpCount++;

        if (!Player.Grounded)
        {
            Player.GroundNormal = -Player.Gravity.Normalized();
        }

        //var groundNormal = Player.Grounded ? Player.GroundNormal : -Player.Gravity.Normalized();
        var groundNormal = Player.GroundNormal;
        Player.LinearVelocity = Vector3Utils.ProjectOnPlane(Player.LinearVelocity, groundNormal);
        Player.LinearVelocity += -Player.Gravity.Normalized() * JumpVelocity;
        riseTimer = 0;
        isJumping = true;
        if (JumpCount == 1)
        {
            riseVelocity = Mathf.Max(Vector3Utils.LengthAlongsideVector(Player.LinearVelocity, -Player.Gravity), JumpVelocity);
        } else
        {
            riseVelocity = JumpVelocity;
        }
        Player.DisableGroundRayTime = MinJumpRiseTime;
        Player.Grounded = false;

        //if (Player.Grounded)
        //{
        //    audioSource.PlayOneShot(groundJumpClip);
        //    playerEffects.GroundJump.Play();
        //}
        //else
        //{
        //    audioSource.PlayOneShot(airJumpClip);
        //    playerEffects.AirJump.Play();
        //}

        audioStreamPlayer.Stream = this.audioStream;
        audioStreamPlayer.Play();

        if (RollDuringJump)
        {
            ActionRoll.LockRollTimer = MinJumpRiseTime;
            ActionRoll.StartRoll(false);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (isJumping)
        {
            if (Vector3Utils.LengthAlongsideVector(Player.LinearVelocity, -Player.Gravity) < riseVelocity)
            {
                var velocityXZ = Vector3Utils.ProjectOnPlane(Player.LinearVelocity, Player.GroundNormal.Normalized());
                var velocityY = Player.GroundNormal.Normalized() * riseVelocity;
                if (Player.Grounded)
                {
                    velocityY += Player.GroundNormal.Normalized() * (Player.GroundStickingPower);
                }
                Player.LinearVelocity = velocityXZ + velocityY;
            }
            riseTimer += delta;

        }
    }
}
