using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class ApplyWorldEnvironmentSettings : WorldEnvironment
{
    public override void _Ready()
    {
        ApplySettings();
    }

    void ApplySettings()
    {

        this.Environment.SsrEnabled = false;
        this.Environment.SsaoEnabled = false;
        this.Environment.SsilEnabled = false;
        this.Environment.SdfgiEnabled = false;
        this.Environment.GlowEnabled = false;
        this.Environment.AdjustmentEnabled = false;
    }
}
