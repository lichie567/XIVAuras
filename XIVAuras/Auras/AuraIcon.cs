using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using Newtonsoft.Json;
using XIVAuras.Config;
using XIVAuras.Helpers;

namespace XIVAuras.Auras
{
    [JsonObject]
    public class AuraIcon : AuraListItem
    {
        public override AuraType Type => AuraType.Icon;

        public IconStyleConfig IconStyleConfig { get; init; }

        public TriggerConfig TriggerConfig { get; init; }

        public VisibilityConfig VisibilityConfig { get; init; }

        // Constructor for deserialization
        public AuraIcon() : this(string.Empty) { }

        public AuraIcon(string name) : base(name)
        {
            this.Name = name;
            this.IconStyleConfig = new IconStyleConfig();
            this.TriggerConfig = new TriggerConfig();
            this.VisibilityConfig = new VisibilityConfig();
        }

        public override IEnumerator<IConfigPage> GetEnumerator()
        {
            yield return this.IconStyleConfig;
            yield return this.TriggerConfig;
            yield return this.VisibilityConfig;
        }

        public override void Draw(Vector2 pos)
        {
            uint statusId = this.TriggerConfig.StatusId;
            DataSource? data = SpellHelpers.GetData(this.TriggerConfig.TriggerSource, statusId);

            if (this.Preview)
            {
                data = new DataSource()
                {
                    Duration = 10
                };
            }

            if (data.HasValue &&
                (this.Preview ||
                 this.TriggerConfig.IsTriggered(data.Value) &&
                 this.VisibilityConfig.IsVisible()))
            {
                Vector2 localPos = pos + this.IconStyleConfig.Position;
                Vector2 size = this.IconStyleConfig.Size;

                DrawHelpers.DrawInWindow($"##{this.ID}", localPos, size, this.Preview, this.LastFrameWasPreview, (drawList) =>
                {
                    if (this.Preview)
                    {
                        localPos = ImGui.GetWindowPos();
                        this.IconStyleConfig.Position = localPos - pos;
                    }

                    this.LastFrameWasPreview = this.Preview;

                    // draw red square until icons sorted out
                    drawList.AddRectFilled(localPos, localPos + size, 0xFF0000FF);

                    if (this.IconStyleConfig.ShowBorder)
                    {
                        drawList.AddRect(localPos, localPos + size, this.IconStyleConfig.BorderColor.Base);
                    }
                });

                foreach (AuraLabel label in IconStyleConfig.AuraLabels)
                {
                    label.SetData(data.Value);
                    label.Draw(localPos);
                }
            }
        }
    }
}