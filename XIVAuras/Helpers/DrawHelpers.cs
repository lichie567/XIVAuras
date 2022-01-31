using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Internal.Notifications;
using ImGuiNET;
using ImGuiScene;
using XIVAuras.Config;

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

        public static void DrawSpacing(int spacingSize = 1)
        {
            for (int i = 0; i < spacingSize; i++)
            {
                ImGui.NewLine();
            }
        }

        public static void DrawIcon(
            ushort iconId,
            Vector2 position,
            Vector2 size,
            bool cropIcon,
            int stackCount,
            bool desaturate,
            float opacity,
            ImDrawListPtr drawList)
        {
            TextureWrap? tex = Singletons.Get<TexturesCache>().GetTextureFromIconId(iconId, (uint)stackCount, true, desaturate, opacity);

            if (tex is null)
            {
                return;
            }

            (Vector2 uv0, Vector2 uv1) = GetTexCoordinates(tex, size, cropIcon);

            drawList.AddImage(tex.ImGuiHandle, position, position + size, uv0, uv1);
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

        public static void DrawInWindow(
            string name,
            Vector2 pos,
            Vector2 size,
            bool preview,
            bool setPosition,
            Action<ImDrawListPtr> drawAction)
        {
            DrawInWindow(name, pos, size, preview, false, preview, setPosition, drawAction);
        }

        public static void DrawInWindow(
            string name,
            Vector2 pos,
            Vector2 size,
            bool needsInput,
            bool needsFocus,
            bool needsWindow,
            Action<ImDrawListPtr> drawAction)
        {
            DrawInWindow(name, pos, size, needsInput, needsFocus, needsWindow, true, drawAction, ImGuiWindowFlags.NoMove);
        }

        public static void DrawInWindow(
            string name,
            Vector2 pos,
            Vector2 size,
            bool needsInput,
            bool needsFocus,
            bool needsWindow,
            bool setPosition,
            Action<ImDrawListPtr> drawAction,
            ImGuiWindowFlags extraFlags = ImGuiWindowFlags.None)
        {
            if (!needsInput && !needsWindow)
            {
                drawAction(ImGui.GetWindowDrawList());
                return;
            }

            ImGuiWindowFlags windowFlags =
                ImGuiWindowFlags.NoSavedSettings |
                ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoBackground |
                ImGuiWindowFlags.NoResize |
                extraFlags;

            if (!needsInput)
            {
                windowFlags |= ImGuiWindowFlags.NoInputs;
            }

            if (!needsFocus)
            {
                windowFlags |= ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoBringToFrontOnFocus;
            }

            ImGui.SetNextWindowSize(size);
            if (setPosition)
            {
                ImGui.SetNextWindowPos(pos);
            }

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);

            if (ImGui.Begin(name, windowFlags))
            {
                drawAction(ImGui.GetWindowDrawList());
                ImGui.End();
            }

            ImGui.PopStyleVar(3);
        }

        public static void DrawText(
            ImDrawListPtr drawList,
            string text,
            Vector2 pos,
            uint color,
            bool outline,
            uint outlineColor = 0xFF000000,
            int thickness = 1)
        {
            // outline
            if (outline)
            {
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
            }

            // text
            drawList.AddText(new Vector2(pos.X, pos.Y), color, text);
        }
        
        public static void DrawSegmentedLineHorizontal(
            ImDrawListPtr drawList,
            Vector2 start,
            float width,
            float height,
            float prog,
            int segments,
            ConfigColor color1,
            ConfigColor color2)
        {
            float segWidth = width / segments;
            Vector2 interval = new(segWidth, height);
            Vector2 first = new(segWidth * (prog < 0.5 ? prog * 2 : (prog - 0.5f) * 2), height);
            Vector2 last = interval.AddX(-first.X);
            uint[] colors = new uint[2] { prog < 0.5 ? color2.Base : color1.Base, prog < 0.5 ? color1.Base : color2.Base };

            drawList.AddRectFilled(start, start + first, colors[1]);
            start = start.AddX(first.X);

            for (int i = 0; i < segments - 1; i ++)
            {
                drawList.AddRectFilled(start, start + interval, colors[i % colors.Length]);
                start = start.AddX(segWidth);
            }

            drawList.AddRectFilled(start, start + last, colors[1]);
        }
        
        public static void DrawSegmentedLineVertical(
            ImDrawListPtr drawList,
            Vector2 start,
            float width,
            float height,
            float prog,
            int segments,
            ConfigColor color1,
            ConfigColor color2)
        {
            float segHeight = height / segments;
            Vector2 interval = new(width, segHeight);
            Vector2 first = new(width, segHeight * (prog < 0.5 ? prog * 2 : (prog - 0.5f) * 2));
            Vector2 last = interval.AddY(-first.Y);
            uint[] colors = new uint[2] { prog < 0.5 ? color2.Base : color1.Base, prog < 0.5 ? color1.Base : color2.Base };

            drawList.AddRectFilled(start, start + first, colors[1]);
            start = start.AddY(first.Y);

            for (int i = 0; i < segments - 1; i ++)
            {
                drawList.AddRectFilled(start, start + interval, colors[i % colors.Length]);
                start = start.AddY(segHeight);
            }

            drawList.AddRectFilled(start, start + last, colors[1]);
        }
    }
}
