using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Newtonsoft.Json;
using XIVAuras.Config;
using XIVAuras.Helpers;

namespace XIVAuras.Auras
{
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

        public override IEnumerable<IConfigPage> GetConfigPages()
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

            if (data is null)
            {
                return;
            }

            bool triggered = this.Preview || this.TriggerConfig.IsTriggered(data) && this.VisibilityConfig.IsVisible();

            Vector2 localPos = pos + this.IconStyleConfig.Position;
            Vector2 size = this.IconStyleConfig.Size;

            if (triggered)
            {
                this.UpdateStartData(data, this.TriggerConfig.TriggerType);

                bool continueDrag = this.LastFrameWasDragging && ImGui.IsMouseDown(ImGuiMouseButton.Left);
                bool hovered = ImGui.IsMouseHoveringRect(localPos, localPos + size);
                bool setPos = this.Preview && !this.LastFrameWasPreview || !hovered;
                DrawHelpers.DrawInWindow($"##{this.ID}", localPos, size, this.Preview, false, this.Preview, setPos && !continueDrag, (drawList) =>
                {
                    if (this.Preview)
                    {
                        data = this.UpdatePreviewData(data);
                        this.LastFrameWasDragging = hovered || continueDrag;
                        if (this.LastFrameWasDragging)
                        {
                            localPos = ImGui.GetWindowPos();
                            this.IconStyleConfig.Position = localPos - pos;
                        }
                    }

                    bool crop = this.IconStyleConfig.CropIcon && this.TriggerConfig.TriggerType != TriggerType.Cooldown;
                    DrawHelpers.DrawIcon(this.TriggerConfig.GetIcon(), localPos, size, crop, 0, drawList);

                    if (this.StartData is not null)
                    {
                        float triggeredValue = this.TriggerConfig.TriggerType == TriggerType.Cooldown && this.StartData.ChargeTime > 0
                            ? data.Value % this.StartData.ChargeTime
                            : data.Value;

                        float startValue = this.TriggerConfig.TriggerType == TriggerType.Cooldown && this.StartData.ChargeTime > 0
                            ? this.StartData.ChargeTime
                            : this.StartData.Value;

                        data.Value = triggeredValue;
                        if (this.TriggerConfig.TriggerType == TriggerType.Cooldown && triggeredValue == 0)
                        {
                            data.Stacks = this.StartData.MaxStacks;
                        }

                        if (this.IconStyleConfig.ShowProgressSwipe)
                        {
                            this.DrawProgressSwipe(localPos, size, triggeredValue, startValue, drawList);
                        }
                    }

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
            else
            {
                this.StartData = null;
                this.StartTime = null;
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
                    label.SetData(data);
                    label.Draw(localPos, size);
                }
            }

            this.LastFrameWasPreview = this.Preview;
        }

        private void DrawProgressSwipe(Vector2 pos, Vector2 size, float triggeredValue, float startValue, ImDrawListPtr drawList)
        {
            if (startValue > 0)
            {
                bool invert = this.IconStyleConfig.InvertSwipe;
                float percent = (invert ? 0 : 1) - (startValue - triggeredValue) / startValue;

                float radius = (float)Math.Sqrt(Math.Pow(Math.Max(size.X, size.Y), 2) * 2) / 2f;
                float startAngle = -(float)Math.PI / 2;
                float endAngle = startAngle - 2f * (float)Math.PI * percent;

                ImGui.PushClipRect(pos, pos + size, false);
                drawList.PathArcTo(pos + size / 2, radius / 2, startAngle, endAngle, (int)(100f * Math.Abs(percent)));
                drawList.PathStroke((uint)(this.IconStyleConfig.ProgressSwipeOpacity * 255) << 24, ImDrawFlags.None, radius);
                if (this.IconStyleConfig.ShowSwipeLines)
                {
                    Vector2 vec = new Vector2((float)Math.Cos(endAngle), (float)Math.Sin(endAngle));
                    Vector2 start = pos + size / 2;
                    Vector2 end = start + vec * radius;
                    float thickness = this.IconStyleConfig.ProgressLineThickness;
                    uint color = this.IconStyleConfig.ProgressLineColor.Base;

                    drawList.AddLine(start, end, color, thickness);
                    drawList.AddLine(start, new(pos.X + size.X / 2, pos.Y), color, thickness);
                    drawList.AddCircleFilled(start + new Vector2(thickness / 4, thickness / 4), thickness / 2, color);
                }

                ImGui.PopClipRect();
            }
        }
    }
}