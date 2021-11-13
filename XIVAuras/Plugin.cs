using System;
using System.IO;
using System.Reflection;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Plugin;
using XIVAuras.Config;
using XIVAuras.Helpers;
using SigScanner = Dalamud.Game.SigScanner;

namespace XIVAuras
{
    public class Plugin : IDalamudPlugin
    {
        public static string Version { get; private set; } = "0.1.3.0";

        public string Name => "XIVAuras";

        public const string ConfigFileName = "XIVAuras.json";

        public static string ConfigFilePath = "";

        public Plugin(
            ClientState clientState,
            CommandManager commandManager,
            Condition condition,
            DalamudPluginInterface pluginInterface,
            DataManager dataManager,
            Framework framework,
            GameGui gameGui,
            JobGauges jobGauges,
            ObjectTable objectTable,
            PartyList partyList,
            SigScanner sigScanner,
            TargetManager targetManager
        )
        {
            Plugin.Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? Plugin.Version;
            Plugin.ConfigFilePath = Path.Combine(pluginInterface.GetPluginConfigDirectory(), Plugin.ConfigFileName);

            // Register Dalamud APIs
            Singletons.Register(clientState);
            Singletons.Register(commandManager);
            Singletons.Register(condition);
            Singletons.Register(pluginInterface);
            Singletons.Register(dataManager);
            Singletons.Register(framework);
            Singletons.Register(gameGui);
            Singletons.Register(jobGauges);
            Singletons.Register(objectTable);
            Singletons.Register(partyList);
            Singletons.Register(sigScanner);
            Singletons.Register(targetManager);
            Singletons.Register(pluginInterface.UiBuilder);

            // Initialize FFXIVClientStructs
            FFXIVClientStructs.Resolver.Initialize(sigScanner.SearchBase);

            // Load config
            XIVAurasConfig config = ConfigHelpers.LoadConfig(Plugin.ConfigFilePath);
            Singletons.Register(config);

            // Initialize Fonts
            Singletons.Register(new FontsManager(pluginInterface.UiBuilder, config.FontConfig.Fonts.Values));

            // Start the plugin
            Singletons.Register(new PluginManager(clientState, commandManager, pluginInterface, config));
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Singletons.Dispose();
            }
        }
    }
}
