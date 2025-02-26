using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

public partial class OptionsMenu : VBoxContainer
{
	const string SaveFileName = "options.json";

	[Export] public Control DefaultFocus;

	[Export] public Slider AudioSliderMaster;
	[Export] public Slider AudioSliderMusic;
	[Export] public Slider AudioSliderEffects;

	[Export] public OptionButton WindowedOption;
	[Export] public OptionButton WindowSizeOption;
	[Export] public Slider ResolutionScaleSlider;
	[Export] public Label ResolutionScaleLabel;
	[Export] public OptionButton ScalingModeOption;
	[Export] public OptionButton AaTaaOption;
	[Export] public OptionButton AaMsaaOption;
	[Export] public OptionButton AaFxaaOption;

	[Export] public OptionButton ShadowResolutionOption;
	[Export] public OptionButton ShadowFilteringOption;
	[Export] public OptionButton ModelQualityOption;
	[Export] public OptionButton GIOption;
	[Export] public OptionButton BloomOption;
	[Export] public OptionButton AOOption;
	[Export] public OptionButton SSROption;
	[Export] public OptionButton SSLOption;
	[Export] public OptionButton VolumetricFogOption;

	[Export] public Slider RenderDistanceSlider;
	[Export] public Label RenderDistanceLabel;
	[Export] public CheckButton ShowFpsButton;

	[Export] public OptionButton CameraDynamicFovOption;
	[Export] public OptionButton CameraSmoothingOption;
	[Export] public OptionButton CameraAutoAirMoveOption;
	[Export] public OptionButton CameraVomitCameraOption;
	[Export] public OptionButton CameraAutoLookDownOption;

	[Export] public GridContainer KeyboardButtonsContainer;
	[Export] public Label KeyboardLabelTemplate;
	[Export] public Button KeyboardButtonTemplate;

	private Button[] InputButtons = new Button[0];
	private string[] InputActions = new string[0];
	[Export]
	public Godot.Collections.Dictionary<string, string> KbActionsToDescriptions = new Godot.Collections.Dictionary<string, string>()
	{
		{ "kb_actionjump", "Jump" },
		{ "kb_actionroll", "Roll/Drift" },
		{ "kb_actiondash", "Dash/Boost" },
		{ "kb_actionhoming", "Homing attack" },
		{ "kb_actionstomp", "Stomp" },
		{ "kb_actionsuper", "Super transformation" },

		{ "kb_moveup", "Move ↑" },
		{ "kb_movedown","Move ↓"  },
		{ "kb_moveleft","Move ←"  },
		{ "kb_moveright","Move →" },

		{ "kb_camup", "Camera ↑" },
		{ "kb_camdown","Camera ↓"  },
		{ "kb_camleft","Camera ←"  },
		{ "kb_camright","Camera →" },

		{ "kb_quickstep_l", "Quick step ←" },
		{ "kb_quickstep_r", "Quick step →" },
	};

	public Vector2I[] Resolutions = new Vector2I[0];

	public class OptionsFile
	{
		[JsonIgnore] public string GameVersion;
		[JsonInclude] public float VolumeMaster = 0.5f;
		[JsonInclude] public float VolumeMusic = 0.5f;
		[JsonInclude] public float VolumeSFX = 0.5f;
		[JsonInclude] public int Windowed = 0;
		[JsonInclude] public float ResolutionScale = 1f;
		[JsonInclude] public int WindowSizeX = 1920;
		[JsonInclude] public int WindowSizeY = 1080;
		[JsonInclude] public int ScalingMode = 0;
		[JsonInclude] public int AaTAA = 0;
		[JsonInclude] public int AaMSAA = 0;
		[JsonInclude] public int AaFXAA = 0;
		[JsonInclude] public int ShadowResolution = 0;
		[JsonInclude] public int ShadowFiltering = 0;
		[JsonInclude] public int ModelQuality = 0;
		[JsonInclude] public int GI = 0;
		[JsonInclude] public int Bloom = 0;
		[JsonInclude] public int AO = 0;
		[JsonInclude] public int SSR = 0;
		[JsonInclude] public int SSL = 0;
		[JsonInclude] public int VolumetricFog = 0;
		[JsonInclude] public float RenderDistance = 4000f;
		[JsonInclude] public bool ShowFps = false;

		[JsonInclude] public int CameraDynamicFov = 1;
		[JsonInclude] public int CameraSmoothing = 1;
		[JsonInclude] public int CameraAutoAirMove = 1;
		[JsonInclude] public int CameraVomitCamera = 1;
		[JsonInclude] public int CameraAutoLookDown = 1;


		[JsonInclude] public System.Collections.Generic.Dictionary<string, Key> KeyboardInput = new System.Collections.Generic.Dictionary<string, Key>();  // Key: action, value: keycode

		public static System.Collections.Generic.Dictionary<string, Key> DefaultKeyboardInput = new System.Collections.Generic.Dictionary<string, Key>()
		{
			{ "kb_actionjump", Key.Space },
			{ "kb_actionroll", Key.Ctrl },
			{ "kb_actiondash", Key.Shift },
			{ "kb_actionhoming", Key.J },
			{ "kb_actionstomp", Key.K },
			{ "kb_actionsuper", Key.Backspace },

			{ "kb_moveup", Key.W },
			{ "kb_movedown", Key.S },
			{ "kb_moveleft", Key.A },
			{ "kb_moveright", Key.D },

			{ "kb_camup", Key.Kp8 },
			{ "kb_camdown", Key.Kp2 },
			{ "kb_camleft", Key.Kp4 },
			{ "kb_camright", Key.Kp6 },

			{ "kb_quickstep_l", Key.Q },
			{ "kb_quickstep_r", Key.E },

		};  // Key: action, value: keycode 

		public void Save()
		{
			this.GameVersion = ProjectSettings.GetSetting("application/config/version", "0.0.0").AsString();

			var serializedSave = JsonSerializer.Serialize(this);
			using var saveFile = FileAccess.Open("user://" + SaveFileName, FileAccess.ModeFlags.Write);
			saveFile.StoreString(serializedSave);
		}

		public static OptionsFile Load()
		{
			OptionsFile loadedSave;
			if (!FileAccess.FileExists("user://" + SaveFileName))
			{
				loadedSave = new OptionsFile(); // No save data
			}
			else
			{
				using var saveFile = FileAccess.Open("user://" + SaveFileName, FileAccess.ModeFlags.Read);
				var serializedSave = saveFile.GetAsText();
				loadedSave = JsonSerializer.Deserialize<OptionsFile>(serializedSave);
			}
			// Apply defaults to unassigned buttons
			foreach(var kbAction in OptionsFile.DefaultKeyboardInput.Keys)
			{
				if (!loadedSave.KeyboardInput.ContainsKey(kbAction))
				{
					loadedSave.KeyboardInput.Add(kbAction, DefaultKeyboardInput[kbAction]);
				}
			}
			return loadedSave;
		}

		
	}
	public static OptionsFile Options;
	private bool RecordingKeyboardInput;
	private string RecordingKeyboardInputAction;

	public void OnWindowedOptionChange(int selection)
	{
		OptionsMenu.Options.Windowed = selection;
		GetResolutions();

	}

	public void OnWindowSizeOptionChange(int selection)
	{
		OptionsMenu.Options.WindowSizeX = Resolutions[selection].X;
		OptionsMenu.Options.WindowSizeY = Resolutions[selection].Y;
	}

	public void OnResolutionScaleChange(float scale)
	{
		OptionsMenu.Options.ResolutionScale = scale;
		UpdateLabels();
	}

	public void OnAaTAAChange(int selection)
	{
		OptionsMenu.Options.AaTAA = selection;
	}

	public void OnAaMSAAChange(int selection)
	{
		OptionsMenu.Options.AaMSAA = selection;
	}
	public void OnAaFXAAChange(int selection)
	{
		OptionsMenu.Options.AaFXAA = selection;
	}

	public void OnScalingModeChange(int selection)
	{
		OptionsMenu.Options.ScalingMode = selection;
	}

	public void OnShadowResolutionChange(int selection)
	{
		OptionsMenu.Options.ShadowResolution = selection;
	}

	public void OnShadowFilteringChange(int selection)
	{
		OptionsMenu.Options.ShadowFiltering = selection;
	}

	public void OnModelQualityChange(int selection)
	{
		OptionsMenu.Options.ModelQuality = selection;
	}

	public void OnGIChange(int selection)
	{
		OptionsMenu.Options.GI = selection;
	}

	public void OnBloomChange(int selection)
	{
		OptionsMenu.Options.Bloom = selection;
	}

	public void OnAOChange(int selection)
	{
		OptionsMenu.Options.AO = selection;
	}

	public void OnSSRChange(int selection)
	{
		OptionsMenu.Options.SSR = selection;
	}

	public void OnSSLChange(int selection)
	{
		OptionsMenu.Options.SSL = selection;
	}
	public void OnVolumetricFogChange(int selection)
	{
		OptionsMenu.Options.VolumetricFog = selection;
	}

	public void OnRestoreDefaultKeyboardControls()
	{
		OptionsMenu.Options.KeyboardInput.Clear();
		foreach(var kbAction in OptionsFile.DefaultKeyboardInput.Keys)
		{
			OptionsMenu.Options.KeyboardInput.Add(kbAction, OptionsFile.DefaultKeyboardInput[kbAction]);
		}
		UpdateLabels();
	}

	public void Apply()
	{
		// Windowed/Fullscreen
		switch (OptionsMenu.Options.Windowed)
		{
			case 0:
				GetTree().Root.Mode = Window.ModeEnum.Windowed;
				break;

			case 1:
				GetTree().Root.Mode = Window.ModeEnum.Fullscreen;
				break;

			case 2:
				GetTree().Root.Mode = Window.ModeEnum.ExclusiveFullscreen;
				break;
		}
		// Window size
		if (OptionsMenu.Options.Windowed == 0)
		{
			DisplayServer.WindowSetSize(new Vector2I(OptionsMenu.Options.WindowSizeX, OptionsMenu.Options.WindowSizeY));
		} else
		{
			DisplayServer.WindowSetSize(DisplayServer.ScreenGetUsableRect().Size);
		}
		// Resolution scale
		ProjectSettings.SetSetting("rendering/scaling_3d/scale", OptionsMenu.Options.ResolutionScale);
		// Antialiasing
		ProjectSettings.SetSetting("rendering/anti_aliasing/quality/use_taa", OptionsMenu.Options.AaTAA == 1);
		ProjectSettings.SetSetting("rendering/anti_aliasing/quality/msaa_3d", OptionsMenu.Options.AaMSAA);
		ProjectSettings.SetSetting("rendering/anti_aliasing/quality/screen_space_aa", OptionsMenu.Options.AaFXAA);
		// Filter
		ProjectSettings.SetSetting("rendering/scaling_3d/mode", OptionsMenu.Options.ScalingMode);
		//Shadows & quality
		int[] shadowAtlasSize = { 256, 512, 1024, 2048, 4096, 8192 };
		ProjectSettings.SetSetting("rendering/lights_and_shadows/directional_shadow/size", shadowAtlasSize[OptionsMenu.Options.ShadowResolution]);
		ProjectSettings.SetSetting("rendering/lights_and_shadows/positional_shadow/atlas_size", shadowAtlasSize[OptionsMenu.Options.ShadowResolution]);
		ProjectSettings.SetSetting("rendering/lights_and_shadows/positional_shadow/soft_shadow_filter_quality", OptionsMenu.Options.ShadowFiltering);
		float[] meshLodThreshold = { 8, 4, 2, 1, 0 };
		ProjectSettings.SetSetting("rendering/mesh_lod/lod_change/threshold_pixels", OptionsMenu.Options.ModelQuality);
		// Effects
		switch(OptionsMenu.Options.GI)
		{
			case 0:
				ProjectSettings.SetSetting("rendering/global_illumination/gi/use_half_resolution", true);
				ProjectSettings.SetSetting("rendering/global_illumination/voxel_gi/quality", 0);
				break;
			case 1:
				ProjectSettings.SetSetting("rendering/global_illumination/gi/use_half_resolution", false);
				ProjectSettings.SetSetting("rendering/global_illumination/voxel_gi/quality", 0);
				break;
			case 2:
				ProjectSettings.SetSetting("rendering/global_illumination/gi/use_half_resolution", false);
				ProjectSettings.SetSetting("rendering/global_illumination/voxel_gi/quality", 1);
				break;

		}
		//ProjectSettings.SetSetting("", OptionsMenu.Options.Bloom);	// TODO: Bloom is attached to environment
		ProjectSettings.SetSetting("rendering/environment/ssao/quality", OptionsMenu.Options.AO);
		ProjectSettings.SetSetting("rendering/environment/ssil/quality", OptionsMenu.Options.SSL);
		ProjectSettings.SetSetting("rendering/environment/screen_space_reflection/roughness_quality", OptionsMenu.Options.SSR);
		ProjectSettings.SetSetting("rendering/environment/volumetric_fog/use_filter", OptionsMenu.Options.VolumetricFog);

		// Keyboard input
		foreach(var keyboardInput in OptionsMenu.Options.KeyboardInput)
		{
			RemapKeyboard(keyboardInput.Key, keyboardInput.Value);
		}

		// Volume
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), Mathf.LinearToDb( OptionsMenu.Options.VolumeMaster));
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), Mathf.LinearToDb(OptionsMenu.Options.VolumeMusic));
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"), Mathf.LinearToDb(OptionsMenu.Options.VolumeSFX));



		UpdatePlayerCameraSettings();
	}

	public void OnBack()
	{
		OptionsMenu.Options = OptionsFile.Load();
		// This menu can be in both pause and main menu. We need to react in both cases.
		if (PauseMenu.Instance != null)
		{
			PauseMenu.Instance.Pause();
		}
		else if (MainMenuController.Instance != null)
		{
			MainMenuController.Instance.ToMainMenu();
		}

		SetDefaultScrolling();
		RecordingKeyboardInput = false;

	}

	public void OnApply()
	{
		Apply();
		OptionsMenu.Options.Save();
		OnBack();
	}

	public override void _Ready()
	{
		//try
		//{
			OptionsMenu.Options = OptionsFile.Load();
			PopulateKeyboardButtons();
			UpdateLabels();
			WindowedOption.Selected = OptionsMenu.Options.Windowed;
			ResolutionScaleSlider.Value = OptionsMenu.Options.ResolutionScale;
			ScalingModeOption.Selected = OptionsMenu.Options.ScalingMode;
			AaTaaOption.Selected = OptionsMenu.Options.AaTAA;
			AaMsaaOption.Selected = OptionsMenu.Options.AaMSAA;
			AaFxaaOption.Selected = OptionsMenu.Options.AaFXAA;

			ShadowResolutionOption.Selected = OptionsMenu.Options.ShadowResolution;
			ShadowFilteringOption.Selected = OptionsMenu.Options.ShadowFiltering;
			ModelQualityOption.Selected = OptionsMenu.Options.ModelQuality;
			GIOption.Selected = OptionsMenu.Options.GI;
			BloomOption.Selected = OptionsMenu.Options.Bloom;
			AOOption.Selected = OptionsMenu.Options.AO;
			SSROption.Selected = OptionsMenu.Options.SSR;
			SSLOption.Selected = OptionsMenu.Options.SSL;
			VolumetricFogOption.Selected = OptionsMenu.Options.VolumetricFog;
			RenderDistanceSlider.Value = OptionsMenu.Options.RenderDistance;
			ShowFpsButton.ButtonPressed = OptionsMenu.Options.ShowFps;

			CameraDynamicFovOption.Selected = OptionsMenu.Options.CameraDynamicFov;
			CameraSmoothingOption.Selected = OptionsMenu.Options.CameraSmoothing;
			CameraAutoAirMoveOption.Selected = OptionsMenu.Options.CameraAutoAirMove;
			CameraVomitCameraOption.Selected = OptionsMenu.Options.CameraVomitCamera;
			CameraAutoLookDownOption.Selected = OptionsMenu.Options.CameraAutoLookDown;

			UpdateLabels();
			GetResolutions();

			// TODO: Some options don't work on mobile. Block them if game is run on a phone. Verify which ones.
			var platform = OS.GetName();
			if (platform == "Android" || platform == "iOS")
			{
				WindowedOption.Disabled = true;
				WindowSizeOption.Disabled = true;
			}

			UpdatePlayerCameraSettings();
			SetDefaultScrolling();
			Apply();
		//} catch (Exception e)
		//{
		//	ErrorLog.AddMessage(e.Message + e.StackTrace.ToString());
		//}

	}

	void UpdateLabels()
	{
		var resolution = GetTree().Root.Size;
		resolution.X = (int)(resolution.X * OptionsMenu.Options.ResolutionScale);
		resolution.Y = (int)(resolution.Y * OptionsMenu.Options.ResolutionScale);
		ResolutionScaleLabel.Text = OptionsMenu.Options.ResolutionScale.ToString("0.00") + "x (" + resolution.X + "x" + resolution.Y + ")";

		for (int i = 0; i < InputButtons.Length; i++)
		{
			string kbText = "Unassigned";
			if (OptionsMenu.Options.KeyboardInput.ContainsKey(InputActions[i]))
			{
				kbText = OS.GetKeycodeString(OptionsMenu.Options.KeyboardInput[InputActions[i]]);
			}
			InputButtons[i].Text = kbText;
		}

		AudioSliderMaster.Value = OptionsMenu.Options.VolumeMaster;
		AudioSliderEffects.Value = OptionsMenu.Options.VolumeSFX;
		AudioSliderMusic.Value = OptionsMenu.Options.VolumeMusic;

		RenderDistanceLabel.Text = (OptionsMenu.Options.RenderDistance / 1000f).ToString() + " km";
	}

	public void SetDefaultFocus()
	{
		DefaultFocus.GrabFocus();
	}

	public void OnRemapKeyboardButton(string actionName)
	{
		RecordingKeyboardInput = true;
		RecordingKeyboardInputAction = actionName;
		
		for (int i = 0; i < InputButtons.Length; i++)
		{
			if (InputActions[i] == actionName)
			{
				InputButtons[i].Text = "Press a keyboard button";
				break;
			}
		}
	}

	public override void _Input(InputEvent @event)
	{
		try
		{
			if (!this.Visible) return;

			if (RecordingKeyboardInput && @event is InputEventKey inputEventKey)
			{
				UpdateLabels();
				if (@event.IsActionPressed("ui_cancel")) return;
				if (!inputEventKey.IsPressed()) return;

				var keycode = inputEventKey.Keycode;
				RemapKeyboard(RecordingKeyboardInputAction, keycode);
				OptionsMenu.Options.KeyboardInput[RecordingKeyboardInputAction] = keycode;

				RecordingKeyboardInput = false;
				UpdateLabels();
			}
			else if (@event.IsActionPressed("ui_cancel"))
			{
				OnBack();
			}
		}
		catch (Exception e)
		{
			ErrorLog.AddMessage(e.Message + e.StackTrace.ToString());
			
		}
	}

	void RemapKeyboard(string actionName, Key keycode)
	{
		if (!InputMap.HasAction(actionName))
		{
			InputMap.AddAction(actionName);
		}

		InputMap.ActionEraseEvents(actionName);
		InputEventKey newEvent = new InputEventKey();
		newEvent.Keycode = keycode;
		InputMap.ActionAddEvent(actionName, newEvent);


		if (actionName.Substring(0, 3) == "kb_")
		{
			var anyAction = "any_" + actionName.Substring(3);
			if (!InputMap.HasAction(anyAction)) {
				InputMap.AddAction(anyAction);
			} else
			{
				foreach(var action in InputMap.ActionGetEvents(anyAction))
				{
					if (action is InputEventKey)
					{
						InputMap.ActionEraseEvent(anyAction, action);
					}
				}
			}

			InputMap.ActionAddEvent(anyAction, newEvent);

		}
	}

	public void OnAudioMasterSliderChange(float value)
	{
		OptionsMenu.Options.VolumeMaster = value;
	}

	public void OnAudioMusicSliderChange(float value)
	{
		OptionsMenu.Options.VolumeMusic = value;
	}

	public void OnAudioSFXSliderChange(float value)
	{
		OptionsMenu.Options.VolumeSFX = value;
	}

	public void OnRenderDistanceChange(float value)
	{
		// TODO: It doesn't work instantly, requires restart.
		OptionsMenu.Options.RenderDistance = value;
		UpdateLabels();
	}

	public void OnShowFpsChange(bool value)
	{
		OptionsMenu.Options.ShowFps = value;
	}

	public void OnCameraDynamicFovChange(int value)
	{
		OptionsMenu.Options.CameraDynamicFov = value;
	}

	public void OnCameraSmoothingChange(int value)
	{
		OptionsMenu.Options.CameraSmoothing = value;
	}

	public void OnCameraAutoAirChange(int value)
	{
		OptionsMenu.Options.CameraAutoAirMove = value;
	}

	public void OnCameraVomitCameraChange(int value)
	{
		OptionsMenu.Options.CameraVomitCamera = value;
	}

	public void OnCameraAutoLookDown(int value)
	{
		OptionsMenu.Options.CameraAutoLookDown = value;
	}

	public void GetResolutions()
	{
		HashSet<Vector2I> resolutionsSet = new HashSet<Vector2I>();
		if (OptionsMenu.Options.Windowed != 0)
		{
			WindowSizeOption.Disabled = true;
			resolutionsSet.Add(DisplayServer.ScreenGetSize(DisplayServer.WindowGetCurrentScreen(0)));
		}
		else
		{
			WindowSizeOption.Disabled = false;
			resolutionsSet.Add(new Vector2I(OptionsMenu.Options.WindowSizeX, OptionsMenu.Options.WindowSizeY));
			resolutionsSet.Add(DisplayServer.WindowGetSize());
			for (int s = 0; s < DisplayServer.GetScreenCount(); s++)
			{
				var baseResolution = DisplayServer.ScreenGetSize(s);
				for (int i = 1; i <= 4; i++)
				{
					Vector2I resolution = baseResolution;
					resolution.X /= i;
					resolution.Y /= i;
					resolutionsSet.Add(resolution);
				}
			}
		}

		WindowSizeOption.Clear();
		Resolutions = resolutionsSet.ToArray();
		foreach (var resolution in Resolutions) {
			WindowSizeOption.AddItem(resolution.X + " x " + resolution.Y);
		}

		WindowSizeOption.Select(0);
	}

	public static void UpdatePlayerCameraSettings()
	{
		foreach (var cam in PlayerCamera.Instances)
		{
			cam.Value.DynamicFov = OptionsMenu.Options.CameraDynamicFov == 1;
			cam.Value.SettingEasing = OptionsMenu.Options.CameraSmoothing == 1;
			cam.Value.VomitCamera = OptionsMenu.Options.CameraVomitCamera == 1;
			cam.Value.AutoLookDown = OptionsMenu.Options.CameraAutoLookDown == 1;
			if (StageData.Instance.Style == StageData.StageStyle.SpecialStage)
			{
				cam.Value.SettingAutoAirCamera = OptionsMenu.Options.CameraAutoAirMove != 0;
			} else
			{
				cam.Value.SettingAutoAirCamera = OptionsMenu.Options.CameraAutoAirMove == 1;
			}
		}
	}

	void SetDefaultScrolling()
	{
		SetDefaultScrolling(this);		
	}

	void SetDefaultScrolling(Node node)
	{
		if (node is TabContainer tab)
		{
			tab.CurrentTab = 0;
		}
		foreach(var childNode in node.GetChildren())
		{
			SetDefaultScrolling(childNode);

		}
	}

	void PopulateKeyboardButtons()
	{
		InputActions = new string[OptionsMenu.Options.KeyboardInput.Count];
		InputButtons = new Button[OptionsMenu.Options.KeyboardInput.Count];
		for (int i = 0; i < OptionsMenu.Options.KeyboardInput.Count; i++)
		{
			var buttonKey = OptionsMenu.Options.KeyboardInput.Keys.ToArray()[i];
			if (!KbActionsToDescriptions.ContainsKey(buttonKey)) continue;

			var labelDuplicate = KeyboardLabelTemplate.Duplicate() as Label;
			labelDuplicate.Text = KbActionsToDescriptions[buttonKey];
			KeyboardButtonsContainer.AddChild(labelDuplicate);

			var buttonDuplicate = KeyboardButtonTemplate.Duplicate() as Button;
			KeyboardButtonsContainer.AddChild(buttonDuplicate);
			buttonDuplicate.Pressed += () => OnRemapKeyboardButton(buttonKey);

			InputActions[i] = buttonKey;
			InputButtons[i] = buttonDuplicate;
		}
		KeyboardLabelTemplate.Visible = false;
		KeyboardButtonTemplate.Visible = false;
	}
}
