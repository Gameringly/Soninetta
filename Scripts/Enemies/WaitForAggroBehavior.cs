using Godot;
using System;

public partial class WaitForAggroBehavior : Node
{
	[Export] public EnemyBehaviorManager BehaviorManager;
	[Export] public Node NextPhase;
	[Export] public AudioStream Music;
	[Export] public Control HpUI;
	[Export] public bool AddTarget;

	private bool active = false;
	public override void _Ready()
	{
		active = false;
	}

	public override void _Process(double delta)
	{
		if (!active) { return; }
	}

	public void OnTriggerEnter(Node3D other)
	{
		if (active || !this.IsProcessing()) { return; }
		var player = other.GetNodeOrNull<PlayerController>(".");
		if (player != null)
		{
			active = true;
			//if (Music != null)
			//{
			//    var musicPlayer = MusicPlayerController.Instance;
			//    if (musicPlayer != null)
			//    {
			//        musicPlayer.OverrideMusic = Music;
			//        musicPlayer.SwitchMusicToOverride();
			//    }
			//}
			BehaviorManager.SetPhase(NextPhase);
			//if (HpUI != null)
			//{
			//    HpUI.Visible = true;
			//}
			if (AddTarget)
			{
				//PlayerCamera.Instances[player.PlayerID].CameraTargets.Add(this.GetNode<Node3D>("../../.."));
				var cam = PlayerCamera.Instances[player.PlayerID];
				cam.Mode = PlayerCamera.CameraMode.LookAtPoint;
				cam.Point = this.GetNode<Node3D>("../../..");

            }
		}
	}
}
