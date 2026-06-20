using System.Collections.Generic;
using System.Linq;
using Lumina.Excel.Sheets;

namespace ActionTimelineReplacement.Helpers;

internal static class ActionLookup
{
    private static IReadOnlyDictionary<uint, string>? _names;

    public static IReadOnlyDictionary<uint, string> Names => _names
        ??= Service.DataManager.GetExcelSheet<Action>()
            .Where(i => !string.IsNullOrEmpty(i.Name.ToString()))
            .ToDictionary(i => i.RowId, i => i.Name.ToString());

    public static (short start, short end, short hit, short cast) GetOriginal(uint id)
    {
        var act = Service.DataManager.GetExcelSheet<Action>().GetRow(id);
        return (
            (short)act.AnimationStart.RowId,
            (short)act.AnimationEnd.RowId,
            (short)act.ActionTimelineHit.RowId,
            (short)act.VFX.RowId);
    }
}