using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace MarketAnalysisPlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Plugin Plugin;

    public ConfigWindow(Plugin plugin) : base(
        "Market Analysis Configuration",
        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse)
    {
        Size = new Vector2(400, 200);
        SizeCondition = ImGuiCond.Always;

        Plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.TextUnformatted("Market Analysis Configuration");
        ImGui.Separator();

        ImGui.TextUnformatted("Configuration options will be added here.");

        ImGui.Separator();

        if (ImGui.Button("Save and Close"))
        {
            IsOpen = false;
        }
    }
}
