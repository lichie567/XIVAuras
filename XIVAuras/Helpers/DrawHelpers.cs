using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Internal.Notifications;
using ImGuiNET;
using ImGuiScene;
using Lumina.Excel;

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

        public static void DrawIcon(uint iconId, Vector2 position, Vector2 size, ImDrawListPtr drawList)
        {
            TextureWrap? texture = Singletons.Get<TexturesCache>().GetTextureFromIconId(iconId);
            if (texture == null) { return; }

            drawList.AddImage(texture.ImGuiHandle, position, position + size, Vector2.Zero, Vector2.One);
        }

        public static void DrawIcon<T>(ImDrawListPtr drawList, dynamic row, Vector2 position, Vector2 size, bool drawBorder, bool cropIcon, int stackCount = 1) where T : ExcelRow
        {
            TextureWrap texture = Singletons.Get<TexturesCache>().GetTexture<T>(row, (uint)Math.Max(0, stackCount - 1));
            if (texture == null) { return; }

            (Vector2 uv0, Vector2 uv1) = GetTexCoordinates(texture, size, cropIcon);

            drawList.AddImage(texture.ImGuiHandle, position, position + size, uv0, uv1);

            if (drawBorder)
            {
                drawList.AddRect(position, position + size, 0xFF000000);
            }
        }

        public static (Vector2, Vector2) GetTexCoordinates(TextureWrap texture, Vector2 size, bool cropIcon = true)
        {
            if (texture == null)
            {
                return (Vector2.Zero, Vector2.Zero);
            }

            // Status = 24x32, show from 2,7 until 22,26
            //show from 0,0 until 24,32 for uncropped status icon

            float uv0x = cropIcon ? 4f : 1f;
            float uv0y = cropIcon ? 14f : 1f;

            float uv1x = cropIcon ? 4f : 1f;
            float uv1y = cropIcon ? 12f : 1f;

            Vector2 uv0 = new(uv0x / texture.Width, uv0y / texture.Height);
            Vector2 uv1 = new(1f - uv1x / texture.Width, 1f - uv1y / texture.Height);

            return (uv0, uv1);
        }

        public static void DrawInWindow(string name, Vector2 pos, Vector2 size, bool needsInput, bool needsFocus, Action<ImDrawListPtr> drawAction)
        {
            const ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoTitleBar |
                                                 ImGuiWindowFlags.NoScrollbar |
                                                 ImGuiWindowFlags.NoBackground |
                                                 ImGuiWindowFlags.NoMove |
                                                 ImGuiWindowFlags.NoResize;

            DrawInWindow(name, pos, size, needsInput, needsFocus, false, windowFlags, drawAction);
        }

        public static void DrawInWindow(
            string name,
            Vector2 pos,
            Vector2 size,
            bool needsInput,
            bool needsFocus,
            bool needsWindow,
            ImGuiWindowFlags windowFlags,
            Action<ImDrawListPtr> drawAction)
        {
            windowFlags |= ImGuiWindowFlags.NoSavedSettings;

            if (!needsInput)
            {
                windowFlags |= ImGuiWindowFlags.NoInputs;
            }

            if (!needsFocus)
            {
                windowFlags |= ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoBringToFrontOnFocus;
            }

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            if (!needsInput && !needsWindow)
            {
                drawAction(drawList);
                return;
            }

            ImGui.SetNextWindowPos(pos);
            ImGui.SetNextWindowSize(size);

            bool begin = ImGui.Begin(name, windowFlags);
            if (!begin)
            {
                ImGui.End();
                return;
            }

            drawAction(drawList);

            ImGui.End();
        }

        public static void DrawOutlinedText(string text, Vector2 pos, uint color, uint outlineColor, ImDrawListPtr drawList, int thickness = 1)
        {
            // outline
            for (int i = 1; i < thickness + 1; i++)
            {
                drawList.AddText(new Vector2(pos.X - i, pos.Y + i), outlineColor, text);
                drawList.AddText(new Vector2(pos.X, pos.Y + i), outlineColor, text);
                drawList.AddText(new Vector2(pos.X + i, pos.Y + i), outlineColor, text);
                drawList.AddText(new Vector2(pos.X - i, pos.Y), outlineColor, text);
                drawList.AddText(new Vector2(pos.X + i, pos.Y), outlineColor, text);
                drawList.AddText(new Vector2(pos.X - i, pos.Y - i), outlineColor, text);
                drawList.AddText(new Vector2(pos.X, pos.Y - i), outlineColor, text);
                drawList.AddText(new Vector2(pos.X + i, pos.Y - i), outlineColor, text);
            }

            // text
            drawList.AddText(new Vector2(pos.X, pos.Y), color, text);
        }
    }
}
