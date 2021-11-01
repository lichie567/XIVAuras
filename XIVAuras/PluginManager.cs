using System;
using System.Numerics;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ImGuiNET;
using XIVAuras.Config;
using XIVAuras.Helpers;

namespace XIVAuras
{
    public class PluginManager : IXIVAurasDisposable
    {
        private ClientState ClientState { get; set; }

        private DalamudPluginInterface PluginInterface { get; set; }

        private CommandManager CommandManager { get; set; }

        private WindowSystem WindowSystem { get; set; }

        private ConfigWindow ConfigRoot { get; set; }

        private XIVAurasConfig Config { get; set; }

        private readonly Vector2 _origin = ImGui.GetMainViewport().Size / 2f;

        public PluginManager(
            ClientState clientState,
            DalamudPluginInterface pluginInterface,
            CommandManager commandManager)
        {
            this.ClientState = clientState;
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Config = XIVAurasConfig.LoadConfig(Plugin.ConfigFilePath);
            this.WindowSystem = new WindowSystem("XIVAuras");
            this.ConfigRoot = new ConfigWindow(this.Config);
            this.WindowSystem.AddWindow(this.ConfigRoot);

            this.CommandManager.AddHandler(
                "/xa",
                new CommandInfo(PluginCommand)
                {
                    HelpMessage = "Opens the XIVAuras configuration window.",
                    ShowInHelp = true
                }
            );

            this.PluginInterface.UiBuilder.Draw += Draw;
            this.PluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;
            this.ClientState.Logout += OnLogout;
        }

        public void AddWindow(Window newWindow)
        {
            this.WindowSystem.AddWindow(newWindow);
        }

        public void RemoveWindow(Window toRemove)
        {
            this.WindowSystem.RemoveWindow(toRemove);
        }

        private void Draw()
        {
            if (this.Config == null || this.ClientState.LocalPlayer == null) return;

            this.WindowSystem.Draw();
            foreach (var aura in this.Config.Auras)
            {
                aura.Draw(_origin);
            }
        }

        private void OpenConfigUi()
        {
            this.ConfigRoot.IsOpen = true;
        }

        private void OnLogout(object? sender, EventArgs? args)
        {
            XIVAurasConfig.SaveConfig(this.Config);
        }

        private void PluginCommand(string command, string arguments)
        {
            this.ConfigRoot.IsOpen = !this.ConfigRoot.IsOpen;
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
                this.PluginInterface.UiBuilder.Draw -= Draw;
                this.PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigUi;
                this.ClientState.Logout -= OnLogout;
                this.WindowSystem.RemoveAllWindows();
                this.CommandManager.RemoveHandler("/xa");
            }
        }
    }
}
