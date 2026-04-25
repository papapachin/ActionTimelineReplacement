using Dalamud.Plugin;
using System;
using System.Collections.Immutable;
using ActionTimelineReplacement.Configurations;
using ActionTimelineReplacement.Hookers;
using ActionTimelineReplacement.Windows;

namespace ActionTimelineReplacement;

public sealed class Plugin : IDalamudPlugin
{
    private readonly ImmutableArray<IDisposable> _disposables;
    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        //初始化服务，主要用于ID
        pluginInterface.Create<Service>();

        //用于读取Config
        Service.Config = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        //添加各种小模组
        _disposables = [new WindowManager()];

        Methods.SetupActions(ReplacementsManager.AllActionIds);
    }

    public void Dispose()
    {
        Methods.SetupActions(ReplacementsManager.AllActionIds, true);
        Service.Config.Save();
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
    }
}