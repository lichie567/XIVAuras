using System.Collections.Generic;
using System.Linq;
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

        public override void Draw(Vector2 pos, Vector2? parentSize = null)
        {
            if (!this.TriggerConfig.TriggerList.Any())
            {
                return;
            }

            DataSource? data = SpellHelpers.GetData(
                this.TriggerConfig.TriggerSource,
                this.TriggerConfig.TriggerType,
                this.TriggerConfig.TriggerList,
                this.TriggerConfig.ShowOnlyMine,
                this.Preview);

            if (!data.HasValue)
            {
                return;
            }

            bool triggered = this.Preview || this.TriggerConfig.IsTriggered(data.Value) && this.VisibilityConfig.IsVisible();

            Vector2 localPos = pos + this.IconStyleConfig.Position;
            Vector2 size = this.IconStyleConfig.Size;

            if (triggered)
            {
                bool continueDrag = this.LastFrameWasDragging && ImGui.IsMouseDown(ImGuiMouseButton.Left);
                bool hovered = ImGui.IsMouseHoveringRect(localPos, localPos + size);
                bool setPos = this.Preview && !this.LastFrameWasPreview || !hovered;
                DrawHelpers.DrawInWindow($"##{this.ID}", localPos, size, this.Preview, false, this.Preview, setPos && !continueDrag, (drawList) =>
                {
                    if (this.Preview)
                    {
                        this.LastFrameWasDragging = hovered && ImGui.IsMouseDown(ImGuiMouseButton.Left) || continueDrag;
                        if (this.LastFrameWasDragging)
                        {
                            localPos = ImGui.GetWindowPos();
                            this.IconStyleConfig.Position = localPos - pos;
                        }
                    }

                    DrawHelpers.DrawIcon(this.TriggerConfig.GetIcon(), localPos, size, this.IconStyleConfig.CropIcon, 0, drawList);

                    if (this.IconStyleConfig.ShowBorder)
                    {
                        for (int i = 0; i < this.IconStyleConfig.BorderThickness; i++)
                        {
                            Vector2 offset = new Vector2(i, i);
                            drawList.AddRect(localPos + offset, localPos + size - offset, this.IconStyleConfig.BorderColor.Base);
                        }
                    }
                });
            }

            foreach (AuraLabel label in IconStyleConfig.AuraLabels)
            {
                if (!this.Preview && this.LastFrameWasPreview)
                {
                    label.Preview = false;
                }
                else
                {
                    label.Preview |= this.Preview;
                }

                if (triggered || label.Preview)
                {
                    label.SetData(data.Value);
                    label.Draw(localPos, size);
                }
            }

            this.LastFrameWasPreview = this.Preview;
        }
    }
}