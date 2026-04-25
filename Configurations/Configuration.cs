using System;
using System.Collections.Generic;
using Dalamud.Configuration;

namespace ActionTimelineReplacement.Configurations;

[Serializable]
public class Configuration: IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool EnableReplacement = true;

    public bool AdvancedMode = false;
    public List<ActionTimelineReplacementSet> ActionTimelineReplacements { get; set; } = [];

    internal void Save()
    {
        Service.PluginInterface.SavePluginConfig(this);
    }
}