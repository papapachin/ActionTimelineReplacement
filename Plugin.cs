using Dalamud.Hooking;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
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
        public int AnimationEnd { get; set; }
        public int ActionTimelineHit { get; set; }
    }
    public unsafe sealed class Plugin : IDalamudPlugin
    {
        public string Name => "ActionTimelineReplacement";

        private SortedDictionary<int, ActionTimelineReplacement> ActionTimelineReplacements = new();
        public unsafe Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Service>();
            SignatureHelper.Initialise(this, true);
            var configDirPath = Path.Combine(pluginInterface.AssemblyLocation.DirectoryName, "Presets");
            if (!Directory.Exists(configDirPath))
            {
                Directory.CreateDirectory(configDirPath);
                var example = new ActionTimelineReplacement() {AnimationEnd = 5146, ActionTimelineHit = 1875 };
                ActionTimelineReplacements.Add(25757,example);
                File.WriteAllText(Path.Combine(configDirPath,"example.json"),JsonConvert.SerializeObject(ActionTimelineReplacements));
                ActionTimelineReplacements.Clear();
            }
            var ActionTimelines = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.ActionTimeline>();
            var Actions = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>();
            foreach (var file in Directory.GetFiles(configDirPath, "*.json"))
            {
                PluginLog.Log($"Loading ${file}");
                var configs = JsonConvert.DeserializeObject<SortedDictionary<int, ActionTimelineReplacement>>(File.ReadAllText(file));
                if (configs is not null)
                    foreach (var config in configs)
                    {
                        var action = Actions.GetRow((uint)config.Key);
                        var animationEnd = ActionTimelines.GetRow((uint)config.Value.AnimationEnd);
                        var actionTimelineHit = ActionTimelines.GetRow((uint)config.Value.ActionTimelineHit);
                        PluginLog.Log($"Action:{action?.Name}({config.Key}):AnimationEnd->{animationEnd?.Key}({config.Value.AnimationEnd}),ActionTimelineHit->{actionTimelineHit?.Key}({config.Value.ActionTimelineHit})");
                        if (this.ActionTimelineReplacements.TryAdd(config.Key, config.Value)) {
                            PluginLog.Log($"Action:{action?.Name}({config.Key}):AnimationEnd->{animationEnd?.Key}({config.Value.AnimationEnd}),ActionTimelineHit->{actionTimelineHit?.Key}({config.Value.ActionTimelineHit})");
                        }
                        else {
                            PluginLog.Log($"FAIL:Action:{action?.Name}({config.Key}):AnimationEnd->{animationEnd?.Key}({config.Value.AnimationEnd}),ActionTimelineHit->{actionTimelineHit?.Key}({config.Value.ActionTimelineHit})");
                        }
                    }
            }
            GetActionDataHook?.Enable();
        }

        private delegate IntPtr GetActionDataDelegate(uint actionId);
        [Signature("E8 ?? ?? ?? ?? 80 FB 12", DetourName = nameof(GetActionDataDetour))]
        private Hook<GetActionDataDelegate> GetActionDataHook;
        private unsafe IntPtr GetActionDataDetour(uint actionId)
        {
            if (this.ActionTimelineReplacements.TryGetValue((int)actionId, out var replacement))
            {
                var ret = GetActionDataHook.Original(actionId);
                Marshal.WriteInt16(ret + 0x1E, (short)replacement.AnimationEnd);
                Marshal.WriteInt16(ret + 0x1C, (short)replacement.ActionTimelineHit);
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
