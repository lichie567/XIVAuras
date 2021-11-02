using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using XIVAuras.Auras;
using XIVAuras.Config;
using XIVAuras.Helpers;

namespace XIVAuras.Windows
{
    public class ConfigWindow : Window
    {
        private IConfigurable? ConfigItem { get; set; }

        public ConfigWindow(string name, Vector2? position = null) : base(name)
        {
            this.Flags =
                ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoScrollWithMouse |
                ImGuiWindowFlags.NoSavedSettings;

            this.Size = new Vector2(500, 500);
            this.Position = position is null ? null : position - this.Size / 2;
            this.PositionCondition = ImGuiCond.FirstUseEver;
        }

        public ConfigWindow(IConfigurable configItem, Vector2? position = null) : this(configItem.Name, position)
        {
            this.ConfigItem = configItem;
        }

        public void DisplayConfig(IAuraListItem aura, Vector2? pos = null)
        {
            this.ConfigItem = aura;
            this.Position ??= pos + new Vector2(503, 0);
            this.WindowName = $"{aura.Type} [{aura.Name}]";
            this.IsOpen = true;
        }

        public override void Draw()
        {
            if (this.ConfigItem is null || !this.ConfigItem.Any())
            {
                this.IsOpen = false;
                return;
            }

            this.Position = ImGui.GetWindowPos();

            if (ImGui.BeginTabBar($"##{this.WindowName}"))
            {
                foreach (IConfigPage page in this.ConfigItem)
                {
                    if (ImGui.BeginTabItem($"{page.Name}##{this.WindowName}"))
                    {
                        page.DrawConfig();
                        ImGui.EndTabItem();
                    }
                }

                ImGui.EndTabBar();
            }
        }

        public override void OnClose()
        {
            ConfigHelpers.SaveConfig();
        }
    }
}