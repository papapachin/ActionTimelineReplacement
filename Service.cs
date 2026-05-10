using ActionTimelineReplacement.Models;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace ActionTimelineReplacement;

internal class Service
{
    internal static MainModel Model { get; set; } = null!;

    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; set; } = null!;
    [PluginService] internal static ISigScanner Scanner { get; set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; set; } = null!;
    [PluginService] internal static ITextureProvider Texture { get; set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; set; } = null!;
    [PluginService] public static IPluginLog Log { get; set; } = null!;
}