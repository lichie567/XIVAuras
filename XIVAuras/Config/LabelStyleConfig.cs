using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using ImGuiNET;
using Newtonsoft.Json;
using XIVAuras.Helpers;

namespace XIVAuras.Config
{
    public enum DrawAnchor
    {
        Center = 0,
        Left = 1,
        Right = 2,
        Top = 3,
        TopLeft = 4,
        TopRight = 5,
        Bottom = 6,
        BottomLeft = 7,
        BottomRight = 8
    }

    public class LabelStyleConfig : IConfigPage
    {
        [JsonIgnore] private string[] _anchorOptions = Enum.GetNames(typeof(DrawAnchor));

        public string Name => "Style";

        public string TextFormat = "";
        public Vector2 Position = new Vector2(0, 0);
        public DrawAnchor ParentAnchor = DrawAnchor.Center;
        public DrawAnchor TextAlign = DrawAnchor.Center;
        public int FontID = 0;
        public string FontKey = "Default";
        public ConfigColor TextColor = new ConfigColor(1, 1, 1, 1);
        public bool ShowOutline = true;
        public ConfigColor OutlineColor = new ConfigColor(0, 0, 0, 1);

        public void DrawConfig(Vector2 size, float padX, float padY)
        {
            if (ImGui.BeginChild("##LabelStyleConfig", new Vector2(size.X, size.Y), true))
            {
                ImGui.InputTextWithHint("Text Format", "[duration] or [cooldown] or [stacks]", ref this.TextFormat, 64);
                ImGui.DragFloat2("Position", ref this.Position);
                ImGui.Combo("Parent Anchor", ref Unsafe.As<DrawAnchor, int>(ref this.ParentAnchor), _anchorOptions, _anchorOptions.Length);
                ImGui.Combo("Text Align", ref Unsafe.As<DrawAnchor, int>(ref this.TextAlign), _anchorOptions, _anchorOptions.Length);

                string[] fontOptions = Singletons.Get<FontsManager>().GetFontList();
                if (this.FontID > fontOptions.Length ||
                    !fontOptions[this.FontID].Equals(this.FontKey))
                {
                    this.FontKey = "Default";
                    this.FontID = 0;
                }

                ImGui.Combo("Font", ref this.FontID, fontOptions, fontOptions.Length);
                this.FontKey = fontOptions[this.FontID];

                DrawHelpers.DrawSpacing(1);
                Vector4 textColor = this.TextColor.Vector;
                ImGui.ColorEdit4("Text Color", ref textColor);
                this.TextColor.Vector = textColor;
                ImGui.Checkbox("Show Outline", ref this.ShowOutline);
                if (this.ShowOutline)
                {
                    DrawHelpers.DrawNestIndicator(1);
                    Vector4 outlineColor = this.OutlineColor.Vector;
                    ImGui.ColorEdit4("Outline Color", ref outlineColor);
                    this.OutlineColor.Vector = outlineColor;
                }

                ImGui.EndChild();
            }
        }
    }
}