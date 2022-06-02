using System;
using System.Numerics;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ImGuiNET;
using XIVAuras.Auras;
using XIVAuras.Config;
using XIVAuras.Helpers;
using XIVAuras.Windows;

namespace XIVAuras
{
    public class PluginManager : IPluginDisposable
    {
        private ClientState ClientState { get; init; }

        private DalamudPluginInterface PluginInterface { get; init; }

        private CommandManager CommandManager { get; init; }

        private WindowSystem WindowSystem { get; init; }

        private ConfigWindow ConfigRoot { get; init; }

        private XIVAurasConfig Config { get; init; }

        private readonly Vector2 _configSize = new Vector2(600, 650);

        private readonly ImGuiWindowFlags _mainWindowFlags = 
            ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoScrollbar |
            ImGuiWindowFlags.AlwaysAutoResize |
            ImGuiWindowFlags.NoBackground |
            ImGuiWindowFlags.NoInputs |
            ImGuiWindowFlags.NoBringToFrontOnFocus |
            ImGuiWindowFlags.NoSavedSettings;

        public PluginManager(
            ClientState clientState,
            CommandManager commandManager,
            DalamudPluginInterface pluginInterface,
            XIVAurasConfig config)
        {
            this.ClientState = clientState;
            this.CommandManager = commandManager;
            this.PluginInterface = pluginInterface;
            this.Config = config;

            this.ConfigRoot = new ConfigWindow("ConfigRoot", ImGui.GetMainViewport().Size / 2, _configSize);
            this.WindowSystem = new WindowSystem("XIVAuras");
            this.WindowSystem.AddWindow(this.ConfigRoot);

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

        private void Draw()
        {
            if (!CharacterState.ShouldBeVisible())
            {
                return;
            }

            this.WindowSystem.Draw();

            Vector2 viewPortSize = ImGui.GetMainViewport().Size;
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(viewPortSize);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);

            try
            {
                if (ImGui.Begin("XIVAuras_Root", _mainWindowFlags))
                {
                    if (this.Config.VisibilityConfig.IsVisible(true))
                    {
                        Singletons.Get<StatusHelpers>().GenerateStatusMap();
                        Singletons.Get<ClipRectsHelper>().Update();
                        foreach (AuraListItem aura in this.Config.AuraList.Auras)
                        {
                            aura.Draw((viewPortSize / 2) + this.Config.GroupConfig.Position);
                        }
                    }
                }
            }
            finally
            {
                ImGui.End();
                ImGui.PopStyleVar(3);
            }
        }

        public void Edit(IConfigurable config)
        {
            this.ConfigRoot.PushConfig(config);
        }

        public bool IsConfigOpen()
        {
            return this.ConfigRoot.IsOpen;
        }

        public bool IsConfigurableOpen(IConfigurable configurable)
        {
            return this.ConfigRoot.IsConfigurableOpen(configurable);
        }

        public bool ShouldClip()
        {
            return this.Config.VisibilityConfig.Clip;
        }

        private void OpenConfigUi()
        {
            if (!this.ConfigRoot.IsOpen)
            {
                this.ConfigRoot.PushConfig(this.Config);
            }
        }

        private void OnLogout(object? sender, EventArgs? args)
        {
            ConfigHelpers.SaveConfig();
        }
        
        private void PluginCommand(string command, string arguments)
        {
            if (this.ConfigRoot.IsOpen)
            {
                this.ConfigRoot.IsOpen = false;
            }
            else
            {
                this.ConfigRoot.PushConfig(this.Config);
            }
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
