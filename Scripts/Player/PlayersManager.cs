using Godot;
using Godot.Collections;
using System;

public partial class PlayersManager : Node3D
{
	// List of players
	public static Array<PackedScene> PlayablePlayers;
	public static Array<string> PlayerIdentifiers;	// Defines inputs
	public static Array<PackedScene> NpcPlayers;

	[Export] public Array<PackedScene> DefaultPlayablePlayers;
	[Export] public Array<string> DefaultPlayerIdentifiers; // Defines inputs
	[Export] public Array<PackedScene> DefaultNpcPlayers;


	// Player UI
	[Export] public PlayerUI PlayerUI;
	//[Export] public SubViewportContainer FirstViewportContainerPrefab;
	[Export] public PackedScene ViewportContainerPrefab;

	// Containers
	[Export] public Node3D PlayersContainer;
	[Export] public GridContainer SubviewportContainer;

	// Gizmos
	[Export] public Node3D StartPointMesh;

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey eventKey)
		{
			if (eventKey.Pressed)
			{
				if (eventKey.Keycode == Key.Pageup)
				{
					SwapCharacters(+1);
				}
				else if (eventKey.Keycode == Key.Pagedown)
				{
					SwapCharacters(-1);
				}
			}
		}
	}

	public override void _Ready()
	{
		if (StartPointMesh != null)
		{
			StartPointMesh.QueueFree();
		}

		if (PlayablePlayers == null || PlayablePlayers.Count < 1)
		{
			PlayablePlayers = DefaultPlayablePlayers;
			PlayerIdentifiers = DefaultPlayerIdentifiers;
			NpcPlayers = DefaultNpcPlayers;

		}

		if (PlayablePlayers.Count <= 2)
		{
			SubviewportContainer.Columns = 1;
		}
		else
		{
			SubviewportContainer.Columns = (PlayablePlayers.Count + 1) / 2;
		}

		// Adding real players
		PlayerController player1 = null;
		for (int i = 0; i < PlayablePlayers.Count; i++)
		{
			var player = PlayablePlayers[i].Instantiate<PlayerController>();
			player.PlayerInput.PlayerIdentifier = PlayerIdentifiers[i];
			PlayersContainer.AddChild(player);
			player.TopLevel = true;
			player.GlobalRotation = this.GlobalRotation;

			if (i == 0)
			{
				player1 = player;
			}

			//if (i > 0)
			{
				var newSubviewport = ViewportContainerPrefab.Instantiate<SubViewportContainer>(); //FirstViewportContainerPrefab.Duplicate();
				var camera = newSubviewport.GetNodeOrNull<PlayerCamera>("./SubViewport/Camera3D");
				if (camera != null)
				{
					camera.PlayerID = player.PlayerID;
				}

				var playerUI = newSubviewport.GetNodeOrNull<OnePlayerUI>("./SubViewport/PlayerUI");
				if (playerUI != null)
				{
					playerUI.PlayerID = player.PlayerID;
					playerUI.HomingIcon = playerUI.GetNode<Node2D>("./HomingIcon");
				}

				
				SubviewportContainer.AddChild(newSubviewport);
			}
		}

		foreach(var cam in PlayerCamera.Instances)
		{
			cam.Value.GlobalRotation = this.GlobalRotation;
		}

		// Adding NPC players
		for (int i = 0; i < NpcPlayers.Count; i++)
		{
			var player = NpcPlayers[i].Instantiate<PlayerController>();
			player.PlayerInput.PlayerIdentifier = "npc" + i.ToString();
			player.PlayerInput.ReadRealInput = false;
			var npcControl = player.GetNodeOrNull<NpcPartnerControl>("./PlayerControl/NpcControl");
			if (npcControl != null)
			{
				npcControl.IsNpc = true;
				npcControl.Target = player1;
			}
			PlayersContainer.AddChild(player);
			player.TopLevel = true;
			player.GlobalRotation = this.GlobalRotation;
			player.Position += player.Basis.X * (i / 2 + 1) * (i % 2 == 0 ? 1 : -1) * 5;
		}
	}

	void SwapCharacters(int offset)
	{
		if (offset == 0) return;
		// TODO: Incompatible with multiplayer. Make teams.
		// TODO: No input mapping.
		int currentPlayerIdx = 0;
		foreach(var player in PlayerController.Instances)
		{
			if (!player.NpcPartnerControl.IsNpc)
			{
				currentPlayerIdx = player.PlayerID;
				break;
			}
		}
		int nextPlayerIdx = (currentPlayerIdx + offset) % PlayerController.Instances.Count;

		var currentPlayer = PlayerController.Instances[currentPlayerIdx];
		var nextPlayer = PlayerController.Instances[nextPlayerIdx];

		// Swap camera
		var cam = PlayerCamera.Instances[currentPlayer.PlayerID];
		cam.PlayerID = nextPlayer.PlayerID;
		cam.CameraTargets.Remove(currentPlayer);
		cam.CameraTargets.Add(nextPlayer);
		cam.Player = nextPlayer;

		PlayerCamera.Instances.Remove(currentPlayer.PlayerID);
		PlayerCamera.Instances.Add(nextPlayer.PlayerID, cam);


		// Swap inputs
		currentPlayer.NpcPartnerControl.IsNpc = true;
		currentPlayer.NpcPartnerControl.Target = nextPlayer;
		currentPlayer.PlayerInput.ReadRealInput = false;

		nextPlayer.NpcPartnerControl.IsNpc = false;
		nextPlayer.PlayerInput.PlayerIdentifier = currentPlayer.PlayerInput.PlayerIdentifier;
		nextPlayer.PlayerInput.ReadRealInput = true;

		// Swap UI
		var onePlayerUI = OnePlayerUI.Instances[currentPlayer.PlayerID];
		OnePlayerUI.Instances.Remove(currentPlayer.PlayerID);
		OnePlayerUI.Instances.Add(nextPlayer.PlayerID, onePlayerUI);

		onePlayerUI.PlayerID = nextPlayer.PlayerID;

		// Teleport
		nextPlayer.GlobalPosition = currentPlayer.GlobalPosition;
		nextPlayer.LinearVelocity = currentPlayer.LinearVelocity;
	}

}
