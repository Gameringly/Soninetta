using Godot;
using System;

public partial class BossEndCamera : Node
{
    [Export] public AudioStreamPlayer3D SoundPlayer;
    [Export] public AudioStream CameraSound;
    [Export] public Node3D[] CameraPoints;
    [Export] public float UnscaledSecondsPerCameraPoint = 1;
    private float CameraPointTimer = 0;
    [Export] public float TimeScale = 0.0001f;

    private int currentCameraPoint = -1;

    public override void _Ready()
    {
        this.ProcessMode = ProcessModeEnum.Disabled;
        currentCameraPoint = -1;
        CameraPointTimer = 0;
    }

    public override void _Process(double delta)
    {

        //GD.Print(currentCameraPoint, " ", CameraPointTimer);
        if (CameraPointTimer > UnscaledSecondsPerCameraPoint || currentCameraPoint == -1)
        {
            currentCameraPoint++;
            CameraPointTimer = 0;
            SoundPlayer.Stream = this.CameraSound;
            SoundPlayer.Play(0);
            Engine.TimeScale = this.TimeScale;
            return;
        }
        if (currentCameraPoint >= CameraPoints.Length) {
            this.ProcessMode = ProcessModeEnum.Disabled;
            Engine.TimeScale = 1;
            foreach (var cam in PlayerCamera.Instances)
            {
                cam.Value.Point = null;
                cam.Value.Mode = PlayerCamera.CameraMode.Free;
            }
            return;
        }
        CameraPointTimer += (float)delta / TimeScale;

        Node3D currentPoint = CameraPoints[currentCameraPoint];
        foreach(var cam in PlayerCamera.Instances)
        {
            cam.Value.Point = currentPoint;
            cam.Value.Mode = PlayerCamera.CameraMode.Point;
        }
    }
}
