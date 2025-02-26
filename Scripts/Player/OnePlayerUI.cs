using Godot;
using System;

public partial class OnePlayerUI : Control
{
    [Export] public int PlayerID;
    [Export] public Node2D HomingIcon;
    [Export] public Node2D HomingOffscreenIcon;
    [Export] public Control BoostUIContainer;
    [Export] public ProgressBar BoostMeter;

    public static Godot.Collections.Dictionary<int, OnePlayerUI> Instances = new Godot.Collections.Dictionary<int, OnePlayerUI> (); // Key: PlayerID

    private ActionBoost ActionBoost;

    public override void _EnterTree()
    {
        Instances.Add(PlayerID, this);
    }

    public override void _ExitTree()
    {
        Instances.Remove(PlayerID);
    }


    public override void _Process(double delta)
    {
        HandleBoostBar();
    }

    void HandleBoostBar()
    {
        if (ActionBoost == null)
        {
            ActionBoost = PlayerController.Instances[PlayerID].GetNodeOrNull<ActionBoost>("./PlayerControl/Actions/ActionBoost");
        }
        bool showBoostBar = ActionBoost != null && ActionBoost.ProcessMode != ProcessModeEnum.Disabled && !ActionBoost.RequireSuperTransformation && !StageData.Instance.IsFinished && ActionBoost.ShowBoostMeter;
        BoostUIContainer.Visible = showBoostBar;
        if (!showBoostBar)
        {
            return;
        }
        if (ActionBoost != null)
        {
            BoostMeter.Value = ActionBoost.Fuel * 100;
        }
    }
}
