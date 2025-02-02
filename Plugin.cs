using Dalamud.Game;
using Dalamud.Hooking;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using Newtonsoft.Json;
using SocialList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace ActionTimelineReplacement
{
    public class ActionTimelineReplacement
    {
        public int AnimationStart { get; set; }
        public int AnimationEnd { get; set; }
        public int ActionTimelineHit { get; set; }
    }
    public unsafe sealed class Plugin : IDalamudPlugin
    {
        public string Name => "ActionTimelineReplacement";
        [PluginService]
        internal static IDalamudPluginInterface PluginInterface { get; set; }
        [PluginService]
        internal static ISigScanner Scanner { get; set; }
        [PluginService]
        internal static ICommandManager CommandManager { get; set; }
        [PluginService]
        internal static IClientState ClientState { get; set; }
        [PluginService]
        internal static IChatGui ChatGui { get; set; }
        [PluginService]
        internal static IDataManager DataManager { get; set; }
        [PluginService]
        internal static IGameInteropProvider GameInteropProvider { get; set; }
        [PluginService]
        internal static IFramework Framework { get; set; }
        [PluginService]
        public static IPluginLog PluginLog { get; set; }

        private SortedDictionary<int, ActionTimelineReplacement> ActionTimelineReplacements = new();
        public Plugin()
        {
            PluginInterface.Create<Service>();
            Service.GameInteropProvider.InitializeFromAttributes(this);
            var configDirPath = Path.Combine(PluginInterface.AssemblyLocation.DirectoryName, "Presets");
            if (!Directory.Exists(configDirPath))
            {
                Directory.CreateDirectory(configDirPath);

                ActionTimelineReplacements.Clear();
            }
            var ActionTimelines = Service.DataManager.GetExcelSheet<Lumina.Excel.Sheets.ActionTimeline>();
            var Actions = Service.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Action>();
            foreach (var file in Directory.GetFiles(configDirPath, "*.json"))
            {
                try {
                    PluginLog.Info($"Loading ${file}");
                    var configs = JsonConvert.DeserializeObject<SortedDictionary<int, ActionTimelineReplacement>>(File.ReadAllText(file));
                    if (configs is not null)
                        foreach (var config in configs) {
                            var action = Actions?.GetRow((uint)config.Key);
                            var animationEnd = ActionTimelines?.GetRow((uint)config.Value.AnimationEnd);
                            var actionTimelineHit = ActionTimelines?.GetRow((uint)config.Value.ActionTimelineHit);
                            if (this.ActionTimelineReplacements.TryAdd(config.Key, config.Value)) {
                                PluginLog.Info($"Action:{action?.Name}({config.Key}):AnimationEnd->{animationEnd?.Key}({config.Value.AnimationEnd}),ActionTimelineHit->{actionTimelineHit?.Key}({config.Value.ActionTimelineHit})");
                            }
                            else {
                                PluginLog.Info($"FAIL:Action:{action?.Name}({config.Key}):AnimationEnd->{animationEnd?.Key}({config.Value.AnimationEnd}),ActionTimelineHit->{actionTimelineHit?.Key}({config.Value.ActionTimelineHit})");
                            }
                        }
                }
                catch (Exception ex) {
                    PluginLog.Error($"Can not load {file}");
                    PluginLog.Error(ex.ToString());
                }
            }
            GetActionDataHook?.Enable();
            PluginLog.Information($"GetActionDataHook is null: {GetActionDataHook == null}");
        }

        private delegate IntPtr GetActionDataDelegate(uint actionId);

        [Signature("E8 ?? ?? ?? ?? 80 FB 12", DetourName = nameof(GetActionDataDetour))]
        private Hook<GetActionDataDelegate> GetActionDataHook;
        private unsafe IntPtr GetActionDataDetour(uint actionId)
        {
            if (this.ActionTimelineReplacements.TryGetValue((int)actionId, out var replacement))
            {
                var ret = GetActionDataHook.Original(actionId);
                Marshal.WriteInt16(ret + 0xC, (short)replacement.ActionTimelineHit);
                Marshal.WriteInt16(ret + 0x1E, (short)replacement.AnimationEnd);
                Marshal.WriteInt16(ret + 0x22, (short)replacement.AnimationStart);
                return ret;
            }
            return GetActionDataHook.Original(actionId);
        }
        public void Dispose()
        {
            GetActionDataHook?.Dispose();
        }

        public void DrawConfigUI()
        {
            //ConfigWindow.IsOpen = true;
        }
    }
}
