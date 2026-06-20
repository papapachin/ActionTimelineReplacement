using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;

namespace ActionTimelineReplacement.Models;

public class IntModel(int data, string name) : BaseModel<int>(data)
{
    protected override bool DrawImplementation()
    {
        ImGui.SetNextItemWidth(50 * ImGuiHelpers.GlobalScale);
        return ImGui.DragInt(name, ref Data);
    }
}