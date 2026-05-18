using System;
using Dalamud.Plugin.Services;

namespace ActionTimelineReplacement.Hookers;

public sealed record ActionOffsetDefinition
{
    private readonly Func<IDataManager, ushort, ReadOnlyMemory<byte>> _lookup;
    public string Name { get; }
    public uint Offset { get; }

    private ActionOffsetDefinition(string name, uint offset, Func<IDataManager, ushort, ReadOnlyMemory<byte>> lookup)
    {
        _lookup = lookup;
        Name = name;
        Offset = offset;
    }
    
    public ReadOnlyMemory<byte> LookUp(ushort key)
    {
        return _lookup(Service.DataManager, key);
    }
    
    public static ActionOffsetDefinition CastVfx { get; } = new("Cast Vfx", 10, (dataManager, key) =>
    {
        return dataManager.GetExcelSheet<Lumina.Excel.Sheets.ActionCastVFX>()[key].VFX.Value.Location;
    });
    public static ActionOffsetDefinition AnimationStart { get; } = new("Start timeline", 36, (dataManager, key) =>
    {
        return dataManager.GetExcelSheet<Lumina.Excel.Sheets.ActionCastTimeline>()[key].VFX.Value.Location;
    });
    public static ActionOffsetDefinition AnimationEnd { get; } = new("End timeline", 32, (dataManager, key) =>
    {
        return dataManager.GetExcelSheet<Lumina.Excel.Sheets.ActionTimeline>()[key].Key;
    });
    public static ActionOffsetDefinition ActionTimelineHit { get; } = new("Hit timeline", 12, (dataManager, key) =>
    {
        return dataManager.GetExcelSheet<Lumina.Excel.Sheets.ActionTimeline>()[key].Key;
    });
}