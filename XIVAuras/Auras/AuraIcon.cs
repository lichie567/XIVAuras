using System.Collections.Generic;
using System.Numerics;
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

            if (data.HasValue && this.TriggerConfig.IsTriggered(data.Value) && this.VisibilityConfig.IsVisible())
            {
                pos += this.IconStyleConfig.Position;
                Vector2 size = this.IconStyleConfig.Size;
                DrawHelpers.DrawInWindow($"AuraIcon_{Name}", pos, size, false, false, (drawList) =>
                {
                    // draw red square until icons sorted out
                    drawList.AddRectFilled(pos, pos + size, 0xFF0000FF);

                    if (this.IconStyleConfig.ShowBorder)
                    {
                        drawList.AddRect(pos, pos + size, this.IconStyleConfig.BorderColor.Base);
                    }
                });

                foreach (AuraLabel label in IconStyleConfig.AuraLabels)
                {
                    label.SetData(data.Value);
                    label.Draw(pos);
                }
            }
        }
    }
}