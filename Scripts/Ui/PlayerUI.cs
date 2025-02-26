using Godot;
using System;

public partial class PlayerUI : Control
{
    [Export] public RichTextLabel UiText;
    public static double CurrentTime = 0;
    [Export] public Color NoRingsColor;
    // Red rings
    [Export] public TextureRect[] RedRingsIcons = new TextureRect[0];
    [Export] public string[] RedRingsIdentifiers = new string[0];
    [Export] public Texture2D RedRingYesIcon;
    [Export] public Texture2D RedRingNoIcon;
    [Export] public Control MainUI;
    [Export] public FinishScreen FinishUI;    // Unrelated to Finland.

    public static PlayerUI Instance;

    public override void _EnterTree()
    {
        Instance = this;
        SetFinishMode(false);
    }

    public override void _ExitTree()
    {
        Instance = null;
    }

    public override void _Process(double delta)
    {
        string timeText = "[b]Time: [/b]" + TimeSpan.FromSeconds(CurrentTime).ToString(@"mm\:ss\:ff");
        string ringsText = "[b]Rings: [/b]";
        string bonusText = "[b]Bonus: [/b]";

        for (int i = 0; i < PlayerController.Instances.Count; i++)
        {
            if (PlayerController.Instances[i].NpcPartnerControl.IsNpc) continue;
            if (i > 0)
            {
                ringsText += " | ";
                bonusText += " | ";
            }
            var player = PlayerController.Instances[i]; // TODO: Multiplayer UI
            var ringsCount = player.PlayerInventory.GetItemCount("rings");
            if (ringsCount == 0)
            {
                var colorString = ((int)NoRingsColor.R * 255).ToString("X2") + ((int)NoRingsColor.G * 255).ToString("X2") + ((int)NoRingsColor.B * 255).ToString("X2");
                ringsText += "[color=" + colorString + "]";
            }
            ringsText += ringsCount;
            if (ringsCount == 0)
            {
                ringsText += "[/color]";
            }

            var sphereBonus = player.PlayerInventory.GetItemCount("bluespherebonus");
            bonusText += sphereBonus.ToString() + "x";
        }

        UiText.Text = "";
        if (StageData.Instance.Style != StageData.StageStyle.HubStage)
        {
            UiText.Text += timeText;
        }
        if (StageData.Instance.Style != StageData.StageStyle.SpecialStage)
        {
            UiText.Text += "\n" + ringsText;
        } else
        {
            UiText.Text += "\n" + bonusText;
        }
        

        UpdateRedRingsIcons();
    }

    private void UpdateRedRingsIcons()
    {
        var player = PlayerController.Instances[0]; // TODO: Multiplayer UI
        for (int i = 0; i < RedRingsIcons.Length && i < RedRingsIdentifiers.Length; i++)
        {
            if (!StageData.Instance.HasRedRings)
            {
                RedRingsIcons[i].Visible = false;
                continue;
            }

            var identifier = RedRingsIdentifiers[i];
            Texture2D targetIcon = RedRingNoIcon;
            if (player.PlayerInventory.GetItemCount(identifier) > 0)
            {
                targetIcon = RedRingYesIcon;
            }
            if (RedRingsIcons[i].Texture != targetIcon)
            {
                RedRingsIcons[i].Texture = targetIcon;
            }
        }
    }

    public void SetFinishMode(bool finishMode)
    {
        if (finishMode)
        {
            MainUI.Visible = false;
            FinishUI.Visible = true;
            FinishUI.ProcessMode = ProcessModeEnum.Inherit;
        }
        else
        {
            MainUI.Visible = true;
            FinishUI.Visible = false;
            FinishUI.ProcessMode = ProcessModeEnum.Disabled;
        }
    }
}
