using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace XIVAuras.Config
{
    public class ConfigWindow : Window
    {
        public Action? CloseAction;

        public ConfigWindow() : base("XIVAuras")
        {
            Flags = ImGuiWindowFlags.NoScrollbar |
                    ImGuiWindowFlags.NoCollapse |
                    ImGuiWindowFlags.NoResize |
                    ImGuiWindowFlags.NoScrollWithMouse;

            Size = new Vector2(1050, 750);
        }

        public override void Draw()
        {
            var pos = ImGui.GetWindowPos();

            if (!ImGui.BeginTabBar("##XIVAuras_Settings_TabBar"))
            {
                return;
            }

            ImGui.EndTabBar();
        }

        public override void OnClose()
        {
            CloseAction?.Invoke();
        }

        public override void PreDraw()
        {
            ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));
            ImGui.PushStyleColor(ImGuiCol.BorderShadow, new Vector4(0f / 255f, 0f / 255f, 0f / 255f, 100f / 100f));
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(20f / 255f, 21f / 255f, 20f / 255f, 100f / 100f));

            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 1);
        }
    }
}
