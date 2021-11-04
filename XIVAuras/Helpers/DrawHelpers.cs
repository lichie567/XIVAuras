using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Internal.Notifications;
using ImGuiNET;

namespace XIVAuras.Helpers
{
    public class DrawHelpers
    {
        public static void DrawButton(
            string label,
            FontAwesomeIcon icon,
            Action clickAction,
            string? help = null,
            Vector2? size = null)
        {
            if (!string.IsNullOrEmpty(label))
            {
                ImGui.Text(label);
                ImGui.SameLine();
            }

            ImGui.PushFont(UiBuilder.IconFont);
            if (ImGui.Button(icon.ToIconString(), size ?? Vector2.Zero))
            {
                clickAction();
            }

            ImGui.PopFont();
            if (!string.IsNullOrEmpty(help) && ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(help);
            }
        }

        public static void DrawNotification(
            string message,
            NotificationType type = NotificationType.Success,
            uint durationInMs = 3000,
            string title = "XIVAuras")
        {
            Singletons.Get<UiBuilder>().AddNotification(message, title, type, durationInMs);
        }

        public static void DrawNestIndicator(int depth)
        {
            // This draws the L shaped symbols and padding to the left of config items collapsible under a checkbox.
            // Shift cursor to the right to pad for children with depth more than 1.
            // 26 is an arbitrary value I found to be around half the width of a checkbox
            Vector2 oldCursor = ImGui.GetCursorPos();
            Vector2 offset = new Vector2(26 * Math.Max((depth - 1), 0), 2);
            ImGui.SetCursorPos(oldCursor + offset);
            ImGui.TextColored(new Vector4(229f / 255f, 57f / 255f, 57f / 255f, 1f), "\u2002\u2514");
            ImGui.SameLine();
            ImGui.SetCursorPosY(oldCursor.Y);
        }

        public static void DrawSpacing(int spacingSize)
        {
            for (int i = 0; i < spacingSize; i++)
            {
                ImGui.NewLine();
            }
        }
    }
}
