using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialList
{
    internal class Service
    {
        [PluginService]
        [RequiredVersion("1.0")]
        internal static DalamudPluginInterface PluginInterface { get; set; }
        [PluginService]
        [RequiredVersion("1.0")]
        internal static ISigScanner Scanner { get; set; }
        [PluginService]
        [RequiredVersion("1.0")]
        internal static ICommandManager CommandManager { get; set; }
        [PluginService]
        [RequiredVersion("1.0")]
        internal static IClientState ClientState { get; set; }
        [PluginService]
        [RequiredVersion("1.0")]
        internal static IChatGui ChatGui { get; set; }
        [PluginService]
        [RequiredVersion("1.0")]
        internal static IDataManager DataManager { get; set; }
        [PluginService]
        [RequiredVersion("1.0")]
        internal static IGameInteropProvider GameInteropProvider { get; set; }
        [PluginService]
        [RequiredVersion("1.0")]
        internal static IFramework Framework { get; set; }
    }
}
