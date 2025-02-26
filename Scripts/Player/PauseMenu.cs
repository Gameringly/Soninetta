using Godot;
using System;

public partial class PauseMenu : Control
{
	[Export] public Control MenuContainer;
	[Export] public Control RestartMenuContainer;
	[Export] public OptionsMenu OptionsMenuContainer;
	[Export] public Control RestartBackButton;

	[Export] public Control DefaultFocus;
	[Export] public Control DefaultRestartFocus;
	[Export] public Control RestartMenuButton;

	//[Export] public PackedScene ExitScene;
	[Export] public string ExitScene;	// I hate it but it's to avoid circular dependency.

	const string inputName = "any_pause";
	bool IsPaused = false;


    public static PauseMenu Instance;

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
		HideAll();
		if (StageData.Instance.Style != StageData.StageStyle.ActionStage)
		{
			RestartMenuButton.Visible = false;
            RestartMenuButton.ProcessMode = ProcessModeEnum.Disabled;
        }
		Input.MouseMode = Input.MouseModeEnum.Captured;
    }

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed(inputName)) // Not using PlayerInput here because it should work with zero TimeScale and it's independent of players
		{
			IsPaused = !IsPaused;
			if (!IsPaused)
			{
				Unpause();
			} 
			else
			{
				Pause();
			}
		}
	}

	public void Unpause()
	{
		IsPaused = false;
        //Engine.TimeScale = 1;
        GetTree().Paused = false;
        HideAll();
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

	private void HideAll()
	{
		MenuContainer.Visible = false;
		RestartMenuContainer.Visible = false;
		OptionsMenuContainer.Visible = false;
	}

	public void Pause()
	{
		IsPaused = true;
        //Engine.TimeScale = 0;
        GetTree().Paused = true;
        //PhysicsServer3D.SetActive(true);
		HideAll();
        MenuContainer.Visible = true;
		DefaultFocus.GrabFocus();
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

	public void Restart()
	{
		HideAll();
		RestartMenuContainer.Visible = true;
		DefaultRestartFocus.GrabFocus();
		RestartBackButton.Visible = PlayerDamage.AlivePlayersCount() > 0;
        Input.MouseMode = Input.MouseModeEnum.Visible;
		Engine.TimeScale = 1;
    }

	public void Exit()
	{
		Unpause();
		GetTree().ChangeSceneToFile(ExitScene);
//		GetTree().ChangeSceneToPacked(ExitScene);
	}

	public void RestartMenuYes()
	{
		CheckpointSystem.Reset();
		Unpause();
		GetTree().ReloadCurrentScene();
	}
	public void RestartMenuCheckpoint()
	{
		Unpause();
        GetTree().ReloadCurrentScene();
    }

	public void RestartMenuNo()
	{
		HideAll();
		Pause();
	}

	public void Options()
	{
		HideAll();
		OptionsMenuContainer.Visible = true;
		OptionsMenuContainer.SetDefaultFocus();
	}

}



