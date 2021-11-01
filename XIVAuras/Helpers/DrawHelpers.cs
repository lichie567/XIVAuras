using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Interface;
using ImGuiNET;

namespace XIVAuras.Helpers
{
    public class DrawHelpers
    {
        public static void DrawButton(string label, FontAwesomeIcon icon, Action clickAction, string? help = null)
        {
            ImGui.Text(label);
            ImGui.SameLine();
            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button(icon.ToIconString()))
            {
                clickAction();
            }

            ImGui.PopFont();
            if (!string.IsNullOrEmpty(help) && ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(help);
            }
        }
    }
}
