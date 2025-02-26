using Godot;
using Godot.Collections;
using System;

public partial class ButtonPromptTextLabel : RichTextLabel
{
	public PlayerController Player;
	[Export] public ColorRect ColorRect;
	public override void _Ready()
	{
		this.Visible = false;

	}

	public void OnBodyEnter(Node3D body)
	{
		var player = body.GetNodeOrNull<PlayerController>(".");
		if (player != null && !player.NpcPartnerControl.IsNpc)
		{
			this.Player = player;
			this.Visible = true;

            HandleInputTag();
            ColorRect.Size = this.Size;
            ColorRect.GlobalPosition = this.GlobalPosition;
            ColorRect.SetAnchorsPreset(LayoutPreset.FullRect, true);
        }
		
	}

	public void OnBodyExit(Node3D body)
	{
		var player = body.GetNodeOrNull<PlayerController>(".");
		if (player == this.Player)
		{
			this.Player = null;
			this.Visible = false;
		}

	}

	public void HandleInputTag()
	{
		string startTag = "[input]";
		string endTag = "[/input]";
		while (true)
		{
			var startTagPos = this.Text.FindN(startTag);
			var endTagPos = this.Text.FindN(endTag);
			if (startTagPos == -1 && endTagPos == -1)
			{
				return;
			}
			if (startTagPos != -1 && endTagPos == -1)
			{
				throw new ArgumentException("Unterminated [input] tag");
			}
			if (startTagPos == -1 && endTagPos != -1)
			{
				throw new ArgumentException("Found closing [/input] tag without opening [/input]");
			}
			var preText = this.Text.Substring(0, startTagPos);
			var insideText = this.Text.Substring(startTagPos + startTag.Length, endTagPos - (startTagPos + startTag.Length));
			var postText = this.Text.Substring(endTagPos + endTag.Length);

			
			string inputText = GetInputText(insideText);

			this.Text = preText + inputText + postText;
			this.AppendText(preText);
		}

		string GetInputText(string action)
		{
			string keyboardText = "";
			string padText = "";

			// TODO: Android version

			var identifier = Player.PlayerInput.PlayerIdentifier;
			if (identifier == "any" || identifier.StartsWith("pad"))
			{
				var events = InputMap.ActionGetEvents(identifier + "_" + action);
				//foreach(var ev in events) {
				if (events.Count > 0) {
					var ev = events[0];
					int padNum = 0;
					if (identifier.StartsWith("pad"))
					{
						var padNumStr = identifier.Substring(3).Split("_")[0];
						padNum = Int32.Parse(padNumStr);
					}

					var joyName = Input.GetJoyName(padNum).ToLower();
					if (joyName.Length == 0) return "";

					var eventText = ev.AsText();
					var eventTextSplit = eventText.Split(new string[] { "(", ")", "," }, StringSplitOptions.TrimEntries);

					var numberName = eventTextSplit[0];
					var genericName = eventTextSplit[1];
					var playstationName = eventTextSplit[2];
					var xboxName = eventTextSplit[3];
					var nintendoName = eventTextSplit[4];


					// Please refer to https://github.com/mdqinc/SDL_GameControllerDB/blob/master/gamecontrollerdb.txt for controller names.
					if (joyName.Contains("xbox") || joyName.Contains("xinput"))
					{
						var xboxText = xboxName.Split(" ")[1];  // First part contains "Xbox"
						switch(xboxText.ToUpper())
						{
							case "A":
								padText = "[color=green]A[/color]";
								break;
							case "B":
								padText = "[color=red]B[/color]";
								break;
							case "X":
								padText = "[color=blue]X[/color]";
								break;
							case "Y":
								padText = "[color=yellow]Y[/color]";
								break;
							default:
								padText = xboxText;
								break;
						}
					}
					else if (joyName.Contains("sony") || joyName.Contains("dualshock") || joyName.Contains("playstation") || joyName.Contains("ps"))
					{
						var psName = playstationName.Split(" ")[1];  // First part contains "Sony";
						switch(psName.ToLower())
						{
							case "cross":
								padText = "[color=blue]×[/color]";
								break;
							case "triangle":
								padText = "[color=green]△[/color]";
								break;
							case "circle":
								padText = "[color=red]○[/color]";
								break;
							case "square":
								padText = "[color=pink]□[/color]";
								break;
							default:
								padText = psName;
								break;
						}
					}
					else if (joyName.Contains("nintendo") || joyName.Contains("wii") || joyName.Contains("gamecube") || joyName.Contains("switch"))
					{
						padText = nintendoName.Split(" ")[1];  // First part contains "Nintendo";
					}
					else
					{
						padText = genericName.Split(" ")[0] + " Button (" + numberName.Split(" ", StringSplitOptions.TrimEntries)[2] + ")";   // Second part contains "Action"
					}
				}
			}
			if (identifier == "any" || identifier == "kb")
			{
				keyboardText = OptionsMenu.Options.KeyboardInput["kb_" + action].ToString();
			}

			if (keyboardText.Length > 0 && padText.Length > 0)
			{
				return keyboardText + "/" + padText;
			} else if (keyboardText.Length > 0)
			{
				return keyboardText;
			} else if (padText.Length > 0)
			{
				return padText;
			}

			// Something went wrong here. This means no input mapping.
			GD.PrintErr("Couldn't find mapping for action {" + action + "} for neither keyboard nor joypad. Returning action name as a fallback.");
			return action;
		}
	}
}
