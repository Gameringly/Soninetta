using Godot;
using System;

public partial class TouchJoystick : Node2D
{
	[Export] public Sprite2D Knob;
	[Export] public Sprite2D Center;
	[Export] public float DeadZone = 0.1f;
	[Export] public TouchScreenButton TouchButton;

	private bool Pressing = false;

	public void OnButtonUp()
	{
		Pressing = false;
        Knob.Visible = false;

        Input.ActionRelease("any_moveup");
        Input.ActionRelease("any_movedown");
        Input.ActionRelease("any_moveleft");
        Input.ActionRelease("any_moveright");
    }

	public void OnButtonDown()
	{
		Pressing = true;
        Knob.Visible = true;
	}

	public override void _Ready()
	{
		Knob.Visible = false;
		Pressing = false;
	}

    public override void _Process(double delta)
    {
		if (Pressing)
		{
			var radius = (float)TouchButton.Shape.Get("radius").AsDouble();

            Vector2 JoyPosition = TouchButton.GetLocalMousePosition() / radius;
			if (JoyPosition.Length() > 1)
			{
				JoyPosition = JoyPosition.Normalized();
			}

            Knob.GlobalPosition = Center.GlobalPosition + JoyPosition * radius;

			if (JoyPosition.X > 0)
			{
				Input.ActionPress("any_moveright", JoyPosition.X);
				Input.ActionRelease("any_moveleft");
			} else if (JoyPosition.X < 0)
            {
                Input.ActionPress("any_moveleft", -JoyPosition.X);
                Input.ActionRelease("any_moveright");
            }
			if (JoyPosition.Y > 0)
            {
                Input.ActionPress("any_movedown", JoyPosition.Y);
                Input.ActionRelease("any_moveup");
            }
            else if (JoyPosition.Y < 0)
            {
                Input.ActionPress("any_moveup", -JoyPosition.Y);
                Input.ActionRelease("any_movedown");
            }

        }
	}
}
