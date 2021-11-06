using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using Newtonsoft.Json;
using XIVAuras.Config;
using XIVAuras.Helpers;

namespace XIVAuras.Auras
{
    [JsonObject]
    public class AuraLabel : AuraListItem
    {
        [JsonIgnore] private DataSource? Data { get; set; }

        public override AuraType Type => AuraType.Label;

        public LabelStyleConfig LabelStyleConfig { get; init; }

        // Constuctor for deserialization
        public AuraLabel() : this(string.Empty) { }

        public AuraLabel(string name) : base(name)
        {
            this.Name = name;
            this.LabelStyleConfig = new LabelStyleConfig();
        }

        public override IEnumerator<IConfigPage> GetEnumerator()
        {
            yield return this.LabelStyleConfig;
        }

        public override void Draw(Vector2 pos, Vector2? parentSize = null)
        {
            if (!parentSize.HasValue || !this.Data.HasValue)
            {
                return;
            }

            Vector2 size = parentSize.Value;
            DataSource data = this.Data.Value;

            string text = this.LabelStyleConfig.TextFormat.Replace("[duration]", $"{Math.Truncate(data.Duration)}");
            text = text.Replace("[stacks]", $"{Math.Truncate(data.Duration)}");
            text = text.Replace("[cooldown]", $"{Math.Truncate(data.Duration)}");

            Vector2 textSize = ImGui.CalcTextSize(text);
            Vector2 textPos = GetAnchoredPosition(pos + this.LabelStyleConfig.Position, -size, this.LabelStyleConfig.ParentAnchor);
            textPos = GetAnchoredPosition(textPos, textSize, this.LabelStyleConfig.TextAlign);

            DrawHelpers.DrawInWindow($"##{this.ID}", textPos, textSize, false, true, true, (drawList) =>
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
        }

        public void SetData(DataSource data)
        {
            this.Data = data;
        }

        public static Vector2 GetAnchoredPosition(Vector2 position, Vector2 size, DrawAnchor anchor)
        {
            return anchor switch
            {
                DrawAnchor.Center => position - size / 2f,
                DrawAnchor.Left => position + new Vector2(0, -size.Y / 2f),
                DrawAnchor.Right => position + new Vector2(-size.X, -size.Y / 2f),
                DrawAnchor.Top => position + new Vector2(-size.X / 2f, 0),
                DrawAnchor.TopLeft => position,
                DrawAnchor.TopRight => position + new Vector2(-size.X, 0),
                DrawAnchor.Bottom => position + new Vector2(-size.X / 2f, -size.Y),
                DrawAnchor.BottomLeft => position + new Vector2(0, -size.Y),
                DrawAnchor.BottomRight => position + new Vector2(-size.X, -size.Y),
                _ => position
            };
        }
    }
}