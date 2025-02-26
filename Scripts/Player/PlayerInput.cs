using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerInput : Node
{
    //[Export] public PlayerCamera Camera;
    [Export] public PlayerController Player;

    public Vector2 LeftRawInput;
    public Vector3 LeftInput3D;
    public Vector2 CameraInput2D;

    [Export] public bool LockedInput = false;
    public double LeftInputTimedLock = 0;

    [Export] public bool TwodimensionalMode = true;
    [Export] public Vector3 Plane2DControlsY = Vector3.Up;
    [Export] public Vector3 Plane2DControlsX = Vector3.Forward;

    [Export] public string PlayerIdentifier = "kb";
    [Export] public bool ReadRealInput = true;

    [Export] public Vector2 MouseCameraSensitivity = new Vector2 ( 0.5f, 0.5f );
    [Export] public Vector2 JoypadCameraSensitivity = new Vector2 ( 1f, 1f );

    [Export] public bool Assist4Directions = false;
    [Export] public float DeadZone = 0.5f;

    // NPC input
    public Vector3 rawNpcLeftInput3D;
    public HashSet<string> NpcInputDown = new HashSet<string>();
    public HashSet<string> NpcInput = new HashSet<string>();
    public HashSet<string> NpcInputUp = new HashSet<string>();


    public override void _EnterTree()
    {
        ConfigureInputMapping();
    }

    public void NpcSetLeftInput(Vector3 newLeftRawInput)
    {
        if (ReadRealInput) return;
        this.rawNpcLeftInput3D = newLeftRawInput;
    }

    public void NpcPressButton(string buttonIdentifier)
    {
        if (ReadRealInput) return;
        NpcInputDown.Add(PlayerIdentifier + "_" + buttonIdentifier);
        NpcInput.Add(PlayerIdentifier + "_" + buttonIdentifier);
    }

    public void NpcReleaseButton(string buttonIdentifier)
    {
        if (ReadRealInput) return;
        NpcInputUp.Add(PlayerIdentifier + "_" + buttonIdentifier);
        NpcInput.Remove(PlayerIdentifier + "_" + buttonIdentifier);
    }


    public override void _Process(double delta)
    {
        if (!ReadRealInput)
        {
            NpcInputDown.Clear();
            NpcInputUp.Clear();
        }

        // Camera controls
        bool isOnMobile = OS.GetName() == "Android" || OS.GetName() == "iOS";

        if (ReadRealInput && !isOnMobile)
        {
            CameraInput2D = Input.GetVector(PlayerIdentifier + "_camleft", PlayerIdentifier + "_camright", PlayerIdentifier + "_camup", PlayerIdentifier + "_camdown") * JoypadCameraSensitivity;
            if (PlayerIdentifier == "kb" || PlayerIdentifier == "any")
            {
                CameraInput2D += Input.GetLastMouseVelocity() * (float)delta * MouseCameraSensitivity;
            }

            
        }

        PlayerCamera Camera = null;
        if (ReadRealInput)
        {
            LeftRawInput = Input.GetVector(PlayerIdentifier + "_moveleft", PlayerIdentifier + "_moveright", PlayerIdentifier + "_moveup", PlayerIdentifier + "_movedown");
            if (LeftRawInput.Length() < DeadZone)
            {
                LeftRawInput = Vector2.Zero;
            }
            Camera = PlayerCamera.Instances[Player.PlayerID];
        }

        LeftInputTimedLock -= delta;
        if (LeftInputTimedLock <= 0)
        {
            LeftInputTimedLock = 0;

            
            if (!TwodimensionalMode)
            {
                var rotationQuaternion = Quaternion.Identity.Normalized();
                
                if (Camera != null)
                {
                    rotationQuaternion = Camera.Basis.Orthonormalized().GetRotationQuaternion().Normalized();
                }
                // 3D movement controls
                Vector3 rawInput3D;
                if (ReadRealInput)
                {
                    //rawInput3D = rotationQuaternion * new Vector3(LeftRawInput.X, 0, LeftRawInput.Y);
                    
                    rawInput3D = rotationQuaternion * new Vector3(LeftRawInput.X, 0, LeftRawInput.Y);


                    //if (Camera.Mode == PlayerCamera.CameraMode.Constant || !Camera.VomitCamera)
                    {
                        var groundNormalRotation = new Quaternion(Camera.Basis.Y.Normalized(), Player.Grounded ? Player.GroundNormal.Normalized() : -Player.Gravity.Normalized()).Normalized();
                        rawInput3D = groundNormalRotation * rawInput3D;

                        var towardsOrAgainstCameraDot = rawInput3D.Normalized().Dot(Camera.GlobalBasis.Z);
                        if (LeftRawInput.Y < 0) towardsOrAgainstCameraDot *= -1;
                        if (towardsOrAgainstCameraDot < 0)
                        {
                            var rawInput3DZ = rawInput3D.Project(Camera.GlobalBasis.Z);
                            var velocityAsZInput = Player.LinearVelocity.Project(Camera.GlobalBasis.Z);
                            if (LeftRawInput.Y < 0) velocityAsZInput *= -1;
                            rawInput3D = rawInput3D.ProjectOnPlane(Camera.GlobalBasis.Z) - velocityAsZInput.Normalized() * rawInput3DZ.Length();
                        }
                    }

                }
                else
                {
                    rawInput3D = rawNpcLeftInput3D;
                }


                LeftInput3D = Vector3Utils.ProjectOnPlane(rawInput3D, Player.GroundNormal).Normalized() * rawInput3D.Length();

                if (Assist4Directions && Camera != null) 
                {
                    var assistDot = 0.995f;
                    foreach (var dir in new Vector3[]{
                        Camera.Basis.X,
                        -Camera.Basis.X,
                        Camera.Basis.Z,
                        -Camera.Basis.Z,
                        Camera.Basis.Y,
                        -Camera.Basis.Y,
                        })
                    {
                        if (LeftInput3D.Normalized().Dot(dir) > assistDot)
                        {
                            LeftInput3D = dir * LeftInput3D.Length();
                            break;
                        }
                    }
                }
            }
            else
            {
                // 2D movement controls
                Vector3 preRotationLeftInput2D;
                if (ReadRealInput) {
                    preRotationLeftInput2D = Plane2DControlsX * LeftRawInput.X + Plane2DControlsY * LeftRawInput.Y;
                }
                else
                {
                    preRotationLeftInput2D = (rawNpcLeftInput3D.Project(Plane2DControlsX) + rawNpcLeftInput3D.Project(Plane2DControlsY)).Normalized() * rawNpcLeftInput3D.Length();
                }
                if (Mathf.Abs(preRotationLeftInput2D.X) < DeadZone) preRotationLeftInput2D.X = 0;
                /*if (Mathf.Abs(preRotationLeftInput2D.Y) < DeadZone)*/ preRotationLeftInput2D.Y = 0;   
                // TODO: Treat grounded down input in 2D as "actionroll"
                var groundNormalRotation = new Quaternion(Plane2DControlsY.Normalized(), Player.Grounded ? Player.GroundNormal.Normalized() : -Player.Gravity.Normalized()).Normalized();
                LeftInput3D = groundNormalRotation * preRotationLeftInput2D;
            }
        } else
        {
            LeftInput3D = LeftInput3D.ProjectOnPlane(-Player.GroundNormal).Normalized() * LeftInput3D.Length();
        }
    }

    public bool IsActionPressed(string identifier, bool ignoreInputLock = false)
    {
        if (!ignoreInputLock && (Engine.TimeScale == 0 || LockedInput)) return false;
        identifier = OverrideAction(identifier);
        if (ReadRealInput)
        {
            return Input.IsActionPressed(PlayerIdentifier + "_" + identifier);
        } else
        {
            return NpcInput.Contains(PlayerIdentifier + "_" + identifier);
        }
    }

    public bool IsActionPressed(InputEvent @event, string identifier)
    {
        if (Engine.TimeScale == 0 || LockedInput) return false;
        identifier = OverrideAction(identifier);
        if (ReadRealInput)
        {
            return @event.IsActionPressed(Player.PlayerInput.PlayerIdentifier + "_" + identifier);
        }
        else
        {
            return NpcInput.Contains(PlayerIdentifier + "_" + identifier);
        }
    }

    public bool IsActionReleased(InputEvent @event, string identifier)
    {
        if (Engine.TimeScale == 0 || LockedInput) return false;
        identifier = OverrideAction(identifier);
        if (ReadRealInput)
        {
            return @event.IsActionReleased(Player.PlayerInput.PlayerIdentifier + "_" + identifier);
        }
        else
        {
            return !NpcInput.Contains(PlayerIdentifier + "_" + identifier);
        }
    }

    public bool IsActionJustPressed(string identifier, bool ignoreInputLock = false)
    {
        if (!ignoreInputLock && (Engine.TimeScale == 0 || LockedInput)) return false;
        identifier = OverrideAction(identifier);
        if (ReadRealInput)
        {
            return Input.IsActionJustPressed(PlayerIdentifier + "_" + identifier);
        }
        else
        {
            return NpcInputDown.Contains(PlayerIdentifier + "_" + identifier);
        }
    }

    public bool IsActionJustReleased(string identifier)
    {
        if (Engine.TimeScale == 0 || LockedInput) return false;
        identifier = OverrideAction(identifier);
        if (ReadRealInput)
        {
            return Input.IsActionJustReleased(PlayerIdentifier + "_" + identifier);
        }
        else
        {
            return NpcInputUp.Contains(PlayerIdentifier + "_" + identifier);
        }
    }

    private static bool inputConfigured = false;
    private static void ConfigureInputMapping()
    {
        if (inputConfigured) return;
        inputConfigured = true;
        List<Tuple<string,JoyButton>>  actionButtons = new List<Tuple<string, JoyButton>> { 
            Tuple.Create("actionjump", JoyButton.A),
            Tuple.Create("actionhoming", JoyButton.X),
            Tuple.Create("actionsuper", JoyButton.Back),
            Tuple.Create("actionstomp", JoyButton.B),
            Tuple.Create("quickstep_l", JoyButton.LeftShoulder),
            Tuple.Create("quickstep_r", JoyButton.RightShoulder),
        };
        List<Tuple<string, JoyAxis, float>> actionAxes = new List<Tuple<string, JoyAxis, float>> {
            Tuple.Create("actionroll", JoyAxis.TriggerLeft, 1f),
            Tuple.Create("actiondash", JoyAxis.TriggerRight, 1f),

            Tuple.Create("moveleft", JoyAxis.LeftX, -1f),
            Tuple.Create("moveright", JoyAxis.LeftX, 1f),
            Tuple.Create("moveup", JoyAxis.LeftY, -1f),
            Tuple.Create("movedown", JoyAxis.LeftY, 1f),

            Tuple.Create("camleft", JoyAxis.RightX, -1f),
            Tuple.Create("camright", JoyAxis.RightX, 1f),
            Tuple.Create("camup", JoyAxis.RightY, -1f),
            Tuple.Create("camdown", JoyAxis.RightY, 1f),
        };

        for (int gamepad = 0; gamepad < 4; gamepad++)
        {
            var identifier = "pad" + gamepad.ToString();
            foreach (var actionButton in actionButtons)
            {
                var inputName = actionButton.Item1;
                var actionName = identifier + "_" + inputName;
                if (!InputMap.HasAction(actionName))
                {
                    InputMap.AddAction(actionName);
                    var ev = new InputEventJoypadButton();
                    ev.ButtonIndex = actionButton.Item2;
                    ev.Device = gamepad;
                    InputMap.ActionAddEvent(actionName, ev);
                }
            }

            foreach (var actionAxis in actionAxes)
            {
                var inputName = actionAxis.Item1;
                var actionName = identifier + "_" + inputName;
                if (!InputMap.HasAction(actionName))
                {
                    InputMap.AddAction(actionName);
                    var ev = new InputEventJoypadMotion();
                    ev.Axis = actionAxis.Item2;
                    ev.AxisValue = actionAxis.Item3;
                    ev.Device = gamepad;
                    InputMap.ActionAddEvent(actionName, ev);
                }
            }
        }

        foreach (var action in InputMap.GetActions())
        {
            if (InputMap.ActionGetEvents(action).Count > 0)
            {
                var e = InputMap.ActionGetEvents(action)[0];
                
            }
        }
    }

    string OverrideAction(string identifier)
    {
        // This function is to swap quick step directions depending on the direction of camera. Other overrides can go here too.

        if (identifier != "quickstep_l" && identifier != "quickstep_r") return identifier;
        PlayerCamera Camera = null;
        Camera = PlayerCamera.Instances.GetValueOrDefault(Player.PlayerID, null);
        if (Camera == null) return identifier;
        var dot = (-Player.Basis.Z).Dot(-Camera.Basis.Z);
        if (dot < 0)
        {
            switch (identifier)
            {
                case "quickstep_l":
                    return "quickstep_r";
                case "quickstep_r":
                    return "quickstep_l";
                default:
                    break;
            }
        }

        return identifier;        
    }
}
