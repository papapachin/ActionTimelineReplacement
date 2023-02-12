using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
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
        internal static SigScanner Scanner { get; set; }
        [PluginService]
        [RequiredVersion("1.0")]
        internal static CommandManager CommandManager { get; set; }
        [PluginService]
        [RequiredVersion("1.0")]
        internal static ClientState ClientState { get; set; }
        [PluginService]
        [RequiredVersion("1.0")]
        internal static ChatGui ChatGui { get; set; }
        [PluginService]
        [RequiredVersion("1.0")]
        internal static DataManager DataManager { get; set; }
        [PluginService]
        [RequiredVersion("1.0")]
        internal static Framework Framework { get; set; }
    }
}
