using System.Collections.Generic;
using System.Linq;
using Dalamud.Bindings.ImGui;
using Lumina.Excel.Sheets;

namespace ActionTimelineReplacement.Helpers;

internal static class ActionLookup
{
    private static IReadOnlyDictionary<uint, string>? _names;

    public static IReadOnlyDictionary<uint, string> Names => _names
        ??= Service.DataManager.GetExcelSheet<Action>()
            .Where(i => !string.IsNullOrEmpty(i.Name.ToString()))
            .ToDictionary(i => i.RowId, i => i.Name.ToString());

    public static (ushort start, ushort end, ushort hit, ushort cast) GetOriginal(uint id)
    {
        var act = Service.DataManager.GetExcelSheet<Action>().GetRow(id);
        return (
            (ushort)act.AnimationStart.RowId,
            (ushort)act.AnimationEnd.RowId,
            (ushort)act.ActionTimelineHit.RowId,
            (ushort)act.VFX.RowId);
    }
}