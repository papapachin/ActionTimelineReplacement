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
    }
}
