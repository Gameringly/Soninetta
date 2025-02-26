using Godot;
using System;

public partial class SplashScreenControl : Node
{
	[Export] public float ShowTime = 5;
	[Export] public string NextScene;
	[Export] public CanvasItem Content;

	private float Timer;

	public override void _Ready()
	{
		ResourceLoader.LoadThreadedRequest(NextScene);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsPressed())
		{
			Timer = Mathf.Max(ShowTime, Timer);
		}
	}

	public override void _Process(double delta)
	{
		Timer += (float)delta;
		if (Timer > ShowTime && ResourceLoader.LoadThreadedGetStatus(NextScene) == ResourceLoader.ThreadLoadStatus.Loaded)
		{
			var nextLevelPackedScene = ResourceLoader.LoadThreadedGet(NextScene);
			GetTree().ChangeSceneToPacked((PackedScene)nextLevelPackedScene);
		}

		var modulate = Content.Modulate;
		if (Timer < 1)
		{
			modulate.A = Timer;
		}
		else if (Timer > ShowTime - 1)
		{
			modulate.A = 1f - (Timer - ShowTime - 1);
		} else
		{
			modulate.A = 1;
		}
		Content.SelfModulate = modulate;
	}

}
