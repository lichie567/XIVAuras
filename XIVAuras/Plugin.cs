using System;
using System.IO;
using System.Numerics;
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
using ImGuiNET;
using XIVAuras.Config;
using XIVAuras.Helpers;
using SigScanner = Dalamud.Game.SigScanner;

namespace XIVAuras
{
    public class Plugin : IDalamudPlugin
    {
        public static string Version { get; private set; } = "0.0.1.0";

        public static string AssemblyLocation { get; private set; } = "";

        public string Name => "XIVAuras";

        public const string ConfigFileName = "XIVAuras.json";

        public static string ConfigFilePath = "";

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

        private ConfigWindow _configWindow;

        private XIVAurasConfig _config;

        private readonly Vector2 _origin = ImGui.GetMainViewport().Size / 2f;

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

            if (this.PluginInterface.AssemblyLocation.DirectoryName != null)
            {
                Plugin.AssemblyLocation = this.PluginInterface.AssemblyLocation.DirectoryName + "\\";
            }
            else
            {
                Plugin.AssemblyLocation = Assembly.GetExecutingAssembly().Location;
            }

            Plugin.Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? Plugin.Version;
            Plugin.ConfigFilePath = Path.Combine(this.PluginInterface.GetPluginConfigDirectory(), Plugin.ConfigFileName);

            this._config = XIVAurasConfig.LoadConfig(Plugin.ConfigFilePath);
            this._windowSystem = new WindowSystem("XIVAuras_Windows");
            this._configWindow = new ConfigWindow(this._config);
            this._windowSystem.AddWindow(this._configWindow);

            this.CommandManager.AddHandler(
                "/xa",
                new CommandInfo(PluginCommand)
                {
                    HelpMessage = "Opens the XIVAuras configuration window.",
                    ShowInHelp = true
                }
            );

            this.UiBuilder.Draw += Draw;
            this.UiBuilder.OpenConfigUi += OpenConfigUi;
            this.ClientState.Logout += OnLogout;
        }

        private void Draw()
        {
            if (this._config == null || ClientState.LocalPlayer == null) return;

            this._windowSystem.Draw();
            foreach (var aura in this._config.Auras)
            {
                aura.Draw(_origin);
            }
        }

        private void OpenConfigUi()
        {
            this._configWindow.IsOpen = true;
        }

        private void OnLogout(object? sender, EventArgs? args)
        {
            XIVAurasConfig.SaveConfig(this._config);
        }

        private void PluginCommand(string command, string arguments)
        {
            this._configWindow.IsOpen = !this._configWindow.IsOpen;
        }

        public void Dispose()
        {
            XIVAurasConfig.SaveConfig(this._config);

            Singletons.DisposeAll();

            this._windowSystem.RemoveAllWindows();
            this.CommandManager.RemoveHandler("/xa");
            this.UiBuilder.Draw -= Draw;
            this.UiBuilder.OpenConfigUi -= OpenConfigUi;
            this.ClientState.Logout -= OnLogout;
            GC.SuppressFinalize(this);
        }
    }
}
