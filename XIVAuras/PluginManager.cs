using System;
using System.Numerics;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ImGuiNET;
using XIVAuras.Auras;
using XIVAuras.Config;
using XIVAuras.Helpers;
using XIVAuras.Windows;

namespace XIVAuras
{
    public class PluginManager : IXIVAurasDisposable
    {
        private ClientState ClientState { get; init; }

        private DalamudPluginInterface PluginInterface { get; init; }

        private CommandManager CommandManager { get; init; }

        private WindowSystem WindowSystem { get; init; }

        private ConfigWindow ConfigRoot { get; init; }

        private EditAuraWindow EditAuraWindow { get; init; }

        private XIVAurasConfig Config { get; init; }

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
            this.ConfigRoot = new ConfigWindow(this.Config);
            this.EditAuraWindow = new EditAuraWindow();
            this.WindowSystem = new WindowSystem("XIVAuras");
            this.WindowSystem.AddWindow(this.ConfigRoot);
            this.WindowSystem.AddWindow(this.EditAuraWindow);

            this.CommandManager.AddHandler(
                "/xa",
                new CommandInfo(PluginCommand)
                {
                    HelpMessage = "Opens the XIVAuras configuration window.",
                    ShowInHelp = true
                }
            );

            this.ClientState.Logout += OnLogout;
            this.PluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;
            this.PluginInterface.UiBuilder.Draw += Draw;
        }

        public void EditAura(IAuraListItem aura)
        {
            this.EditAuraWindow.DisplayAuraConfig(aura);
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
                // Don't modify order
                this.PluginInterface.UiBuilder.Draw -= Draw;
                this.PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigUi;
                this.ClientState.Logout -= OnLogout;
                this.CommandManager.RemoveHandler("/xa");
                this.WindowSystem.RemoveAllWindows();
            }
        }
    }
}
