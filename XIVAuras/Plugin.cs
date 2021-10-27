using System;
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
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using XIVAuras.Config;
using SigScanner = Dalamud.Game.SigScanner;

namespace DelvUI
{
    public class Plugin : IDalamudPlugin
    {
        public static string Version { get; private set; } = "0.0.1.0";

        public static string AssemblyLocation { get; private set; } = "";

        public string Name => "XIVAuras";

        public ClientState ClientState { get; private set; }

        public CommandManager CommandManager { get; private set; }

        public Condition Condition { get; private set; }

        public DalamudPluginInterface PluginInterface { get; private set; }

        public DataManager DataManager { get; private set; }

        public Framework Framework { get; private set; }

        public GameGui GameGui { get; private set; }

        public JobGauges JobGauges { get; private set; }

        public ObjectTable ObjectTable { get; private set; }

        public SigScanner SigScanner { get; private set; }

        public TargetManager TargetManager { get; private set; }

        public UiBuilder UiBuilder { get; private set; }

        public PartyList PartyList { get; private set; }

        private WindowSystem _windowSystem;

        private Window _configRoot;


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
            this.ClientState = clientState;
            this.CommandManager = commandManager;
            this.Condition = condition;
            this.PluginInterface = pluginInterface;
            this.DataManager = dataManager;
            this.Framework = framework;
            this.GameGui = gameGui;
            this.JobGauges = jobGauges;
            this.ObjectTable = objectTable;
            this.PartyList = partyList;
            this.SigScanner = sigScanner;
            this.TargetManager = targetManager;
            this.UiBuilder = PluginInterface.UiBuilder;

            if (pluginInterface.AssemblyLocation.DirectoryName != null)
            {
                AssemblyLocation = pluginInterface.AssemblyLocation.DirectoryName + "\\";
            }
            else
            {
                AssemblyLocation = Assembly.GetExecutingAssembly().Location;
            }

            Plugin.Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? Plugin.Version;

            this._windowSystem = new WindowSystem("XIVAuras_Windows");
            this._configRoot = new ConfigWindow();
            this._windowSystem.AddWindow(this._configRoot);

            CommandManager.AddHandler(
                "/xa",
                new CommandInfo(PluginCommand)
                {
                    HelpMessage = "Opens the XIVAuras settings.",
                    ShowInHelp = true
                }
            );

            UiBuilder.Draw += Draw;
            UiBuilder.OpenConfigUi += OpenConfigUi;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private void OpenConfigUi()
        {
            _configRoot.IsOpen = true;
        }
        private void PluginCommand(string command, string arguments)
        {
            _configRoot.IsOpen = !_configRoot.IsOpen;
        }

        private void Draw()
        {
            this._windowSystem.Draw();
        }
    }
}
