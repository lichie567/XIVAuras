using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using XIVAuras.Auras;
using XIVAuras.Config;

namespace XIVAuras.Windows
{
    public class EditAuraWindow : Window
    {
        private IAuraListItem? Aura { get; set; }

        public EditAuraWindow() : base("Edit Aura")
        {
            this.Flags = ImGuiWindowFlags.NoScrollbar |
                    ImGuiWindowFlags.NoCollapse |
                    ImGuiWindowFlags.NoResize |
                    ImGuiWindowFlags.NoScrollWithMouse;

            this.Size = new Vector2(500, 500);
        }

        public void DisplayAuraConfig(IAuraListItem aura)
        {
            this.Aura = aura;
            this.IsOpen = true;
        }

        public override void Draw()
        {
            if (this.Aura is null || !this.Aura.ConfigPages.Any())
            {
                this.IsOpen = false;
                return;
            }

            if (!ImGui.BeginTabBar($"##XIVAuras_Edit"))
            {
                return;
            }

            foreach (IConfigPage page in this.Aura.ConfigPages)
            {
                if (ImGui.BeginTabItem($"{page.Name}##XIVAuras_Edit"))
                {
                    page.DrawOptions();
                    ImGui.EndTabItem();
                }
            }

            ImGui.EndTabBar();
        }

        public override void OnClose()
        {
            this.Aura = null;
        }
    }
}
