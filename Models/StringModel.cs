using ActionTimelineReplacement.Helpers;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace ActionTimelineReplacement.Models;

public class StringModel(string data, string name) : BaseModel<string>(data)
{
    protected override bool DrawImplementation()
    {
        using (ImRaii.PushFont(Fonts.GetFont(20)))
        {
            var width = ImGui.CalcTextSize(Data).X + ImGui.GetStyle().FramePadding.X * 2;
            var x = ImGui.GetWindowWidth() / 2 - width / 2;
            ImGui.SetCursorPosX(x);

            ImGui.SetNextItemWidth(width);
            return ImGui.InputText("##" + name, ref Data, 256);
        }
    }
}