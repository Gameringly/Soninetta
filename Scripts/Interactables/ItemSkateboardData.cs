using Godot;
using System;

public partial class ItemSkateboardData : Node
{
    [Export] public PackedScene SkateboardModel;
    [Export] public Vector3 SkateboardStandingPoint = Vector3.Zero;
    [Export] public bool CanBoost = false;
    [Export] public bool FloatOnWater = false;

    public override void _Ready()
    {
        var parentItem = GetNodeOrNull<CollectableItem>("..");
        if (parentItem != null)
        {
            parentItem.OnItemCollected += this.OnItemCollected;
        }
    }

    public void OnItemCollected(PlayerController collectorPlayer)
    {
        var actionSkate = collectorPlayer.GetNodeOrNull<ActionSkate>("./PlayerControl/Actions/ActionSkate");
        if (actionSkate != null)
        {
            actionSkate.SkateboardModel = this.SkateboardModel;
            actionSkate.SkateboardStandingPoint = this.SkateboardStandingPoint;
            actionSkate.CanBoost = this.CanBoost;
            actionSkate.FloatOnWater = this.FloatOnWater;
        }
    }
}
