using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using ImGuiNET;
using Newtonsoft.Json;
using XIVAuras.Helpers;

namespace XIVAuras.Config
{
    public class LabelStyleConfig : IConfigPage
    {
        [JsonIgnore] private string[] _anchorOptions = Enum.GetNames(typeof(DrawAnchor));
        [JsonIgnore] private string[] _roundingOptions = new string[] { "Floor", "Ceiling", "Round" };

        public string Name => "Text";

        public string TextFormat = "";
        public Vector2 Position = new Vector2(0, 0);
        public DrawAnchor ParentAnchor = DrawAnchor.Center;
        public DrawAnchor TextAlign = DrawAnchor.Center;
        public int FontID = 0;
        public string FontKey = FontsManager.DefaultBigFontKey;
        public ConfigColor TextColor = new ConfigColor(1, 1, 1, 1);
        public bool ShowOutline = true;
        public ConfigColor OutlineColor = new ConfigColor(0, 0, 0, 1);
        public int Rounding = 0;

        public LabelStyleConfig() : this("") { }

        public LabelStyleConfig(string textFormat)
        {
            this.TextFormat = textFormat;
        }

        public IConfigPage GetDefault() => new LabelStyleConfig("[value]");

        public void DrawConfig(IConfigurable parent, Vector2 size, float padX, float padY)
        {
            if (ImGui.BeginChild("##LabelStyleConfig", new Vector2(size.X, size.Y), true))
            {
                ImGui.InputTextWithHint("Text Format", "Hover for Formatting Info", ref this.TextFormat, 64);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(Utils.GetTagsTooltip(DataSource.TextTags));
                }

                ImGui.Combo("Number Format", ref this.Rounding, _roundingOptions, _roundingOptions.Length);
                ImGui.DragFloat2("Position", ref this.Position);
                ImGui.Combo("Parent Anchor", ref Unsafe.As<DrawAnchor, int>(ref this.ParentAnchor), _anchorOptions, _anchorOptions.Length);
                ImGui.Combo("Text Align", ref Unsafe.As<DrawAnchor, int>(ref this.TextAlign), _anchorOptions, _anchorOptions.Length);

                string[] fontOptions = FontsManager.GetFontList();
                if (!FontsManager.ValidateFont(fontOptions, this.FontID, this.FontKey))
                {
                    this.FontID = 0;
                    for (int i = 0; i < fontOptions.Length; i++)
                    {
                        if (this.FontKey.Equals(fontOptions[i]))
                        {
                            this.FontID = i;
                        }
                    }
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
            }

            ImGui.EndChild();
        }
    }
}