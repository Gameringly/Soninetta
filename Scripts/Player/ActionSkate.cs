using Godot;
using System;
using System.Threading.Tasks.Dataflow;

public partial class ActionSkate : Node
{
	[Export] public PlayerController Player;
	public PackedScene SkateboardModel;
	public Vector3 SkateboardStandingPoint = Vector3.Zero;
	private Node3D SpawnedSkateboardModel;
    public bool CanBoost = false;
    public bool FloatOnWater = false;
	[Export] public float FloatVelocity = 5f;

    private bool OriginallyCouldManuallyRoll;
	private bool OriginallyCouldSpinDash;
	private bool OriginallyCouldDropDash;
	private bool OriginallyCouldRollDuringJump;
	private float OriginalAntiInertiaFactor;
    private bool OriginalCanBoost;

    public override void _Ready()
	{
		Player.PlayerInventory.OnItemCollected += this.OnItemCollected;
	}

	private void OnItemCollected(string itemType, int newCount, int difference)
	{
		if (itemType == "skateboard" && newCount > 0)
		{

			if (!Player.SkateMode)
			{
				StartSkating();
			}
		} else if (itemType == "skateboard" && newCount <= 0)
		{
			if (Player.SkateMode)
			{
				StopSkating();
			}
		}
	}

	public void StartSkating()
	{
		Player.SkateMode = true;
		Player.DisableNoInputDecceleration = true;
		var actionRoll = Player.GetNodeOrNull<ActionRoll>("./PlayerControl/Actions/ActionRoll");
		if (actionRoll != null)
		{
			OriginallyCouldManuallyRoll = actionRoll.CanManuallyRoll;
			OriginallyCouldSpinDash = actionRoll.CanSpinDash;
			OriginallyCouldDropDash = actionRoll.CanDropDash;

			actionRoll.CanManuallyRoll = false;
			actionRoll.CanSpinDash = false;
			actionRoll.CanDropDash = false;
		}

		var actionJump = Player.GetNodeOrNull<ActionJump>("./PlayerControl/Actions/ActionJump");
		if (actionJump != null)
		{
			OriginallyCouldRollDuringJump = actionJump.RollDuringJump;

			actionJump.RollDuringJump = false;
		}

		var actionBoost = Player.GetNodeOrNull<ActionBoost>("./PlayerControl/Actions/ActionBoost");
		if (actionBoost != null)
		{
			OriginalCanBoost = actionBoost.ProcessMode != ProcessModeEnum.Disabled;
			actionBoost.ProcessMode = CanBoost ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
		}


		OriginalAntiInertiaFactor = Player.AntiInertiaFactor;
		Player.AntiInertiaFactor = 1;
		Player.PlayerSkinController.TravelAnimation("Skate");

		SpawnSkateboardModel();
		Player.PlayerSkinController.Position = SkateboardStandingPoint;
	}

	public void StopSkating()
	{
		Player.SkateMode = false;
		Player.DisableNoInputDecceleration = false;
		var actionRoll = Player.GetNodeOrNull<ActionRoll>("./PlayerControl/Actions/ActionRoll");
		if (actionRoll != null)
		{
			actionRoll.CanManuallyRoll = OriginallyCouldManuallyRoll;
			actionRoll.CanSpinDash = OriginallyCouldSpinDash;
			actionRoll.CanDropDash = OriginallyCouldDropDash;
		}

		var actionJump = Player.GetNodeOrNull<ActionJump>("./PlayerControl/Actions/ActionJump");
		if (actionJump != null)
		{
			actionJump.RollDuringJump = OriginallyCouldRollDuringJump;
		}

        var actionBoost = Player.GetNodeOrNull<ActionBoost>("./PlayerControl/Actions/ActionBoost");
        if (actionBoost != null)
        {
            actionBoost.ProcessMode = OriginalCanBoost ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
        }

        Player.AntiInertiaFactor = OriginalAntiInertiaFactor;
		Player.SlopeGravityFactor = 0.3f;

		Player.PlayerSkinController.TravelAnimation("Running");
		DespawnSkateboardModel();
		Player.PlayerSkinController.Position = Vector3.Zero;

	}

	public override void _PhysicsProcess(double delta)
	{
		if (!Player.SkateMode) return;

		if ((Player.LinearVelocity.Normalized()).Dot(Player.Gravity.Normalized()) >= 0)
		{
			Player.SlopeGravityFactor = 2f;
		}
		else
		{
			Player.SlopeGravityFactor = 0.3f;
		}

		if (FloatOnWater)
		{
			var waterInteraction = Player.GetNodeOrNull<WaterInteraction>("./PlayerControl/WaterInteraction");
			if (waterInteraction != null && waterInteraction.WaterFeetCounter > 0)
			{
				Player.LinearVelocity += -Player.Gravity.Normalized() * Player.Gravity.Length() * FloatVelocity * (float)delta;
				if (Player.VelocityYDirected < 0)
				{
					Player.LinearVelocity = Player.VelocityXZ;
				}
			} 
		}
	}

	void SpawnSkateboardModel()
	{
		SpawnedSkateboardModel = SkateboardModel.Instantiate<Node3D>();
		Player.PlayerSkinController.AddChild(SpawnedSkateboardModel);
	}

	void DespawnSkateboardModel()
	{
		SpawnedSkateboardModel.QueueFree();
		SpawnedSkateboardModel = null;

	}
}
