using System;
using System.Linq;
using ActionTimelineReplacement.Hookers;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace ActionTimelineReplacement.Models;

public sealed class ActionOffsetModel :
    BaseModel<ushort>,
    IDisposable
{
    private readonly ActionOffsetAction _action;
    private readonly IntModel _priority;
    private readonly BoolModel[] _enable;

    public ActionOffsetModel(ushort data,
        ActionOffsetAction action,
        IntModel priority,
        BoolModel[] enable) : base(data)
    {
        _action = action;
        _priority = priority;
        _enable = enable;
        
        priority.OnChanged -= action.UpdatePriorityOrEnable;
        priority.OnChanged += action.UpdatePriorityOrEnable;
        foreach (var boolModel in _enable)
        {
            boolModel.OnChanged -= action.UpdatePriorityOrEnable;
            boolModel.OnChanged += action.UpdatePriorityOrEnable;
        }

        _action.RegisterModel(this);
        _action.UpdateValue(this);
    }

    public int Priority => _priority.Value;
    public bool Enable => _enable.All(i => i.Value);
    
    public override void Changed()
    {
        _action.UpdateValue(this);
    }
    
    protected override bool DrawImplementation()
    {
        var result = false;
        ImGui.SetNextItemWidth(60 * ImGuiHelpers.GlobalScale);
        if (ImGui.DragUShort(string.Empty, ref Data))
        {
            result = true;
        }
        
        ImGui.SameLine();
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            if (ImGui.Button(FontAwesomeIcon.Reply.ToIconString()))
            {
                result = true;
                Data = _action.DefaultValue;
            }
        }

        return result;
    }

    public void Dispose()
    {
        _action.UnregisterModel(this);
    }
}