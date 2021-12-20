using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using Newtonsoft.Json;
using XIVAuras.Config;
using XIVAuras.Helpers;

namespace XIVAuras.Auras
{
    public class AuraLabel : AuraListItem
    {
        [JsonIgnore] private DataSource? Data { get; set; }

        public override AuraType Type => AuraType.Label;

        public LabelStyleConfig LabelStyleConfig { get; init; }

        public VisibilityConfig VisibilityConfig { get; init; }

        // Constuctor for deserialization
        public AuraLabel() : this(string.Empty) { }

        public AuraLabel(string name, string textFormat = "") : base(name)
        {
            this.Name = name;
            this.LabelStyleConfig = new LabelStyleConfig(textFormat);
            this.VisibilityConfig = new VisibilityConfig();
        }

        public override IEnumerable<IConfigPage> GetConfigPages()
        {
            yield return this.LabelStyleConfig;
            yield return this.VisibilityConfig;
        }

        public override void Draw(Vector2 pos, Vector2? parentSize = null)
        {
            if (!this.VisibilityConfig.IsVisible(this.Data))
            {
                return;
            }

            Vector2 size = parentSize.HasValue ? parentSize.Value : ImGui.GetMainViewport().Size;
            pos = parentSize.HasValue ? pos : Vector2.Zero;

            string text = this.LabelStyleConfig.TextFormat;
            if (this.Data is not null)
            {
                text = text.Replace("[value]", this.LabelStyleConfig.FormatNumber(this.Data.Value));
                text = text.Replace("[stacks]", this.LabelStyleConfig.FormatNumber(this.Data.Stacks));
            }

            bool fontPushed = FontsManager.PushFont(this.LabelStyleConfig.FontKey);

            Vector2 textSize = ImGui.CalcTextSize(text);
            Vector2 textPos = Utils.GetAnchoredPosition(pos + this.LabelStyleConfig.Position, -size, this.LabelStyleConfig.ParentAnchor);
            textPos = Utils.GetAnchoredPosition(textPos, textSize, this.LabelStyleConfig.TextAlign);

            Vector2 textPad = new Vector2(2, 2); // Add small amount of padding to avoid text getting clipped
            DrawHelpers.DrawInWindow($"##{this.ID}", textPos - textPad, textSize + textPad, false, true, true, (drawList) =>
            {
                uint textColor = this.LabelStyleConfig.TextColor.Base;

                if (this.LabelStyleConfig.ShowOutline)
                {
                    uint outlineColor = this.LabelStyleConfig.OutlineColor.Base;
                    DrawHelpers.DrawOutlinedText(text, textPos, textColor, outlineColor, drawList);
                }
                else
                {
                    drawList.AddText(textPos, textColor, text);
                }
            });

            if (fontPushed)
            {
                ImGui.PopFont();
            }
        }

        public void SetData(DataSource data)
        {
            this.Data = data;
        }
    }
}