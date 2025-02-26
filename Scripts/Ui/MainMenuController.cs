using Godot;
using System;
using System.Collections.Generic;

public partial class MainMenuController : Control
{
	[Export] public PackedScene NextLevel;
	[Export] public Control MainMenuContainer;
	[Export] public Button DefaultMainMenuButton;
	[Export] public Control LevelSelectContainer;
	[Export] public Button DefaultLevelSelectButton;
	[Export] public CharacterSelector CharacterSelector;
	[Export] public Button DefaultCharacterSelectorButton;
	[Export] public OptionsMenu OptionsMenuContainer;
	[Export] public LoadingScreen LoadingScreen;
	private string CurrentlyLoadingScene;
	private SaveData SaveData;


	[Export] public Texture2D RedRingYesIcon;
	[Export] public Texture2D RedRingNoIcon;

	// Stage select
	[Export] public Node StagesData;
	[Export] public Button StageTemplateButton;


	public static MainMenuController Instance;
	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _ExitTree()
	{
		Instance = null;
	}

	public override void _Ready()
	{
		PopulateLevelSelect();
		SaveData = SaveData.Load();
		SaveData.Save();

		OptionsMenuContainer.Apply();

		ToMainMenu();
		LoadingScreen.Visible = false;

		Input.MouseMode = Input.MouseModeEnum.Visible;
		Engine.TimeScale = 1;
	}

	public override void _Process(double delta)
	{
		try
		{
			var focus = GetViewport().GuiGetFocusOwner();
			if (focus == null && this.Visible)
			{
				DefaultMainMenuButton.GrabFocus();
			}

			if (LoadingScreen.Returning && ResourceLoader.LoadThreadedGetStatus(CurrentlyLoadingScene) == ResourceLoader.ThreadLoadStatus.Loaded && LoadingScreen.Instance.Returning && LoadingScreen.Instance.TransitionProcess >= 1)
			{
				var nextLevelPackedScene = ResourceLoader.LoadThreadedGet(CurrentlyLoadingScene);
				GetTree().ChangeSceneToPacked((PackedScene)nextLevelPackedScene);
			}
		} catch (Exception e)
		{
			ErrorLog.AddMessage(e.Message + e.StackTrace.ToString());
		}
	}

	public void ToMainMenu()
	{
		HideAll();
		MainMenuContainer.Visible = true;
		DefaultMainMenuButton.GrabFocus();
	}

	public void ToLevelSelectMenu()
	{
		HideAll();
		//LevelSelectContainer.Visible = true;
		//DefaultLevelSelectButton.GrabFocus();
		CharacterSelector.PreviousMenu = MainMenuContainer;
		CharacterSelector.NextMenu = LevelSelectContainer;
		CharacterSelector.Visible = true;
		DefaultCharacterSelectorButton.GrabFocus();

	}

	public void HideAll()
	{
		MainMenuContainer.Visible = false;
		LevelSelectContainer.Visible = false;
		CharacterSelector.Visible = false;
		OptionsMenuContainer.Visible = false;
	}

	public void OnPlay()
	{
		HideAll();
		//		if (CurrentlyLoadingScene != "") return;

		//GetTree().ChangeSceneToPacked(NextLevel);
		LoadingScreen.Visible = true;
		LoadingScreen.Return();
		if (LoadingScreen.StagePathsToStageNames.ContainsKey(NextLevel.ResourcePath))
		{
			var zoneText = LoadingScreen.StagePathsToStageNames[NextLevel.ResourcePath].Item1.ToString().Trim();
			var actText = LoadingScreen.StagePathsToStageNames[NextLevel.ResourcePath].Item2.ToString().Trim();
			LoadingScreen.SetTexts(zoneText, actText);
		}
		if (ResourceLoader.LoadThreadedGetStatus(NextLevel.ResourcePath) == ResourceLoader.ThreadLoadStatus.InvalidResource)
		{
			ResourceLoader.LoadThreadedRequest(NextLevel.ResourcePath);
			CurrentlyLoadingScene = NextLevel.ResourcePath;
		}
	}


	public void OnPlay(string stagePath)
	{
		NextLevel = ResourceLoader.Load<PackedScene>(stagePath);
		CheckpointSystem.Reset();
		//CheckpointSystem.CheckpointScene = stagePath;
		CheckpointSystem.CheckpointScene = ProjectSettings.GetSetting("application/run/main_scene").AsString();
		OnPlay();
	}

	public void OnOptions()
	{
		HideAll();
		OptionsMenuContainer.Visible = true;
		OptionsMenuContainer.SetDefaultFocus();
	}

	public void OnQuit()
	{
		GetTree().Quit();
	}

	void PopulateLevelSelect()
	{
		if (SaveData == null) SaveData = SaveData.Load();
		foreach (var stageDataNode in StagesData.GetChildren())
		{
			MainMenuStagesData stage = (MainMenuStagesData)stageDataNode;
			if (!(stage.UnlockedByDefault || SaveData.IsLevelUnlocked(stage.StageUniqueIdentifier))) continue;
			var newButton = (BaseButton)StageTemplateButton.Duplicate();
			var stageNameLabel = newButton.GetNode<Label>("./HBoxContainer/StageName");
			stageNameLabel.Text = stage.StageName + "\n" + stage.ActName;

			newButton.Pressed += () => { this.OnPlay(stage.ScenePath); };

			string bestTimeString = "--:--:--";
			string bestRankString = "-";

			string characterName = "Sonic";		// TODO: Use your character

			if (SaveData.LevelSaves.ContainsKey(stage.StageUniqueIdentifier)) {
				if (SaveData.LevelSaves[stage.StageUniqueIdentifier].BestTime.ContainsKey(characterName))
				{
					bestTimeString = TimeSpan.FromSeconds(SaveData.LevelSaves[stage.StageUniqueIdentifier].BestTime[characterName]).ToString(@"mm\:ss\:ff");
				}

				if (SaveData.LevelSaves[stage.StageUniqueIdentifier].BestRank.ContainsKey(characterName))
				{
					bestRankString = SaveData.LevelSaves[stage.StageUniqueIdentifier].BestRank[characterName];
				}

				if (stage.HasRedRings)
				{
					newButton.GetNode<Control>("./HBoxContainer/VBoxContainer/RedRings").Visible = true;
					for (int redring = 1; redring <= 5; redring++)
					{
						if (SaveData.LevelSaves[stage.StageUniqueIdentifier].Items.GetValueOrDefault("RedRing" + redring.ToString()) > 0)
						{
							newButton.GetNode<TextureRect>("./HBoxContainer/VBoxContainer/RedRings/RedRing" + redring.ToString()).Texture = RedRingYesIcon;
						}
					}
				} else
				{
					newButton.GetNode<Control>("./HBoxContainer/VBoxContainer/RedRings").Visible = false;
				}
			}

			newButton.GetNode<Label>("./HBoxContainer/VBoxContainer/BestTime").Text = "BEST TIME:   " + bestTimeString;
			newButton.GetNode<Label>("./HBoxContainer/VBoxContainer/BestRank").Text = "BEST RANK:  " + bestRankString;


			newButton.Visible = true;
			newButton.ProcessMode = ProcessModeEnum.Inherit;

			LevelSelectContainer.GetChild(0).AddChild(newButton);
			LevelSelectContainer.GetChild(0).MoveChild(newButton, LevelSelectContainer.GetChild(0).GetChildCount()-2);
		}
		StageTemplateButton.Visible = false;
		StageTemplateButton.ProcessMode = ProcessModeEnum.Inherit;
	}
}
