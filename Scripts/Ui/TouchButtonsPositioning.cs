using Godot;
using System;

public partial class TouchButtonsPositioning : Node
{
    [Export] public Node2D[] BottomLeft = new Node2D[0];
    [Export] public Node2D[] BottomRight = new Node2D[0];
    [Export] public Node2D[] TopLeft = new Node2D[0];
    [Export] public Node2D[] TopRight = new Node2D[0];

    public override void _Process(double delta)
    {
        // It's not in _Ready() because even on mobile screen can be resized at any point (for example when doing split screen)
        foreach (var element in BottomLeft)
        {
            element.GlobalPosition = new Vector2(GetViewport().GetVisibleRect().Position.X, GetViewport().GetVisibleRect().End.Y);
        }
        foreach (var element in BottomRight)
        {
            element.GlobalPosition = GetViewport().GetVisibleRect().End;
        }
        foreach (var element in TopLeft)
        {
            element.GlobalPosition = GetViewport().GetVisibleRect().Position;
        }
        foreach (var element in TopRight)
        {
            element.GlobalPosition = new Vector2(GetViewport().GetVisibleRect().End.X, GetViewport().GetVisibleRect().Position.Y);
        }
    }
}
