using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using XIVAuras.Config;
using XIVAuras.Helpers;

namespace XIVAuras.Auras
{
    public class AuraIcon : AuraListItem
    {
        public override AuraType Type => AuraType.Icon;

        public IconStyleConfig IconStyleConfig { get; set; }
        public LabelListConfig LabelListConfig { get; set; }
        public TriggerConfig TriggerConfig { get; set; }
        public VisibilityConfig VisibilityConfig { get; set; }

        // Constructor for deserialization
        public AuraIcon() : this(string.Empty) { }

        public AuraIcon(string name) : base(name)
        {
            this.IconStyleConfig = new IconStyleConfig();
            this.LabelListConfig = new LabelListConfig();
            this.TriggerConfig = new TriggerConfig();
            this.VisibilityConfig = new VisibilityConfig();
        }

        public override IEnumerable<IConfigPage> GetConfigPages()
        {
            yield return this.IconStyleConfig;
            yield return this.LabelListConfig;
            yield return this.TriggerConfig;
            yield return this.VisibilityConfig;
        }

        public override void ImportPage(IConfigPage page)
        {
            switch (page)
            {
                case IconStyleConfig newPage:
                    this.IconStyleConfig = newPage;
                    break;
                case LabelListConfig newPage:
                    this.LabelListConfig = newPage;
                    break;
                case TriggerConfig newPage:
                    this.TriggerConfig = newPage;
                    break;
                case VisibilityConfig newPage:
                    this.VisibilityConfig = newPage;
                    break;
            }
        }

        public override void Draw(Vector2 pos, Vector2? parentSize = null)
        {
            if (!this.Preview &&
                (!this.TriggerConfig.TriggerOptions.Any() ||
                !this.VisibilityConfig.IsVisible()))
            {
                return;
            }

            Vector2 localPos = pos + this.IconStyleConfig.Position;
            Vector2 size = this.IconStyleConfig.Size;

            bool triggered = this.TriggerConfig.IsTriggered(this.Preview, out DataSource data);

            if (triggered)
            {
                this.UpdateStartData(data);
                this.UpdateDragData(localPos, size);

                DrawHelpers.DrawInWindow($"##{this.ID}", localPos, size, this.Preview, this.SetPosition, (drawList) =>
                {
                    if (this.Preview)
                    {
                        data = this.UpdatePreviewData(data);
                        if (this.LastFrameWasDragging)
                        {
                            localPos = ImGui.GetWindowPos();
                            this.IconStyleConfig.Position = localPos - pos;
                        }
                    }
                    
                    ushort icon = this.IconStyleConfig.IconOption switch
                    {
                        0 => data.Icon,
                        1 => this.IconStyleConfig.CustomIcon,
                        _ => 0
                    };

                    if (icon > 0)
                    {
                        bool desaturate = this.IconStyleConfig.DesaturateIcon;
                        float alpha = this.IconStyleConfig.Opacity;

                        DrawHelpers.DrawIcon(icon, localPos, size, this.IconStyleConfig.CropIcon, 0, desaturate, alpha, drawList);

                        if (this.StartData is not null && this.IconStyleConfig.ShowProgressSwipe)
                        {
                            this.DrawProgressSwipe(localPos, size, data.Value, this.StartData.Value, alpha, drawList);
                        }

                        if (this.IconStyleConfig.ShowBorder)
                        {
                            for (int i = 0; i < this.IconStyleConfig.BorderThickness; i++)
                            {
                                Vector2 offset = new Vector2(i, i);
                                Vector4 color = this.IconStyleConfig.BorderColor.Vector.AddTransparency(alpha);
                                drawList.AddRect(localPos + offset, localPos + size - offset, ImGui.ColorConvertFloat4ToU32(color));
                            }
                        }
                    }
                });
            }
            else
            {
                this.StartData = null;
                this.StartTime = null;
            }

            foreach (AuraLabel label in this.LabelListConfig.AuraLabels)
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

        private void DrawProgressSwipe(Vector2 pos, Vector2 size, float triggeredValue, float startValue, float alpha, ImDrawListPtr drawList)
        {
            if (startValue > 0 && triggeredValue != 0)
            {
                bool invert = this.IconStyleConfig.InvertSwipe;
                float percent = (invert ? 0 : 1) - (startValue - triggeredValue) / startValue;

                float radius = (float)Math.Sqrt(Math.Pow(Math.Max(size.X, size.Y), 2) * 2) / 2f;
                float startAngle = -(float)Math.PI / 2;
                float endAngle = startAngle - 2f * (float)Math.PI * percent;

                ImGui.PushClipRect(pos, pos + size, false);
                drawList.PathArcTo(pos + size / 2, radius / 2, startAngle, endAngle, (int)(100f * Math.Abs(percent)));
                uint progressAlpha = (uint)(this.IconStyleConfig.ProgressSwipeOpacity * 255 * alpha) << 24;
                drawList.PathStroke(progressAlpha, ImDrawFlags.None, radius);
                if (this.IconStyleConfig.ShowSwipeLines)
                {
                    Vector2 vec = new Vector2((float)Math.Cos(endAngle), (float)Math.Sin(endAngle));
                    Vector2 start = pos + size / 2;
                    Vector2 end = start + vec * radius;
                    float thickness = this.IconStyleConfig.ProgressLineThickness;
                    Vector4 swipeLineColor = this.IconStyleConfig.ProgressLineColor.Vector.AddTransparency(alpha);
                    uint color = ImGui.ColorConvertFloat4ToU32(swipeLineColor);

                    drawList.AddLine(start, end, color, thickness);
                    drawList.AddLine(start, new(pos.X + size.X / 2, pos.Y), color, thickness);
                    drawList.AddCircleFilled(start + new Vector2(thickness / 4, thickness / 4), thickness / 2, color);
                }

                ImGui.PopClipRect();
            }
        }

        public static AuraIcon GetDefaultAuraIcon(string name)
        {
            AuraIcon newIcon = new AuraIcon(name);
            newIcon.ImportPage(newIcon.LabelListConfig.GetDefault());
            return newIcon;
        }
    }
}