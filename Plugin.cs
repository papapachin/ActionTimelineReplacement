using System;
using System.Collections.Immutable;
using ActionTimelineReplacement.Models;
using ActionTimelineReplacement.Windows;
using Dalamud.Plugin;

namespace ActionTimelineReplacement;

public sealed class Plugin : IDalamudPlugin
{
    private readonly ImmutableArray<IDisposable> _disposables;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();

        Service.Model = pluginInterface.GetPluginConfig() as MainModel ?? new MainModel();

        _disposables = [new WindowManager(), Service.Model];
    }

    public void Dispose()
    {
        for (var i = _disposables.Length - 1; i >= 0; i--)
        {
            _disposables[i].Dispose();
        }
    }
}