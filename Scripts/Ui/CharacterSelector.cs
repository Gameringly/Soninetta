using Godot;
using Godot.Collections;
using System;

public partial class CharacterSelector : ScrollContainer
{
	[Export] public int PlayerCount = 1;
	[Export] public int NpcCount = 1;
	[Export] public bool AddEveryoneElseAsNpc = false;	// Recommended to set NpcCount to 0 if you use this.

	[Export] public GridContainer Container;

	[Export] public Array<OptionButton> PlayerOptionButtons = new Array<OptionButton>();
	[Export] public Array<OptionButton> InputOptionButtons = new Array<OptionButton>();
	[Export] public Array<OptionButton> NpcCharacterOptionButtons = new Array<OptionButton>();

	[Export] public string[] CharacterNames = new string[0];
	[Export] public PackedScene[] CharacterPrefabs = new PackedScene[0];

	[Export] public RichTextLabel PlayerTextTemplate;
	[Export] public OptionButton OptionTemplate;
	[Export] public Control PlaceholderTemplate;    // To keep grid columns intact

	const int KEYBOARD_INPUT_ID = 101;
	const int NO_CHARACTER_ID = 101;

	[Export] public Control PreviousMenu;
	[Export] public Control NextMenu;

	public override void _EnterTree()
	{
		RefreshWindow();
	}

	void RefreshWindow()
	{
		MakeMenu();
		PopulateOptions();

		for (int i = 0; i < InputOptionButtons.Count; i++)
		{
			InputOptionButtons[i].Selected = i;
		}
	}

	private void MakeMenu()
	{
		PlayerOptionButtons.Clear();
		InputOptionButtons.Clear();
		NpcCharacterOptionButtons.Clear();
		foreach (var child in Container.GetChildren())
		{
			child.QueueFree();
		}

		if (PlayerCount == 1)
		{
			Container.Columns = 2;
		} else
		{
			Container.Columns = 3;
		}

		for (int i = 0; i < PlayerCount; i++)
		{
			var playerText = PlayerTextTemplate.Duplicate().GetNode<RichTextLabel>(".");
			playerText.Text = "Player " + (i + 1).ToString();
			Container.AddChild(playerText);

			var characterOption = OptionTemplate.Duplicate().GetNode<OptionButton>(".");
			PlayerOptionButtons.Add(characterOption);
			Container.AddChild(characterOption);

			if (PlayerCount > 1)
			{
				var inputOption = OptionTemplate.Duplicate().GetNode<OptionButton>(".");
				InputOptionButtons.Add(inputOption);
				Container.AddChild(inputOption);
			}
		}

		for (int i = 0; i < NpcCount; i++)
		{
			var playerText = PlayerTextTemplate.Duplicate().GetNode<RichTextLabel>(".");
			playerText.Text = "CPU";
			Container.AddChild(playerText);

			var characterOption = OptionTemplate.Duplicate().GetNode<OptionButton>(".");
			NpcCharacterOptionButtons.Add(characterOption);
			Container.AddChild(characterOption);

			if (PlayerCount > 1)
			{
				var placeholder = PlaceholderTemplate.Duplicate();
				Container.AddChild(placeholder);
			}
		}
	}

	private void PopulateOptions()
	{
		var connectedJoypads = Input.GetConnectedJoypads();
		foreach (var inputOption in InputOptionButtons)
		{
			inputOption.Clear();
			inputOption.AddItem("Keyboard", KEYBOARD_INPUT_ID);
			foreach (var joypad in connectedJoypads)
			{
				inputOption.AddItem("Joypad " + (joypad + 1).ToString(), joypad);
			}
		}

		foreach (var playerOption in PlayerOptionButtons)
		{
			playerOption.Clear();
			for (int i = 0; i < CharacterNames.Length; i++)
			{
				playerOption.AddItem(CharacterNames[i], i);
			}
		}

		foreach (var npcOption in NpcCharacterOptionButtons)
		{
			npcOption.Clear();
			npcOption.AddItem("None", NO_CHARACTER_ID);
			for (int i = 0; i < CharacterNames.Length; i++)
			{
				npcOption.AddItem(CharacterNames[i], i);
			}
		}
	}

	void OnPlay()
	{
		ApplyCharacters();
		this.Visible = false;
		NextMenu.Visible = true;
		NextMenu.FindNextValidFocus().GrabFocus();
	}

	void OnBack()
	{
		this.Visible = false;
		PreviousMenu.Visible = true;
		PreviousMenu.FindNextValidFocus().GrabFocus();
	}

	void ApplyCharacters()
	{
		PlayersManager.PlayablePlayers = new Array<PackedScene>();
		PlayersManager.NpcPlayers = new Array<PackedScene>();
		PlayersManager.PlayerIdentifiers = new Array<string>();
		foreach (var playerOption in PlayerOptionButtons)
		{
			var selectedPlayer = playerOption.GetSelectedId();
			PlayersManager.PlayablePlayers.Add(CharacterPrefabs[selectedPlayer]);
		}

		if (PlayerCount == 1)
		{
			PlayersManager.PlayerIdentifiers.Add("any");
		} else
		{
			foreach (var inputOption in InputOptionButtons)
			{
				var selectedInput = inputOption.GetSelectedId();
				if (selectedInput == KEYBOARD_INPUT_ID)
				{
					PlayersManager.PlayerIdentifiers.Add("kb");
				}
				else
				{
					PlayersManager.PlayerIdentifiers.Add("pad" + selectedInput.ToString());
				}
			}
		}

		foreach (var npcPlayerOption in NpcCharacterOptionButtons)
		{
			var selectedPlayer = npcPlayerOption.GetSelectedId();
			if (selectedPlayer == NO_CHARACTER_ID) continue;
			 
			PlayersManager.NpcPlayers.Add(CharacterPrefabs[selectedPlayer]);
		}

		if (AddEveryoneElseAsNpc)
		{
			for(int i=0; i < CharacterPrefabs.Length; i++)
			{
				bool foundAmongPlayers = false;
                foreach (var playerOption in PlayerOptionButtons)
                {
                    var selectedPlayer = playerOption.GetSelectedId();
					if (selectedPlayer == i)
					{
						foundAmongPlayers = true;
						break;
					}
                }
				if (!foundAmongPlayers)
				{
                    PlayersManager.NpcPlayers.Add(CharacterPrefabs[i]);
                }
            }
		}
	}

	void ChangePlayers(int playersDiff)
	{
		PlayerCount = Math.Clamp(PlayerCount + playersDiff, 1, 4);
		RefreshWindow();
	}
}

