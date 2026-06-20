using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace ActionTimelineReplacement.Windows;

public sealed class ConfigWindow : Window
{
    public ConfigWindow()
        : base("ActionTimeline Replacement v" + typeof(ConfigWindow).Assembly.GetName().Version,
            ImGuiWindowFlags.NoScrollbar)
    {
        SizeCondition = ImGuiCond.FirstUseEver;
        Size = new Vector2(960, 540);
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(480, 360),
            MaximumSize = new Vector2(5000, 5000),
        };
    }

    public override void Draw() => Service.Model.Draw();
}