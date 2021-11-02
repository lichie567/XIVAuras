using System;
using System.Numerics;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
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

        private ConfigWindow EditAuraWindow { get; init; }

        private ConfigWindow EditGroupWindow { get; init; }

        private XIVAurasConfig Config { get; init; }

        private readonly Vector2 _origin = ImGui.GetMainViewport().Size / 2f;

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

            this.ConfigRoot = new ConfigWindow(this.Config, _origin);
            this.EditAuraWindow = new ConfigWindow("Edit Aura");
            this.EditGroupWindow = new ConfigWindow("Edit Group");

            this.WindowSystem = new WindowSystem("XIVAuras");
            this.WindowSystem.AddWindow(this.ConfigRoot);
            this.WindowSystem.AddWindow(this.EditAuraWindow);
            this.WindowSystem.AddWindow(this.EditGroupWindow);

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

        public void Edit(IAuraListItem aura)
        {
            ConfigWindow window = aura.Type switch
            {
                AuraType.Group => this.EditGroupWindow,
                _ => this.EditAuraWindow
            };

            window.DisplayConfig(aura, this.ConfigRoot.Position);
        }

        private void Draw()
        {
            if (this.ClientState.LocalPlayer == null)
            {
                return;
            }

            Condition condition = Singletons.Get<Condition>();
            bool characterBusy =
                condition[ConditionFlag.WatchingCutscene] ||
                condition[ConditionFlag.WatchingCutscene78] ||
                condition[ConditionFlag.OccupiedInCutSceneEvent] ||
                condition[ConditionFlag.CreatingCharacter] ||
                condition[ConditionFlag.BetweenAreas] ||
                condition[ConditionFlag.BetweenAreas51] ||
                condition[ConditionFlag.OccupiedSummoningBell];

            if (characterBusy)
            {
                return;
            }

            this.WindowSystem.Draw();
            foreach (IAuraListItem aura in this.Config.AuraList.Auras)
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
            ConfigHelpers.SaveConfig();
        }

        private void PluginCommand(string command, string arguments)
        {
            this.ConfigRoot.IsOpen ^= true;
            this.EditAuraWindow.IsOpen = false;
            this.EditGroupWindow.IsOpen = false;
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
