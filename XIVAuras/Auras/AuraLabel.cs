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
        [JsonIgnore] private DataSource? _data;

        public override AuraType Type => AuraType.Label;

        public LabelStyleConfig LabelStyleConfig { get; set; }

        public VisibilityConfig VisibilityConfig { get; set; }

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

        public override void ImportPage(IConfigPage page)
        {
            switch (page)
            {
                case LabelStyleConfig newPage:
                    this.LabelStyleConfig = newPage;
                    break;
                case VisibilityConfig newPage:
                    this.VisibilityConfig = newPage;
                    break;
            }
        }

        public override void Draw(Vector2 pos, Vector2? parentSize = null)
        {
            if (!this.VisibilityConfig.IsVisible() && !this.Preview)
            {
                return;
            }

            Vector2 size = parentSize.HasValue ? parentSize.Value : ImGui.GetMainViewport().Size;
            pos = parentSize.HasValue ? pos : Vector2.Zero;

            string text = _data is not null
                ? _data.GetFormattedString(this.LabelStyleConfig.TextFormat, "N")
                : this.LabelStyleConfig.TextFormat;

            using (FontsManager.PushFont(this.LabelStyleConfig.FontKey))
            {
                Vector2 textSize = ImGui.CalcTextSize(text);
                Vector2 textPos = Utils.GetAnchoredPosition(pos + this.LabelStyleConfig.Position, -size, this.LabelStyleConfig.ParentAnchor);
                textPos = Utils.GetAnchoredPosition(textPos, textSize, this.LabelStyleConfig.TextAlign);
                DrawHelpers.DrawInWindow($"##{this.ID}", textPos, textSize, false, true, true, (drawList) =>
                {
                    DrawHelpers.DrawText(
                        drawList,
                        text,
                        textPos,
                        this.LabelStyleConfig.TextColor.Base,
                        this.LabelStyleConfig.ShowOutline,
                        this.LabelStyleConfig.OutlineColor.Base);
                });
            }
        }

        public void SetData(DataSource data)
        {
            _data = data;
        }
    }
}