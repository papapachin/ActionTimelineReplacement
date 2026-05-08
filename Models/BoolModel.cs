using Dalamud.Bindings.ImGui;

namespace ActionTimelineReplacement.Models;

public class BoolModel(bool data, string name) : BaseModel<bool>(data)
{
    protected override bool DrawImplementation()
    {
        return ImGui.Checkbox(name, ref Data);
    }
}